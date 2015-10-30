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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MultiSourceAcqMain.Logic;

namespace MultiSourceAcqMain.AccGlove
{
    public class AccGloveManager
    {
        #region Singleton

        private static AccGloveManager instance;
        public static AccGloveManager Instance { get { return instance ?? (instance = new AccGloveManager()); } }
        private AccGloveManager() { }

        #endregion

        private int displayEveryNthFrame = 20;  // no need to display at 400Hz

        private SerialPort port;
        private AccGlovePanel panel;
        private string portName;
        private bool isStreaming;
        private List<int[]> bufferAcc = new List<int[]>();
        private string filePathAcc;
        private long ticksRecordingStart = 0;
        private bool isRecording = false;
        private string fileLastRecording;

        // init & check
        public void Init()
        {
            panel = Context.GUI.AccGlovePanel;
            panel.AccGloveManager = this;
        }
        public string Check()
        {
            portName = GetPortName();
            port = new SerialPort(portName);
            port.Open();
            port.Write("pi");
            string response = port.ReadLine();
            port.Close();
            return response;
        }
        private string GetPortName()
        {
            string portName = panel.SelectedPort;
            if (portName == null)
            {
                throw new Exception("Arrr, you need a porrrt!");
            }
            return portName;
        }

        // acquisition
        public void Start()
        {
            panel.IsCheckEnabled = false;
            portName = GetPortName();           
            panel.ClearCheckLabel();
            panel.SetInfo("running");

            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerAsync();
        }
        public void Stop()
        {
            if (port != null && port.IsOpen)
            {
                port.Write("e");
                isStreaming = false;
            }
            panel.IsCheckEnabled = true;
            panel.ClearCheckLabel();
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            long ticksStart = DateTime.Now.Ticks;
            long readingNb = 0;

            // open            
            port = new SerialPort(portName);
            port.Open();

            // start
            port.Write("b");
            isStreaming = true;

            // find level component (first reading)            
            string response = port.ReadLine();
            string[] split = response.Split(' ');
            int len = split.Length;                     // first entry is time (need len + 1, but len is 1 longer anyway)
            // will index many things from 1 now

            int[] levelComponent = new int[len];
            for (int i = 1; i < len; i++) levelComponent[i] = int.Parse(split[i - 1]);

            while (isStreaming)
            {
                response = port.ReadLine();
                readingNb++;

                // data
                split = response.Split(' ');
                int[] values = new int[len];
                for (int i = 1; i < len; i++)
                {
                    int value = int.Parse(split[i - 1]);
                    values[i] = value;
                }
            
                // send to display
                if (readingNb % displayEveryNthFrame == 0)
                {
                    Context.GUI.DisplayAccData(values);
                }

                // send to recording
                if (isRecording)
                {
                    long time = Utils.GetTimeMs(ticksRecordingStart);
                    values[0] = (int)time;
                    AddAccData(values);
                }
            }

            port.Close();
        }

        // recording
        public void StartAccRecording()
        {
            bufferAcc.Clear();
            filePathAcc = Utils.GetFilePathBase(DateTime.Now, Context.GUI.RecorderPanel) + "_Acc.mat";
            ticksRecordingStart = DateTime.Now.Ticks;
            isRecording = true;
        }
        public void StopAccRecording()
        {
            isRecording = false;

            if (bufferAcc.Count > 0)
            {
                int[][] accData = bufferAcc.ToArray();

                csmatio.types.MLInt32 accDataMat = new csmatio.types.MLInt32("accData", accData);
                List<csmatio.types.MLArray> lstAccDatMat = new List<csmatio.types.MLArray>();
                lstAccDatMat.Add(accDataMat);
                csmatio.io.MatFileWriter writer = new csmatio.io.MatFileWriter(filePathAcc, lstAccDatMat, true);
                fileLastRecording = filePathAcc;

                bufferAcc.Clear();
            }
        }
        private void AddAccData(int[] data)
        {
            bufferAcc.Add(data);
        }

        public void ClearLastRecordingHandle()
        {
            fileLastRecording = filePathAcc;
        }
        public void RemoveLastRecording()
        {
            File.Delete(fileLastRecording);
        }

        // dispose
        public void Dispose()
        {
            try
            {
                Context.GUI.Dispatcher.Invoke(() => Stop());
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("Error on acc glove dispose: " + e.Message);
            }
        }
    }
}
