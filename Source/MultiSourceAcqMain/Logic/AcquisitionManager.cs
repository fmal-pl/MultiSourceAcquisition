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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;
using MultiSourceAcqMain.PS3Eye;
using MoreLinq;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace MultiSourceAcqMain.Logic
{
    public class AcquisitionManager
    {
        #region Singleton

        private static AcquisitionManager instance;
        public static AcquisitionManager Instance { get { return instance ?? (instance = new AcquisitionManager()); } }
        private AcquisitionManager() { }

        #endregion

        // -- Kinect
        // sensor
        private KinectSensor kinectSensor;
        private MultiSourceFrameReader multiReader;
        private CoordinateMapper coordinateMapper;

        // color
        private FrameDescription colorFrameDescription;
        private int colorWidth;
        private int colorHeight;
        private int colorPixelSize;
        private int colorByteSize;

        // depth
        private FrameDescription depthFrameDescription;
        private int depthWidth;
        private int depthHeight;
        private int depthPixelSize;
        private int depthByteSize;
                
        // -- PS3Eye
        private int numCameras = 2;
        private int activeCameras;
        private List<PS3EyeDevice> devices = new List<PS3EyeDevice>();
        private PS3EyePanel panel;
        private Barrier barrier;
        private CancellationTokenSource cancelSource;
        private CancellationToken cancelToken;
        private byte[][] frames;
        private int currentFramerate;
        private bool isPs3EyeRunning;
        private int psWidth;
        private int psHeight;
        private int psPixelSize;
        private int psByteSize;
        private int psSleepTime = 5;    // ms to wait in thread acquiring frames

        // -- Recording & debug
        private long frameNb = -1;          // absolute ordering       
        private int frameNbAbsolute = 0;
        private int nthFrame = 1;
        private long ticksLastFrame = 0;

        // --- init
        public void Init()
        {
            InitKinect();
            InitPS3Eye();
        }
        private void InitKinect()
        {
            // init sensor
            kinectSensor = KinectSensor.GetDefault();
            kinectSensor.IsAvailableChanged += (s, e) => Context.GUI.UpdateKinectStatus();
            multiReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);                 // (!!!!!)
            multiReader.MultiSourceFrameArrived += multiReader_MultiSourceFrameArrived;
            coordinateMapper = kinectSensor.CoordinateMapper;            

            // color
            colorFrameDescription = kinectSensor.ColorFrameSource.FrameDescription;
            colorWidth = colorFrameDescription.Width;
            colorHeight = colorFrameDescription.Height;
            colorPixelSize = colorWidth * colorHeight;
            colorByteSize = colorPixelSize * 4;            

            // depth
            depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            depthWidth = depthFrameDescription.Width;
            depthHeight = depthFrameDescription.Height;
            depthPixelSize = depthWidth * depthHeight;
            depthByteSize = depthPixelSize * (int)depthFrameDescription.BytesPerPixel;                        

            // context
            Context.KinectSensor = kinectSensor;
            Context.DepthWidth = depthWidth;
            Context.DepthHeight = depthHeight;
            Context.ColorWidth = colorWidth;
            Context.ColorHeight = colorHeight;
        }
        private void InitPS3Eye()
        {
            //int cameraCount = PS3EyeDevice.CameraCount;       // optional
            frames = new byte[numCameras][];
            activeCameras = numCameras;

            // init device objects
            for (int i = 0; i < numCameras; i++)
            {
                devices.Add(new PS3EyeDevice());
            }

            // init settings panel
            panel = Context.GUI.PS3EyePanel;

            panel.ResolutionChanged += () => devices.Take(activeCameras).ForEach(dev =>
            {
                dev.Resolution = panel.Resolution;
            });
            panel.FramerateChanged += () => devices.Take(activeCameras).ForEach(dev =>
            {
                dev.Framerate = panel.FrameRate;
            });
            panel.ExposureChanged += () => devices.Take(activeCameras).ForEach(dev =>
            {
                dev.AutoExposure = panel.IsAutoExposure;
                dev.Exposure = panel.Exposure;
            });
            panel.GainChanged += () => devices.Take(activeCameras).ForEach(dev =>
            {
                dev.AutoGain = panel.IsAutoGain;
                dev.Gain = panel.Gain;
            });
            panel.WhiteBalanceChanged += () => devices.Take(activeCameras).ForEach(dev =>
            {
                dev.AutoWhiteBalance = panel.IsAutoWhiteBalance;
                dev.WhiteBalanceRed = panel.WhiteBalance1;
                dev.WhiteBalanceGreen = panel.WhiteBalance2;
                dev.WhiteBalanceBlue = panel.WhiteBalance3;
            });
        }

        // --- start/stop
        public void StartKinect()
        {            
            this.kinectSensor.Open();
            Context.GUI.SetNthFrameStatus(nthFrame);
            barrier = null;     // in case where ps3eye is not run now, but was run before
        }
        public void StopKinect()
        {
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                //this.kinectSensor = null;
            }
            if (cancelSource != null) cancelSource.Cancel();
        }

        public void StartPS3Eye(bool kinectAsSupervisor)
        {
            // open devices
            int framerate = panel.FrameRate;
            currentFramerate = framerate;
            CLEyeCameraResolution resolution = panel.Resolution;

            for (int i = 0; i < activeCameras; i++)
            {
                devices[i].Framerate = framerate;
                devices[i].Resolution = resolution;
                devices[i].Create(PS3EyeDevice.CameraUUID(i));
            }

            // update panel
            panel.OnGainChanged();
            panel.OnExposureChanged();
            panel.OnWhiteBalanceChanged();

            // --- multi thread version
            // resolution
            if (resolution == CLEyeCameraResolution.CLEYE_VGA)
            {
                psWidth = 640;
                psHeight = 480;
            }
            else
            {
                psWidth = 320;
                psHeight = 240;
            }

            // sizes
            Context.PsWidth = psWidth;
            Context.PsHeight = psHeight;
            psPixelSize = psWidth * psHeight;
            psByteSize = psPixelSize * 4;
            
            // start/stop 
            isPs3EyeRunning = true;
            panel.SetMode(true);

            if (kinectAsSupervisor)
            {                
                barrier = new Barrier(numCameras);
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;

                for (int i = 0; i < numCameras; i++)
                {
                    BackgroundWorker bgWorkerCam = new BackgroundWorker();
                    bgWorkerCam.DoWork += new DoWorkEventHandler(bgWorkerCamWithKinect_DoWork);
                    bgWorkerCam.RunWorkerAsync(i);
                }
            }
            else
            {                
                barrier = new Barrier(numCameras + 1);
                cancelSource = new CancellationTokenSource();
                cancelToken = cancelSource.Token;

                for (int i = 0; i < numCameras; i++)
                {
                    BackgroundWorker bgWorkerCam = new BackgroundWorker();
                    bgWorkerCam.DoWork += new DoWorkEventHandler(bgWorkerCam_DoWork);
                    bgWorkerCam.RunWorkerAsync(i);
                }
                                
                BackgroundWorker bgWorkerSupervisor = new BackgroundWorker();
                bgWorkerSupervisor.DoWork += new DoWorkEventHandler(bgWorkerSupervisor_DoWork);
                bgWorkerSupervisor.RunWorkerAsync();                
            }            
        }
        public void StopPS3Eye()
        {
            if (isPs3EyeRunning)
            {
                panel.SetMode(false);
                isPs3EyeRunning = false;
                if (cancelSource != null) cancelSource.Cancel();
            }
        }

        // --- ps3eye acq
        private void bgWorkerCamWithKinect_DoWork(object sender, DoWorkEventArgs e)
        {
            int camIdx = (int)e.Argument;
            devices[camIdx].Start();

            while (isPs3EyeRunning)
            {
                // wait for signal from Kinect
                if (barrier != null) { try { barrier.SignalAndWait(cancelToken); } catch { } }

                // acquire data
                byte[] bytes = null;
                while ((bytes = devices[camIdx].GetFrameBytes()) == null) Thread.Sleep(psSleepTime);
                frames[camIdx] = bytes;
            }
            devices[camIdx].Dispose();
            frames[0] = null;
            frames[1] = null;
        }
        private void bgWorkerCam_DoWork(object sender, DoWorkEventArgs e)
        {
            int camIdx = (int)e.Argument;
            devices[camIdx].Start();

            while (isPs3EyeRunning)
            {
                // wait for signal to acquire frame
                if (barrier != null) { try { barrier.SignalAndWait(cancelToken); } catch { } }

                byte[] bytes = null;
                while ((bytes = devices[camIdx].GetFrameBytes()) == null) Thread.Sleep(psSleepTime);
                frames[camIdx] = bytes;

                // signal frame acquired
                if (barrier != null) { try { barrier.SignalAndWait(cancelToken); } catch { } }

                // supervisor runs here (other thread)
            }

            devices[camIdx].Dispose();
        }
        private void bgWorkerSupervisor_DoWork(object sender, DoWorkEventArgs e)
        {
            int frameWidth = devices[0].FrameWidth;
            int frameHeight = devices[0].FrameHeight;
            int frameStride = devices[0].FrameStride;
            int imgSize = devices[0].ImgSize;

            long ticksNextAcq = DateTime.Now.Ticks + 5 * TimeSpan.TicksPerMillisecond;      // initial value
            int fps = Context.GUI.PS3EyePanel.FrameRate;
            int frameTime = 1000 / fps;

            while (isPs3EyeRunning)
            {                
                ticksNextAcq = ticksNextAcq + frameTime * TimeSpan.TicksPerMillisecond;
                while (DateTime.Now.Ticks < ticksNextAcq) Thread.Sleep(1);

                // signal other threads to acquire frames
                if (barrier != null) { try { barrier.SignalAndWait(cancelToken); } catch { } }
                Utils.UpdateCounter("Input");
                long ticksAcqTotal = DateTime.Now.Ticks;

                // frames acquired here (other threads)

                // signal other threads to acquire frames
                if (barrier != null) { try { barrier.SignalAndWait(cancelToken); } catch { } }
                Utils.UpdateCounter("Acquired");
                Utils.UpdateTimer("Acquire", ticksAcqTotal);
                Utils.UpdateCounter("Expired", false);

                long ticksCopyData = DateTime.Now.Ticks;

                // now first frames are synchronized
                byte[] bytes0 = frames[0];
                byte[] bytes1 = frames[1];

                // join frames
                byte[] psBytes = null;
                if (bytes0 != null && bytes1 != null)
                {
                    long ticksCreatePS3EyeData = DateTime.Now.Ticks;
                    psBytes = new byte[psByteSize * 2];
                    Utils.UpdateTimer("CreatePS3EyeData", ticksCreatePS3EyeData);

                    long ticksCopyPS3EyeData = DateTime.Now.Ticks;
                    CopyPS3EyeDataMirror(psBytes, bytes0, bytes1);      // mirror to be consistent with Kinect 2
                    Utils.UpdateTimer("CopyPS3EyeData", ticksCopyPS3EyeData);
                }

                // multiFrame
                long ticksMultiFrame = DateTime.Now.Ticks;
                MultiFrame multiFrame = new MultiFrame();
                multiFrame.FrameNb = Interlocked.Increment(ref frameNb);
                multiFrame.PS3EyeData = psBytes;
                multiFrame.HasKinectData = false;
                multiFrame.HasPS3EyeData = true;
                Utils.UpdateTimer("MultiFrame", ticksMultiFrame);

                long ticksEnqueue = DateTime.Now.Ticks;
                ProcessingManager.Instance.EnqueueMultiFrame(multiFrame);
                Utils.UpdateTimer("Enqueue", ticksEnqueue);

                Utils.UpdateTimer("CopyFramesData", ticksCopyData);

                // display timers & queues
                Context.GUI.DisplayPerformance();
            }
            // each camera thread disposes its cam    
        }
        private void CopyPS3EyeDataMirror(byte[] psBytes, byte[] psBytes0, byte[] psBytes1)
        {
            int psStride = psWidth * 4;

            unsafe
            {
                fixed (byte* fixedPsBytes = psBytes)
                fixed (byte* fixedPsBytes0 = psBytes0)
                fixed (byte* fixedPsBytes1 = psBytes1)
                {
                    byte* ptrDest0 = fixedPsBytes;
                    byte* ptrDest1 = fixedPsBytes;
                    ptrDest1 += psByteSize;
                    byte* ptrSrc0 = fixedPsBytes0;
                    byte* ptrSrc1 = fixedPsBytes1;
                    ptrSrc0 -= psStride;        // boundary conditions
                    ptrSrc1 -= psStride;        // boundary conditions

                    for (int h = 0; h < psHeight; h++)
                    {
                        ptrSrc0 += psStride * 2;
                        ptrSrc1 += psStride * 2;
                        for (int w = 0; w < psStride; w += 4)
                        {
                            *ptrDest0++ = *ptrSrc0++;
                            *ptrDest0++ = *ptrSrc0++;
                            *ptrDest0++ = *ptrSrc0++;
                            *ptrDest0++ = *ptrSrc0++;

                            *ptrDest1++ = *ptrSrc1++;
                            *ptrDest1++ = *ptrSrc1++;
                            *ptrDest1++ = *ptrSrc1++;
                            *ptrDest1++ = *ptrSrc1++;

                            ptrSrc0 -= 8;
                            ptrSrc1 -= 8;
                        }
                    }
                }
            }
        }

        // --- kinect acq
        private void multiReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // acquires each type of frame and sends it to process

            // performance
            if (ticksLastFrame != 0) Utils.UpdateTimer("InputInterval", ticksLastFrame);
            ticksLastFrame = DateTime.Now.Ticks;            
            Utils.UpdateCounter("Input");

            // debug
            frameNbAbsolute++;
            if (frameNbAbsolute % nthFrame != 0) return;

            // process
            ThreadPool.QueueUserWorkItem(new WaitCallback(o => { ProcessMultiFrame(e); }));           
        }
        private void ProcessMultiFrame(MultiSourceFrameArrivedEventArgs e)
        {
            long ticksAcqTotal = DateTime.Now.Ticks;                        

            // frames
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            BodyFrame bodyFrame = null;
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            // ps3eye
            byte[] psBytes0 = null;
            byte[] psBytes1 = null;

            // if the frame has expired by the time we process this event, return (this actually never happens)
            if (multiSourceFrame == null) return;            

            try
            {
                // get kinect frames                
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();

                // optionally get ps3eye frames
                if (isPs3EyeRunning)
                {                    
                    psBytes0 = frames[0];
                    psBytes1 = frames[1];
                }

                // if any frame has expired by the time we process this event, return (dispose others in finally)
                // psBytes0 and psBytes1 may be null in the beginning (when ps3eye starts longer than kinect)
                if (colorFrame == null || depthFrame == null || bodyIndexFrame == null || bodyFrame == null || 
                    (isPs3EyeRunning && (psBytes0 == null || psBytes1 == null)))
                {                    
                    Utils.UpdateCounter("Expired");
                    Utils.IncrementTotalLost();
                    return;
                }
                else
                {                    
                    Utils.UpdateCounter("Expired", false);
                }

                // performance
                Utils.UpdateTimer("Acquire", ticksAcqTotal);
                Utils.UpdateCounter("Acquired");

                // process
                ProcessFrames(colorFrame, depthFrame, bodyIndexFrame, bodyFrame, psBytes0, psBytes1);
            }
            catch
            {
                
            }
            finally
            {
                if (depthFrame != null) depthFrame.Dispose();
                if (colorFrame != null) colorFrame.Dispose();
                if (bodyIndexFrame != null) bodyIndexFrame.Dispose();
                if (bodyFrame != null) bodyFrame.Dispose();
            }
        }
        private void ProcessFrames(ColorFrame colorFrame, DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame, BodyFrame bodyFrame, byte [] psBytes0, byte [] psBytes1)
        {            
            // create multiframe to process
            long ticksCopyData = DateTime.Now.Ticks;

            MultiFrame multiFrame = new MultiFrame();
            multiFrame.FrameNb = Interlocked.Increment(ref frameNb);

            // color
            long ticksCreateColorData = DateTime.Now.Ticks;
            byte[] colorData = new byte[colorByteSize];
            Utils.UpdateTimer("CreateColorData", ticksCreateColorData);

            long ticksCopyColorData = DateTime.Now.Ticks;
            colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Bgra);
            Utils.UpdateTimer("CopyColorData", ticksCopyColorData);

            // depth
            long ticksCreateDepthData = DateTime.Now.Ticks;
            ushort[] depthData = new ushort[depthPixelSize];
            depthFrame.CopyFrameDataToArray(depthData);            
            Utils.UpdateTimer("CreateDepthData", ticksCreateDepthData);

            // body index
            long ticksCreateBodyIndexData = DateTime.Now.Ticks;
            byte[] bodyIndexData = new byte[depthPixelSize];
            bodyIndexFrame.CopyFrameDataToArray(bodyIndexData);
            Utils.UpdateTimer("CreateBodyIndexData", ticksCreateBodyIndexData);

            // bodies
            long ticksCreateBodiesData = DateTime.Now.Ticks;
            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);
            Utils.UpdateTimer("CreateBodiesData", ticksCreateBodiesData);

            // ps3eye
            byte[] psBytes = null;
            if (psBytes0 != null && psBytes1 != null)
            {
                long ticksCreatePS3EyeData = DateTime.Now.Ticks;
                psBytes = new byte[psByteSize * 2];
                Utils.UpdateTimer("CreatePS3EyeData", ticksCreatePS3EyeData);

                long ticksCopyPS3EyeData = DateTime.Now.Ticks;
                CopyPS3EyeDataMirror(psBytes, psBytes0, psBytes1);
                Utils.UpdateTimer("CopyPS3EyeData", ticksCopyPS3EyeData);
            }

            // multiFrame
            long ticksMultiFrame = DateTime.Now.Ticks;
            multiFrame.DepthData = depthData;
            multiFrame.ColorData = colorData;
            multiFrame.BodyIndexData = bodyIndexData;
            multiFrame.Bodies = bodies;
            multiFrame.PS3EyeData = psBytes;
            multiFrame.HasKinectData = true;
            multiFrame.HasPS3EyeData = psBytes != null ? true : false;
            Utils.UpdateTimer("MultiFrame", ticksMultiFrame);

            long ticksEnqueue = DateTime.Now.Ticks;
            ProcessingManager.Instance.EnqueueMultiFrame(multiFrame);
            Utils.UpdateTimer("Enqueue", ticksEnqueue);

            Utils.UpdateTimer("CopyFramesData", ticksCopyData);

            // display timers & queues
            Context.GUI.DisplayPerformance();
        }

        // --- dispose
        public void Dispose()
        {
            StopKinect();
            StopPS3Eye();            
        }
        
    }
}
