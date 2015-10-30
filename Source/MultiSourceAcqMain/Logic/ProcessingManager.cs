// MultiSourceAcquisition (MSA)
// Copyright (c) 2015 Filip Malawski
// AGH University of Science and Technology, Cracow, Poland
// Contact: fmal@agh.edu.pl
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video.FFMPEG;
using Microsoft.Kinect;

namespace MultiSourceAcqMain.Logic
{
    public class ProcessingManager
    {
        #region Singleton

        private static ProcessingManager instance;
        public static ProcessingManager Instance { get { return instance ?? (instance = new ProcessingManager()); } }
        private ProcessingManager() { }

        #endregion

        private bool isProcessing = true;
        private bool isRecording = false;

        private ConcurrentQueue<MultiFrame> queueMultiFramesInput = new ConcurrentQueue<MultiFrame>();
        private ConcurrentQueue<MultiFrame> queueMultiFramesProcessed = new ConcurrentQueue<MultiFrame>();
        private ConcurrentQueue<MultiFrame> queueMultiFramesRecording = new ConcurrentQueue<MultiFrame>();
        private List<MultiFrame> lstMultiFramesSorted = new List<MultiFrame>();      // only one thread has access -> no sync needed
        private CoordinateMapper coordMapper;

        private int colorWidth;
        private int colorHeight;
        private int depthWidth;
        private int depthHeight;
        private int colorResizedHeight;
        private int colorResizedWidth;

        private int colorByteSize;
        private int colorPixelSize;
        private int depthByteSize;
        private int colorResizedByteSize;
        private int colorResizedPixelSize;

        private int psWidth;
        private int psHeight;

        private const int depthToByte = 8000 / 256;         // for 8bit depth

        // processing & recording
        private VideoFileWriter videoWriterKinectColor;
        private VideoFileWriter videoWriterKinectDepth;
        private VideoFileWriter videoWriterPS3Eye;
        private object lockObj = new object();        
        private int processingSleepTime = 5;                // ms
        private bool setNextFrameAsLastToRecord = false;    // flag when to finish
        private long nextFrame;
        private List<float[]> bufferBody;
        private string currFilePathBase;
        private RecorderPanel recorderPanel;
        private Dictionary<string, VideoCodec> dctCodecs;
        private List<string> filesLastRecording = new List<string>();

        // --- init & enqueue
        public void Init()
        {
            // sizes & context
            coordMapper = Context.KinectSensor.CoordinateMapper;
            colorWidth = Context.ColorWidth;
            colorHeight = Context.ColorHeight;
            depthWidth = Context.DepthWidth;
            depthHeight = Context.DepthHeight;
            colorResizedWidth = Context.ColorResizedWidth;
            colorResizedHeight = Context.ColorResizedHeight;
            
            colorByteSize = colorWidth * colorHeight * 4;
            colorPixelSize = colorWidth * colorHeight;
            depthByteSize = depthWidth * depthHeight;
            colorResizedByteSize = colorResizedWidth * colorResizedHeight * 4;
            colorResizedPixelSize = colorResizedWidth * colorResizedHeight;

            psWidth = Context.PsWidth;
            psHeight = Context.PsHeight;

            recorderPanel = Context.GUI.RecorderPanel;

            // codecs
            dctCodecs = new Dictionary<string, VideoCodec>();
            dctCodecs.Add(VideoCodec.H263P.ToString(), VideoCodec.H263P);
            dctCodecs.Add(VideoCodec.MPEG2.ToString(), VideoCodec.MPEG2);
            dctCodecs.Add(VideoCodec.MPEG4.ToString(), VideoCodec.MPEG4);
            dctCodecs.Add(VideoCodec.Raw.ToString(), VideoCodec.Raw);
            dctCodecs.Add(VideoCodec.WMV1.ToString(), VideoCodec.WMV1);
            dctCodecs.Add(VideoCodec.WMV2.ToString(), VideoCodec.WMV2);
        }
        public void EnqueueMultiFrame(MultiFrame multiFrame)
        {            
            if (setNextFrameAsLastToRecord)
            {
                setNextFrameAsLastToRecord = false;
                multiFrame.IsLastRecorded = true;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(o => { ProcessFrame(multiFrame); }));
        }

        #region Processing

        public void StartProcessing()
        {
            BackgroundWorker bgWorkerInput = new BackgroundWorker();
            bgWorkerInput.DoWork += (s, e) =>
            {
                // loop
                while (isProcessing)
                {
                    // --- processed queue
                    MultiFrame multiFrameProcessed;
                    if (queueMultiFramesProcessed.TryDequeue(out multiFrameProcessed))
                    {
                        // insert into list (will be sorted later)
                        lstMultiFramesSorted.Add(multiFrameProcessed);
                        Utils.SetQueueCount("Sorted", lstMultiFramesSorted.Count);
                        //multiFrameProcessed = null;
                    }
                    else
                    {
                        Thread.Sleep(processingSleepTime);
                    }

                    // --- sorted list
                    lstMultiFramesSorted.Sort(new Comparison<MultiFrame>((frameA, frameB) => frameA.FrameNb.CompareTo(frameB.FrameNb)));
                    while (lstMultiFramesSorted.Count > 0 && lstMultiFramesSorted[0].FrameNb == nextFrame)
                    {                        
                        // enqueue for recording
                        MultiFrame multiFrameRecording = lstMultiFramesSorted[0];
                        if (isRecording)
                        {
                            queueMultiFramesRecording.Enqueue(multiFrameRecording);
                            Utils.SetQueueCount("Recording", queueMultiFramesRecording.Count);
                        }
                        if (multiFrameRecording.IsLastRecorded) isRecording = false;                       // !!
                        lstMultiFramesSorted.RemoveAt(0);
                        nextFrame++;

                        //multiFrameRecording.Dispose();
                        //GC.Collect();
                    }
                }
            };
            bgWorkerInput.RunWorkerAsync();
        }
        public void StopProcessing()
        {
            isProcessing = false;
        }
        private void ProcessFrame(MultiFrame multiFrame)
        {
            long ticksProcessFrame = DateTime.Now.Ticks;

            if (multiFrame.HasKinectData)
            {
                // --- color
                // color mapping
                long ticksMappingColor = DateTime.Now.Ticks;
                if (Context.ProcessColorMapping) ProcessColorMapping(multiFrame);
                Utils.UpdateTimer("ColorMapping", ticksMappingColor);

                // color resize
                long ticksColorResize = DateTime.Now.Ticks;
                if (Context.ProcessColorResize) ProcessColorResize(multiFrame);
                Utils.UpdateTimer("ColorResize", ticksColorResize);

                // color bitmap (for recording -> not for display)
                long ticksColorBitmap = DateTime.Now.Ticks;
                if (Context.ProcessColorBitmap) ProcessColorBitmap(multiFrame);
                Utils.UpdateTimer("ColorBitmap", ticksColorBitmap);

                // --- depth
                // depth data (depth data & bodyIndexData)
                long ticksDepthData = DateTime.Now.Ticks;
                if (Context.ProcessDepthData) ProcessDepthData(multiFrame);
                Utils.UpdateTimer("DepthData", ticksDepthData);

                // depth mapping
                long ticksMappingDepth = DateTime.Now.Ticks;
                if (Context.ProcessDepthMapping) ProcessDepthMapping(multiFrame);
                Utils.UpdateTimer("DepthMapping", ticksMappingDepth);

                // depth bitmap (for recording -> not for display)
                long ticksDepthBitmap = DateTime.Now.Ticks;
                if (Context.ProcessDepthBitmap) ProcessDepthBitmap(multiFrame);
                Utils.UpdateTimer("DepthBitmap", ticksDepthBitmap);

                // --- body
                long ticksBody = DateTime.Now.Ticks;
                if (Context.ProcessBody) ProcessBodyData(multiFrame);
                Utils.UpdateTimer("Body", ticksBody);
            }
            if (multiFrame.HasPS3EyeData)
            {
                // --- ps3eye
                long ticksPS3EyeBitmap = DateTime.Now.Ticks;
                if (Context.ProcessPS3EyeBitmap) ProcessPS3EyeBitmap(multiFrame);
                Utils.UpdateTimer("PS3EyeBitmap", ticksPS3EyeBitmap);
            }

            // --- display & enqueue
            ThreadPool.QueueUserWorkItem((object state) => Context.GUI.DisplayBitmaps(multiFrame));
            queueMultiFramesProcessed.Enqueue(multiFrame);
            Utils.SetQueueCount("Processed", queueMultiFramesProcessed.Count);
            Utils.UpdateTimer("ProcessFrame", ticksProcessFrame);
            Utils.UpdateCounter("Processed");
        }
        
        // --- color
        private void ProcessColorMapping(MultiFrame multiFrame)
        {
            // --- color mapping from kinect
            long ticks1 = DateTime.Now.Ticks;
            DepthSpacePoint[] colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];
            coordMapper.MapColorFrameToDepthSpace(multiFrame.DepthData, colorMappedToDepthPoints);            
            Utils.UpdateTimer("(ColorMappingCoord)", ticks1);

            // --- mapped colorAsDepth -> depth
            long ticks2 = DateTime.Now.Ticks;
            byte[] colorMappedBytes = new byte[colorWidth * colorHeight * 4];

            unsafe
            {
                fixed (byte* fixedColorMapped = colorMappedBytes)
                fixed (ushort* fixedDepthData = multiFrame.DepthData)
                fixed (byte* fixedBodyIndexData = multiFrame.BodyIndexData)
                {
                    byte* ptrColorMapped = fixedColorMapped;
                    ushort* ptrDepthData = fixedDepthData;
                    byte* ptrBodyIndexData = fixedBodyIndexData;

                    // 8 bit
                    if (Context.Use8bitDepth)
                    {
                        for (int i = 0; i < colorPixelSize; i++)                                               
                        {
                            // checking infinity before adding + 0.5f is about 5x faster (!!)
                            float xTmp = colorMappedToDepthPoints[i].X;
                            float yTmp = colorMappedToDepthPoints[i].Y;

                            int x = float.IsInfinity(xTmp) ? -1 : (int)(xTmp + 0.5f);
                            int y = float.IsInfinity(yTmp) ? -1 : (int)(yTmp + 0.5f);

                            if (x >= 0 && x < depthWidth && y >= 0 && y < depthHeight)
                            {
                                int idx = x + y * depthWidth;
                                byte val = (ptrBodyIndexData[idx] < 6 ? byte.MaxValue : (byte)(ptrDepthData[idx] / depthToByte));
                                *ptrColorMapped++ = val;
                                *ptrColorMapped++ = val;
                                *ptrColorMapped++ = val;
                                *ptrColorMapped++ = 255;            // alpha
                            }
                            else
                            {
                                ptrColorMapped += 4;                // 0 is default
                            }
                        }
                    }
                    // full depth (13 bit)
                    else
                    {
                        for (int i = 0; i < colorPixelSize; i++)
                        {
                            // checking infinity before adding + 0.5f is about 5x faster (!!)
                            float xTmp = colorMappedToDepthPoints[i].X;
                            float yTmp = colorMappedToDepthPoints[i].Y;

                            int x = float.IsInfinity(xTmp) ? -1 : (int)(xTmp + 0.5f);
                            int y = float.IsInfinity(yTmp) ? -1 : (int)(yTmp + 0.5f);

                            if (x >= 0 && x < depthWidth && y >= 0 && y < depthHeight)
                            {
                                int idx = x + y * depthWidth;
                                ushort val = ptrDepthData[idx];
                                *ptrColorMapped++ = (byte)(val % 256);
                                *ptrColorMapped++ = (byte)(val / 256);
                                *ptrColorMapped++ = ptrBodyIndexData[idx];
                                *ptrColorMapped++ = 255;            // alpha
                            }
                            else
                            {
                                ptrColorMapped += 4;                // 0 is default
                            }
                        }
                    }
                }
            }            
            multiFrame.ColorMappedBytes = colorMappedBytes;
            Utils.UpdateTimer("(ColorMappingBytes)", ticks2);
        }
        private void ProcessColorResize(MultiFrame multiFrame)
        {            
            // --- resize color input
            long ticksResize = DateTime.Now.Ticks;

            byte[] colorResizedData;
            if (Context.ProcessColorMapping)
            {
                colorResizedData = new byte[colorResizedWidth * colorResizedHeight * 4 * 2];  // joined -> height x2
            }
            else
            {
                colorResizedData = new byte[colorResizedWidth * colorResizedHeight * 4];
            }
            multiFrame.ColorResizedData = colorResizedData;

            BitmapSource bmpSrc = Utils.ToBitmapSource(multiFrame.ColorData, colorWidth, colorHeight, PixelFormats.Bgr32);

            // resize to 640x360
            //BitmapSource bmpSrcResized = Utils.ResizeBitmapSource(bmpSrc, colorResizedWidth, colorResizedHeight, Context.ResizeQuality);
            //bmpSrcResized.CopyPixels(colorResizedData, colorResizedWidth * 4, 0);

            // resize to 640x480
            BitmapSource bmpSrcResized = Utils.ResizeBitmapSource(bmpSrc, Context.ColorResizedWidthTmp, colorResizedHeight, Context.ResizeQuality);
            byte[] colorResizedDataTmp = new byte[Context.ColorResizedWidthTmp * colorResizedHeight * 4];
            bmpSrcResized.CopyPixels(colorResizedDataTmp, Context.ColorResizedWidthTmp * 4, 0);

            unsafe
            {
                fixed (byte* fixedColorResizedData = colorResizedData)
                fixed (byte* fixedColorResizedDataTmp = colorResizedDataTmp)
                {
                    byte* ptrDest = fixedColorResizedData;
                    byte* ptrSrc = fixedColorResizedDataTmp;
                    ptrSrc += 107 * 4;
                    for (int h = 0; h < colorResizedHeight; h++)
                    {
                        for (int w = 0; w < colorResizedWidth; w++)
                        {
                            *ptrDest++ = *ptrSrc++;
                            *ptrDest++ = *ptrSrc++;
                            *ptrDest++ = *ptrSrc++;
                            *ptrDest++ = *ptrSrc++;
                        }
                        ptrSrc += 213 * 4;
                    }
                }
            }
            Utils.UpdateTimer("(ColorResizeInput)", ticksResize);

            // --- resize color mapped
            long ticksResizeMapped = DateTime.Now.Ticks;

            if (multiFrame.ColorMappedBytes != null)
            {
                bmpSrc = Utils.ToBitmapSource(multiFrame.ColorMappedBytes, colorWidth, colorHeight, PixelFormats.Bgr32);

                // resize to 640x360
                //bmpSrcResized = Utils.ResizeBitmapSource(bmpSrc, colorResizedWidth, colorResizedHeight, Context.ResizeQuality);
                //bmpSrcResized.CopyPixels(colorResizedData, colorResizedWidth * 4, colorResizedByteSize);

                // resize to 640x480
                bmpSrcResized = Utils.ResizeBitmapSource(bmpSrc, Context.ColorResizedWidthTmp, colorResizedHeight, Context.ResizeQuality);
                colorResizedDataTmp = new byte[Context.ColorResizedWidthTmp * colorResizedHeight * 4];
                bmpSrcResized.CopyPixels(colorResizedDataTmp, Context.ColorResizedWidthTmp * 4, 0);

                unsafe
                {
                    fixed (byte* fixedColorResizedData = colorResizedData)
                    fixed (byte* fixedColorResizedDataTmp = colorResizedDataTmp)
                    {
                        byte* ptrDest = fixedColorResizedData + colorResizedByteSize;
                        byte* ptrSrc = fixedColorResizedDataTmp;
                        ptrSrc += 107 * 4;
                        for (int h = 0; h < colorResizedHeight; h++)
                        {
                            for (int w = 0; w < colorResizedWidth; w++)
                            {
                                *ptrDest++ = *ptrSrc++;
                                *ptrDest++ = *ptrSrc++;
                                *ptrDest++ = *ptrSrc++;
                                *ptrDest++ = *ptrSrc++;
                            }
                            ptrSrc += 213 * 4;
                        }
                    }
                }
            }

            Utils.UpdateTimer("(ColorResizeMapped)", ticksResizeMapped);
        }
        private void ProcessColorBitmap(MultiFrame multiFrame)
        {
            if (Context.ProcessColorMapping)
            {
                multiFrame.ColorResizedBitmap = Utils.CreateBitmap(multiFrame.ColorResizedData,
                    colorResizedWidth, colorResizedHeight * 2, colorResizedWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            else
            {
                multiFrame.ColorResizedBitmap = Utils.CreateBitmap(multiFrame.ColorResizedData,
                    colorResizedWidth, colorResizedHeight, colorResizedWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
        }

        // --- depth
        private void ProcessDepthData(MultiFrame multiFrame)
        {            
            byte[] depthBytes = new byte[depthWidth * depthHeight * 4 * 2];     // joined -> height x2
            int len = multiFrame.DepthData.Length;
            unsafe
            {
                fixed (ushort* fixedDepthData = multiFrame.DepthData)
                fixed (byte* fixedBodyIndexData = multiFrame.BodyIndexData)
                fixed (byte* fixedDepthBytes = depthBytes) 
                {
                    ushort* ptrDepthData = fixedDepthData;
                    byte* ptrBodyIndexData = fixedBodyIndexData;
                    byte* ptrDepthBytes = fixedDepthBytes;

                    // 8 bit depth
                    if (Context.Use8bitDepth)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            // this is to show full depth resolution in selected range
                            //byte val = 0;
                            //ushort uVal = *ptrDepthData;
                            //if (uVal > 1000 && uVal < 1255) val = (byte)(uVal - 1000);

                            // this is full range but reduced depth resolution
                            byte val = (*ptrBodyIndexData < 6 ? byte.MaxValue : (byte)(*ptrDepthData / depthToByte));                            

                            *ptrDepthBytes++ = val;
                            *ptrDepthBytes++ = val;
                            *ptrDepthBytes++ = val;
                            *ptrDepthBytes++ = byte.MaxValue;        // alpha channel

                            ptrDepthData++;
                            ptrBodyIndexData++;
                        }
                    }
                    // full depth (13 bit)
                    else
                    {
                        for (int i = 0; i < len; i++)
                        {
                            // optimized for raw (faster processing)
                            *ptrDepthBytes++ = (byte)(*ptrDepthData % byte.MaxValue);
                            *ptrDepthBytes++ = (byte)(*ptrDepthData / byte.MaxValue);
                            
                            // body & alpha
                            *ptrDepthBytes++ = (*ptrBodyIndexData < 6 ? *ptrBodyIndexData : byte.MaxValue);         // assume one person only !!
                            *ptrDepthBytes++ = byte.MaxValue;        // alpha channel

                            ptrDepthData++;
                            ptrBodyIndexData++;
                        }
                    }
                }
            }

            multiFrame.DepthBytes = depthBytes;    // !! (otherwise GC will remove it before saving bitmap to disk)            
        }
        private void ProcessDepthMapping(MultiFrame multiFrame)
        {
            // if depth data was not run
            if (multiFrame.DepthBytes == null) multiFrame.DepthBytes = new byte[depthWidth * depthHeight * 4 * 2];

            // --- depth mapping from kinect
            long ticks1 = DateTime.Now.Ticks;
            ColorSpacePoint[] depthMapping = new ColorSpacePoint[depthWidth * depthHeight];
            coordMapper.MapDepthFrameToColorSpace(multiFrame.DepthData, depthMapping);
            Utils.UpdateTimer("(DepthMappingCoord)", ticks1);

            // --- mapped depthAsColor -> color
            long ticks2 = DateTime.Now.Ticks;            
            unsafe
            {
                fixed (byte* fixedDepthMapped = multiFrame.DepthBytes)
                fixed (byte* fixedColorData = multiFrame.ColorData)
                {
                    byte* ptrDepthMapped = fixedDepthMapped;
                    ptrDepthMapped += depthByteSize * 4;                            // joined
                    byte* ptrColorData = fixedColorData;

                    int countMapped = 0;
                    int countNotMapped = 0;
                    for (int i = 0; i < depthByteSize; i++)
                    {
                        // checking infinity before adding + 0.5f is about 5x faster (!!)
                        float xTmp = depthMapping[i].X;
                        float yTmp = depthMapping[i].Y;

                        int x = float.IsInfinity(xTmp) ? -1 : (int)(xTmp + 0.5f);
                        int y = float.IsInfinity(yTmp) ? -1 : (int)(yTmp + 0.5f);

                        if (x >= 0 && x < colorWidth && y >= 0 && y < colorHeight)
                        {                            
                            int idxBase = x * 4 + y * colorWidth * 4;
                            *ptrDepthMapped++ = ptrColorData[idxBase];
                            *ptrDepthMapped++ = ptrColorData[idxBase + 1];
                            *ptrDepthMapped++ = ptrColorData[idxBase + 2];
                            *ptrDepthMapped++ = ptrColorData[idxBase + 3];
                            countMapped++;
                        }
                        else
                        {
                            *ptrDepthMapped++ = 0;
                            *ptrDepthMapped++ = 0;
                            *ptrDepthMapped++ = 0;
                            *ptrDepthMapped++ = 0;
                            countNotMapped++;
                        }
                    }
                }                
            }                                    
            Utils.UpdateTimer("(DepthMappingBytes)", ticks2);            
        }
        private void ProcessDepthBitmap(MultiFrame multiFrame)
        {
            if (Context.ProcessDepthMapping)
            {
                multiFrame.DepthBitmap = Utils.CreateBitmap(multiFrame.DepthBytes,
                    depthWidth, depthHeight * 2, depthWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            else
            {
                multiFrame.DepthBitmap = Utils.CreateBitmap(multiFrame.DepthBytes,
                    depthWidth, depthHeight, depthWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }            
        }

        // --- body
        private void ProcessBodyData(MultiFrame multiFrame)
        {
            // NOTES (!!)
            // we assume there is only one body 
            // we assume joints are always in the same order
            // 12 values per joint   -> trackingStatus, posX, posY, posZ, depthX, depthY, colorX, colorY, orientX, orientY, orientZ, orientW
            // joint trackingstate enum: tracked = 2, nottracked = 0, inffered = 1?

            Body[] bodies = multiFrame.Bodies;
            Body body = null;
            foreach (var b in bodies)
            {
                if (b.IsTracked)
                {
                    body = b;
                    break;
                }
            }

            if (body == null)
            {
                multiFrame.BodyData = new float[25 * 12];       // return zeros if not found
                return;
            }
            
            int jointsCount = body.Joints.Count;
            float[] bodyData = new float[jointsCount * 12];
            int idx = 0;
            
            foreach (var kvp in body.Joints)            
            {
                var jointType = kvp.Key;        
                var joint = kvp.Value;
                var jointOrientation = body.JointOrientations[jointType];

                CameraSpacePoint position = joint.Position;
                if (position.Z < 0) position.Z = 0.1f;          // according to Kinect code sample (sometimes z < 0 and the mapping return -infinity)

                DepthSpacePoint depthSpacePoint = coordMapper.MapCameraPointToDepthSpace(position);
                ColorSpacePoint colorSpacePoint = coordMapper.MapCameraPointToColorSpace(position);

                bodyData[idx++] = (int)joint.TrackingState;
                bodyData[idx++] = joint.Position.X;
                bodyData[idx++] = joint.Position.Y;
                bodyData[idx++] = joint.Position.Z;
                bodyData[idx++] = depthSpacePoint.X;
                bodyData[idx++] = depthSpacePoint.Y;
                bodyData[idx++] = colorSpacePoint.X;
                bodyData[idx++] = colorSpacePoint.Y;
                bodyData[idx++] = jointOrientation.Orientation.X;
                bodyData[idx++] = jointOrientation.Orientation.Y;
                bodyData[idx++] = jointOrientation.Orientation.Z;
                bodyData[idx++] = jointOrientation.Orientation.W;
            }
            multiFrame.BodyData = bodyData;
        }

        // --- ps3eye
        private void ProcessPS3EyeBitmap(MultiFrame multiFrame)
        {
            multiFrame.PS3EyeBitmap = Utils.CreateBitmap(multiFrame.PS3EyeData,
                psWidth, psHeight * 2, psWidth * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb);         
        }

        #endregion

        #region Recording

        public List<string> GetListOfCodecs()
        {
            List<string> lstCodecs = new List<string>();
            lstCodecs.AddRange(dctCodecs.Keys);
            return lstCodecs;
        }

        public void StartRecording()
        {            
            string filePathBase = Utils.GetFilePathBase(DateTime.Now, recorderPanel);

            VideoCodec codec = dctCodecs[recorderPanel.Codec];
            int bitrate = recorderPanel.Bitrate;
            bitrate *= 1024;                // b -> kb

            OpenVideoWriters(filePathBase, codec, bitrate);
            isRecording = true;
            RunRecording();
        }
        public void StopRecording()
        {            
            setNextFrameAsLastToRecord = true;      // processing thread sets isRecording = false and closes the writers            
        }

        private void RunRecording()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) =>
            {
                while (true)
                {
                    MultiFrame multiFrame;
                    if (queueMultiFramesRecording.TryDequeue(out multiFrame))
                    {                        
                        AddFrame(multiFrame);                        
                        if (multiFrame.IsLastRecorded) break;                              // !!
                    }
                    else
                    {
                        Thread.Sleep(processingSleepTime);
                    }
                }
                CloseVideoWriters();                                                        // !!
            };
            bgWorker.RunWorkerAsync();
        }

        private void OpenVideoWriters(string filePathBase, VideoCodec codec, int bitrate)
        {
            lock (lockObj)
            {
                if (Context.IsKinectActive)
                {
                    // color
                    videoWriterKinectColor = new VideoFileWriter();                   
                    int colorWidth = Context.ColorResizedWidth;
                    int colorHeight = Context.ColorResizedHeight;
                    if (Context.ProcessColorMapping) colorHeight *= 2;                                       
                    videoWriterKinectColor.Open(filePathBase + "_Color.avi", colorWidth, colorHeight, 30, codec, bitrate);                        

                    //depth
                    videoWriterKinectDepth = new VideoFileWriter();
                    int depthWidth = Context.DepthWidth;
                    int depthHeight = Context.DepthHeight;
                    if (Context.ProcessDepthMapping) depthHeight *= 2;
                    videoWriterKinectDepth.Open(filePathBase + "_Depth.avi", depthWidth, depthHeight, 30, VideoCodec.Raw, bitrate);                    

                    // body
                    bufferBody = new List<float[]>();
                }
                if (Context.IsPS3EyeActive)
                {
                    int fps = Context.IsKinectActive ? 30 : Context.GUI.PS3EyePanel.FrameRate;                  // fps depends on devices !!
                    videoWriterPS3Eye = new VideoFileWriter();
                    videoWriterPS3Eye.Open(filePathBase + "_PS3Eye.avi", 640, 480 * 2, fps, codec, bitrate);
                }

                currFilePathBase = filePathBase;
            }
        }        
        private void AddFrame(MultiFrame multiFrame)
        {
            lock (lockObj)              // sync
            {                                
                if (videoWriterKinectColor != null && videoWriterKinectColor.IsOpen) videoWriterKinectColor.WriteVideoFrame(multiFrame.ColorResizedBitmap);

                if (videoWriterKinectDepth != null && videoWriterKinectDepth.IsOpen) videoWriterKinectDepth.WriteVideoFrame(multiFrame.DepthBitmap);

                if (bufferBody != null) bufferBody.Add(multiFrame.BodyData);
                multiFrame.BodyData = null;            // otherwise it will keep the frame

                if (videoWriterPS3Eye != null && videoWriterPS3Eye.IsOpen) videoWriterPS3Eye.WriteVideoFrame(multiFrame.PS3EyeBitmap);
            }
        }
        private void CloseVideoWriters()
        {
            lock (lockObj)
            {
                if (videoWriterKinectColor != null && videoWriterKinectColor.IsOpen)
                {
                    videoWriterKinectColor.Close();
                    filesLastRecording.Add(currFilePathBase + "_Color.avi");
                }
                if (videoWriterKinectDepth != null && videoWriterKinectDepth.IsOpen)
                {
                    videoWriterKinectDepth.Close();
                    //videoWriterKinectDepthTmp.Close();
                    filesLastRecording.Add(currFilePathBase + "_Depth.avi");
                }
                if (bufferBody != null)
                {
                    float[][] bodyData = bufferBody.ToArray();
                    csmatio.types.MLSingle bodyDataMat = new csmatio.types.MLSingle("bodyData", bodyData);
                    List<csmatio.types.MLArray> lstBodyDataMat = new List<csmatio.types.MLArray>();
                    lstBodyDataMat.Add(bodyDataMat);
                    csmatio.io.MatFileWriter writer = new csmatio.io.MatFileWriter(currFilePathBase + "_Body.mat", lstBodyDataMat, true);
                    bufferBody = null;      // clear memory
                    filesLastRecording.Add(currFilePathBase + "_Body.mat");
                }

                if (videoWriterPS3Eye != null && videoWriterPS3Eye.IsOpen)
                {
                    videoWriterPS3Eye.Close();
                    filesLastRecording.Add(currFilePathBase + "_PS3Eye.avi");
                }
            }
        }

        public void ClearLastRecordingHandle()
        {
            filesLastRecording.Clear();
        }
        public void RemoveLastRecording()
        {
            filesLastRecording.ForEach(file => File.Delete(file));
        }

        #endregion       

        public void Dispose()
        {
            
        }        
    }
}
