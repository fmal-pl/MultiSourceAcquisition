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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
    public class Utils
    {
        // performance
        private static int maxTimeQueueLen = 60;
        private static int maxMemoryQueueLen = 30;
        private static string [] queueNames = { "Input", "Processed", "Sorted", "Recording" };
        private static Dictionary<string, int> queueValues = new Dictionary<string, int>();

        private static string[] timerNames = { "InputInterval", "Acquire", "CopyFramesData", 
                                                 "-",
                                                 "ColorMapping", "(ColorMappingCoord)", "(ColorMappingBytes)",
                                                 "ColorResize", "(ColorResizeInput)", "(ColorResizeMapped)", 
                                                 "ColorBitmap",
                                                 "DepthData", 
                                                 "DepthMapping", "(DepthMappingCoord)", "(DepthMappingBytes)", 
                                                 "DepthBitmap",
                                                 "Body",
                                                 "PS3EyeBitmap",
                                                 "ProcessGPU",
                                                 "--",
                                                 "ProcessFrame",
                                                 "DisplayColor", "DisplayDepth", "DisplayPS3Eye",
                                                 "---",
                                                 "CreateColorData", "CreateDepthData", "CreateBodyIndexData",  "CreateBodiesData", "CreatePS3EyeData",
                                                 "MultiFrame", "Enqueue", "CopyColorData", "CopyPS3EyeData"
                                             };

        private static string[] counterNames = { "Input", "Expired", "Acquired", "Processed" };

        private static Dictionary<string, long> timerValues = new Dictionary<string, long>();
        private static Dictionary<string, ConcurrentQueue<long>> timerQueues = new Dictionary<string, ConcurrentQueue<long>>();
        private static Dictionary<string, long> counterValues = new Dictionary<string, long>();
        private static Dictionary<string, ConcurrentQueue<long>> counterQueues = new Dictionary<string, ConcurrentQueue<long>>();
        private static ConcurrentQueue<long> memoryQueue = new ConcurrentQueue<long>();
        private static PerformanceCounter cpuCounter;

        private static int totalLost = 0;

        public static string[] QueueNames { get { return queueNames; } }
        public static string[] TimerNames { get { return TimerNames; } }

        static Utils()
        {
            foreach (var qname in queueNames)
            {
                queueValues.Add(qname, 0);
            }
            
            foreach (var name in timerNames)
            {
                timerValues.Add(name, 0);
                timerQueues.Add(name, new ConcurrentQueue<long>());
            }

            foreach (var name in counterNames)
            {
                counterValues.Add(name, 0);
                counterQueues.Add(name, new ConcurrentQueue<long>());
            }

            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
        }

        public static long GetTimeMs(long ticks)
        {
            return (DateTime.Now.Ticks - ticks) / TimeSpan.TicksPerMillisecond;
        }

        public static Bitmap CreateBitmap(byte[] bytes, int width, int height, int stride, System.Drawing.Imaging.PixelFormat format)
        {
            Bitmap bmp;
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    bmp = new Bitmap(width, height, stride, format, new IntPtr(ptr));
                }
            }
            return bmp;
        }
        public static BitmapSource ToBitmapSource(Image img)
        {
            if (img == null) return null;

            MemoryStream memStream = new MemoryStream();
            img.Save(memStream, ImageFormat.Bmp);
            memStream.Seek(0, SeekOrigin.Begin);
            BitmapImage bmpImg = new BitmapImage();
            bmpImg.BeginInit();
            bmpImg.StreamSource = memStream;
            bmpImg.EndInit();
            bmpImg.Freeze();
            return bmpImg;
        }
        public static BitmapSource ToBitmapSource(byte[] bytes, int width, int height, System.Windows.Media.PixelFormat format)
        {
            if (bytes == null) return null;

            int bpp = format.BitsPerPixel / 8;
            int stride = bpp * width;
            BitmapSource bmpSrc = BitmapSource.Create(width, height, 96.0, 96.0, format, null, bytes, stride);
            bmpSrc.Freeze();
            return bmpSrc;
        }

        public static BitmapSource ResizeBitmapSource(BitmapSource bmpSrc, int width, int height, BitmapScalingMode scalingMode)
        {
            DrawingGroup group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(bmpSrc, new Rect(0, 0, width, height)));
            
            DrawingVisual targetVisual = new DrawingVisual();
            DrawingContext targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            RenderTargetBitmap target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            target.Freeze();

            return target;
        }

        public static void UpdateTimer(string name, long ticks)
        {
            if (!Context.DisplayPerformance) return;

            long ms = Utils.GetTimeMs(ticks);
            var queue = timerQueues[name];
            queue.Enqueue(ms);
            while (queue.Count > maxTimeQueueLen) queue.TryDequeue(out ms);
            ms = (long)queue.Average();
            timerValues[name] = ms;
        }
        public static void SetQueueCount(string queueName, int count)
        {
            if (!Context.DisplayPerformance) return;

            queueValues[queueName] = count;
        }
        public static void UpdateUsageMemoryAndCPU()
        {
            // this is displayed always
            long memoryUsage = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            memoryQueue.Enqueue(memoryUsage);
            while (memoryQueue.Count > maxMemoryQueueLen) memoryQueue.TryDequeue(out memoryUsage);
        }
        public static void UpdateCounter(string name, bool enqueueCurrentTime = true)
        {
            long ticksNow = DateTime.Now.Ticks;
            var queue = counterQueues[name];
            if (enqueueCurrentTime) queue.Enqueue(ticksNow);
            long ticks = 0;
            if (queue.Count > 0)
            {
                while (queue.TryPeek(out ticks))
                {
                    long ms = ((ticksNow - ticks) / TimeSpan.TicksPerMillisecond);
                    if (ms > 1000)
                    {
                        queue.TryDequeue(out ticks);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            counterValues[name] = queue.Count;
        }

        public static string GetTimersInfo()
        {
            if (!Context.DisplayPerformance) return null;

            StringBuilder sb = new StringBuilder();
            foreach (var key in timerNames)
            {
                if (key.StartsWith("-"))
                {
                    sb.AppendLine("-");
                }
                else
                {
                    long val = timerValues[key];
                    string space = " " + (val >= 100 ? "" : val >= 10 ? " " : "  ");

                    string key2 = key.StartsWith("(") ? "  " + key : key;

                    sb.AppendLine(key2.PadRight(30) + space + val + " ms");
                }
            }
            return sb.ToString();
        }
        public static string GetQueuesInfo()
        {
            if (!Context.DisplayPerformance) return null;

            StringBuilder sb = new StringBuilder();
            foreach (var key in queueNames)
            {
                long val = queueValues[key];
                sb.AppendLine(key.PadRight(30) + val.ToString().PadLeft(4));
            }
            return sb.ToString();
        }
        public static string GetMemoryAndCPUInfo()
        {            
            return
                ((int)(memoryQueue.Average() / (1024 * 1024))).ToString().PadLeft(4) + " MB" +
                "".PadLeft(20) +
                "CPU:" + ((int)cpuCounter.NextValue()).ToString().PadLeft(3) + "%";
        }
        public static string GetCountersInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("FPS\n");
            foreach (var key in counterNames)
            {                
                long val = counterValues[key];
                sb.AppendLine(key.PadRight(30) + val.ToString().PadLeft(4));
            }
            
            sb.AppendLine();
            sb.AppendLine("Total lost:".PadRight(30) + totalLost.ToString().PadLeft(4));

            return sb.ToString();
        }

        public static void ClearTotalLost()
        {
            totalLost = 0;
        }
        public static void IncrementTotalLost()
        {
            Interlocked.Increment(ref totalLost);
        }

        public static VideoCodec StringToVideoCodec(string codec)
        {
            return (VideoCodec)Enum.Parse(typeof(VideoCodec), codec);
        }
        public static List<string> GetListVideoCodecs()
        {
            List<string> codecs = Enum.GetNames(typeof(VideoCodec)).ToList();
            codecs.Sort();
            return codecs;
        }

        public static string ResolvePath(string dir)
        {
            string currDir = Directory.GetCurrentDirectory();
            string path;

            path = Path.Combine(currDir, dir);
            if (Directory.Exists(path)) return path;

            path = Path.Combine(currDir, @"..\" + dir);
            if (Directory.Exists(path)) return path;

            path = Path.Combine(currDir, @"..\..\" + dir);
            if (Directory.Exists(path)) return path;

            return null;
        }
        public static string GetFilePathBase(DateTime time, RecorderPanel recorderPanel)
        {
            // check base dir (database root dir)
            string dir = recorderPanel.Path;
            if (!Directory.Exists(dir)) throw new Exception("Invalid path");

            // person -> session root dir
            string person = recorderPanel.Person;
            if (string.IsNullOrWhiteSpace(person)) throw new Exception("Please specify person");
            person = person.Replace(' ', '_').Replace('-', '_').Replace('.', '_');

            // add subdir
            dir = Path.Combine(dir, String.Format("{0:yyyy_MM_dd}", time) + "_" + person);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);     // will throw exception if cant create
            }

            // modify concept according to naming rules
            string concept = recorderPanel.Concept;
            if (string.IsNullOrWhiteSpace(concept)) throw new Exception("Please specify concept");
            concept = concept.ToLower();
            concept = concept.Replace(' ', '_').Replace('-', '_').Replace('.', '_');            // convert separators
            concept = concept.Replace('ę', 'e').Replace('ó', 'o').Replace('ą', 'a').Replace('ś', 's').Replace('ł', 'l').Replace('ż', 'z').Replace('ź', 'z').Replace('ć','c').Replace('ń', 'n');     // remove polish diactrics

            if (char.IsNumber(concept, 0))
            {
                concept = "liczebnik_" + concept;
            }

            // add subdir
            dir = Path.Combine(dir, concept);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);     // will throw exception if cant create
            }            

            // fileNameBase
            string timeString = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", time);
            //string filePath = Path.Combine(dir, concept + "_" + timeString + "_" + person);
            string filePath = Path.Combine(dir, timeString);
            return filePath;
        }
        
    }
}
