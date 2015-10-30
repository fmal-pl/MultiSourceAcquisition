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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using MultiSourceAcqMain.AccGlove;
using MultiSourceAcqMain.Logic;
using MultiSourceAcqMain.PS3Eye;
using Settings = WiTKoM.Common.Settings;
using Brush = System.Windows.Media.Brush;
using System.IO;
using Path = System.IO.Path;

namespace MultiSourceAcqMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isRecording = false;
        private int bmpNbAbsolute = 0;
        private long usageTicks = 0;
        private bool isClosing = false;
        private int countRep = 0;
        private bool isDevicesRunning = false;
        private bool isGloveRunning = false;
        private int[] levelComponents;

        // theme
        private bool isDarkTheme = false;

        private Brush colorDarkWindowBg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(68, 68, 68));
        private Brush colorDarkTextFg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 226, 205));
        private Brush colorDarkButtonBg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 189, 189));
        private Brush colorDarkTextBoxBg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(225, 225, 225));
        private Brush colorDarkCboBg = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 189, 189));

        private Brush colorLightTextFg;
        private Brush colorLightWindowBg;
        private Brush colorLightButtonBg;
        private Brush colorLightTextBoxBg;
        private Brush colorLightCboBg;

        // quick concept/action
        private Dictionary<string, List<string>> dctQuickConcepts = new Dictionary<string, List<string>>();

        // public
        public PS3EyePanel PS3EyePanel { get { return ps3EyePanel; } }
        public RecorderPanel RecorderPanel { get { return recorderPanel; } }
        public AccGlovePanel AccGlovePanel { get { return accGlovePanel; } }

        // ----- init
        public MainWindow()
        {
            InitializeComponent();

            //InitPathFFMPEG(); // use if FFMPEG libs are to remain in external location (currently they are copied in an post-build event)
            InitVisibility();

            Context.GUI = this;

            recorderPanel.ConceptChanged += () => bClearCount_Click(null, null);
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void InitPathFFMPEG()
        {
            //string ffmpegLibsPath = @"..\Libs\ffmpeg_x64";        // x64
            string ffmpegLibsPath = @"..\Libs\ffmpeg_x86";        // x86            
            ffmpegLibsPath = Utils.ResolvePath(ffmpegLibsPath);     // change to absolute path
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + ffmpegLibsPath);            
        }
        private void InitVisibility()
        {
            elRecording.Visibility = System.Windows.Visibility.Hidden;
            elDevicesRunning.Visibility = System.Windows.Visibility.Hidden;
            elGloveRunning.Visibility = System.Windows.Visibility.Hidden;
            Update8bitDepthInfo();
        }
        private void LoadSettings()
        {
            // cboDevices
            string selectedItem = Settings.GetSetting("mainCboDevices");
            if (selectedItem != null && cboDevices.Items.Contains(selectedItem)) cboDevices.SelectedItem = selectedItem;
            else cboDevices.SelectedIndex = 0;

            // colorMapping
            chkColorMapping.IsChecked = Settings.GetSettingBool("mainChkColorMapping");
            chkDepthMapping.IsChecked = Settings.GetSettingBool("mainChkDepthMapping");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AcquisitionManager.Instance.Init();
                AccGloveManager.Instance.Init();
                ProcessingManager.Instance.Init();

                cboDevices.Items.Add("Kinect");
                cboDevices.Items.Add("PS3Eye");
                cboDevices.Items.Add("Kinect&PS3Eye");
                bRemoveLast.IsEnabled = false;

                multiGraphAcc.Init(21, 50);      // acc glove
                recorderPanel.FillCodecCombo(ProcessingManager.Instance.GetListOfCodecs());

                // settings
                LoadSettings();

                // start theads etc
                ProcessingManager.Instance.StartProcessing();

                // monitoring
                BackgroundWorker bgWorker = new BackgroundWorker();
                bgWorker.DoWork += MonitorPerformance;
                bgWorker.RunWorkerAsync();

                // theme & grid
                InitThemes();
                InitGrids();
                InitQuickConcept();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization error: " + ex.Message);
            }

        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // double closing is neccessary for the ps3eye
            if (!isClosing)
            {
                e.Cancel = true;
                isClosing = true;
                BackgroundWorker bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (s, a) =>
                {
                    Thread.Sleep(100);

                    // dispose everything here
                    AcquisitionManager.Instance.Dispose();
                    ProcessingManager.Instance.Dispose();
                    AccGloveManager.Instance.Dispose();
                    WiTKoM.Common.Settings.SaveSettings();

                    Thread.Sleep(100);
                    Dispatcher.Invoke(new Action(() => Close()));
                };
                bgWorker.RunWorkerAsync();
            }            
        }
                     
        // --- display bitmaps
        public void DisplayBitmaps(MultiFrame multiFrame)
        {
            if (bmpNbAbsolute++ % Context.DisplayEveryNthFrame != 0) return;

            BitmapSource bmpSrcColor = null;
            BitmapSource bmpSrcDepth = null;
            BitmapSource bmpSrcPS3Eye = null;

            // TODO: freeze?

            if (Context.DisplayColor)
            {
                long ticksColor = DateTime.Now.Ticks;
                bmpSrcColor = GetColorBitmapSource(multiFrame);
                if (bmpSrcColor != null) bmpSrcColor.Freeze();
                Utils.UpdateTimer("DisplayColor", ticksColor);
            }

            if (Context.ProcessDepthData && Context.DisplayDepth)           // TODO: do not check processdepth?
            {
                long ticksDepth = DateTime.Now.Ticks;
                bmpSrcDepth = GetDepthBitmapSource(multiFrame);
                if (bmpSrcDepth != null) bmpSrcDepth.Freeze();
                Utils.UpdateTimer("DisplayDepth", ticksDepth);
            }

            if (Context.DisplayPS3Eye)
            {
                long ticksDepth = DateTime.Now.Ticks;
                bmpSrcPS3Eye = Utils.ToBitmapSource(multiFrame.PS3EyeData, Context.PsWidth, Context.PsHeight * 2, PixelFormats.Bgr32);
                if (bmpSrcPS3Eye != null) bmpSrcPS3Eye.Freeze();
                Utils.UpdateTimer("DisplayPS3Eye", ticksDepth);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                imgColor.Source = bmpSrcColor;
                imgDepth.Source = bmpSrcDepth;
                imgPS3Eye.Source = bmpSrcPS3Eye;
            }));
        }
        private BitmapSource GetColorBitmapSource(MultiFrame multiFrame)
        {
            BitmapSource bmpSrcColor;
            if (Context.ProcessColorResize)
            {
                if (Context.ProcessColorMapping)
                {
                    bmpSrcColor = Utils.ToBitmapSource(multiFrame.ColorResizedData, Context.ColorResizedWidth, Context.ColorResizedHeight * 2, PixelFormats.Bgr32);
                }
                else
                {
                    bmpSrcColor = Utils.ToBitmapSource(multiFrame.ColorResizedData, Context.ColorResizedWidth, Context.ColorResizedHeight, PixelFormats.Bgr32);
                }
            }
            else
            {
                if (Context.ProcessColorMapping)
                {
                    int colorByteSize = Context.ColorWidth * Context.ColorHeight * 4;
                    byte[] colorBytes = new byte[colorByteSize * 2];
                    Array.Copy(multiFrame.ColorData, colorBytes, colorByteSize);
                    Array.Copy(multiFrame.ColorMappedBytes, 0, colorBytes, colorByteSize, colorByteSize);
                    bmpSrcColor = Utils.ToBitmapSource(colorBytes, Context.ColorWidth, Context.ColorHeight * 2, PixelFormats.Bgr32);
                }
                else
                {
                    bmpSrcColor = Utils.ToBitmapSource(multiFrame.ColorData, Context.ColorWidth, Context.ColorHeight, PixelFormats.Bgr32);
                }
            }
            return bmpSrcColor;
        }
        private BitmapSource GetDepthBitmapSource(MultiFrame multiFrame)
        {
            if (Context.ProcessDepthMapping)
            {
                return Utils.ToBitmapSource(multiFrame.DepthBytes, Context.DepthWidth, Context.DepthHeight * 2, PixelFormats.Bgr32);
            }
            else
            {
                return Utils.ToBitmapSource(multiFrame.DepthBytes, Context.DepthWidth, Context.DepthHeight, PixelFormats.Bgr32);
            }
        }

        public void DisplayPS3Eye(BitmapSource bmpSrc)
        {
            Dispatcher.BeginInvoke(new Action(() => imgPS3Eye.Source = bmpSrc));
        }
        public void DisplayAccData(int [] values)
        {
            // acc manager decides how many frames to send (e.g. every 20th)

            if (Context.DisplayAccGloveGraph)
            {
                if (levelComponents == null || accGlovePanel.DoLevel)
                {
                    accGlovePanel.DoLevel = false;
                    levelComponents = values;
                }

                int[] accLeveled = new int[values.Length - 1];      // do not copy time
                for (int i = 0; i < accLeveled.Length; i++)
                {
                    accLeveled[i] = values[i + 1] - levelComponents[i + 1];
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    multiGraphAcc.EnqueueData(accLeveled);
                }));
            }

            if (Context.DisplayAccGloveValues)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < values.Length; i++)             // do not display time
                {
                    sb.Append(String.Format("{0}; ", values[i]).PadLeft(8));
                    if (i % 7 == 0) sb.AppendLine();
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    lAccGloveValues.Content = sb.ToString();
                }));
            }
        }

        // --- memory & cpu 
        private void MonitorPerformance(object sender, DoWorkEventArgs e)
        {
            while (!isClosing)
            {
                Utils.UpdateUsageMemoryAndCPU();
                DisplayUsageMemoryAndCPU();
                Thread.Sleep(100);
            }
        }
        private void DisplayUsageMemoryAndCPU()
        {            
            if (Utils.GetTimeMs(usageTicks) > 1000)
            {
                usageTicks = DateTime.Now.Ticks;
                Dispatcher.Invoke(() => 
                    lMemory.Content = Utils.GetMemoryAndCPUInfo());
            }
        }

        // --- perf & info & clear
        public void DisplayPerformance()
        {
            Dispatcher.Invoke(() => {
                lTimes.Content = Utils.GetTimersInfo();
                lQueues.Content = Utils.GetQueuesInfo();
                lCounters.Content = Utils.GetCountersInfo();
            });
        }
        public void UpdateKinectStatus()
        {
            Dispatcher.Invoke(() => lRunning.Content = Context.KinectSensor.IsAvailable ? "Running" : "Not running");

        }
        public void SetNthFrameStatus(int nthFrame)
        {
            Dispatcher.Invoke(() => lNthFrame.Content = "Processing every: " + nthFrame + " frame(s)");
        }
        public void DisplayInfo(string info)
        {
            Dispatcher.Invoke(() => lInfo.Content = info);
        }
        public void Update8bitDepthInfo()
        {
            l8bitDepth.Visibility = Context.Use8bitDepth ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        // --- gui
        private void cboDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool mappingEnabled = true;

            if (cboDevices.SelectedIndex == 1)
            {
                mappingEnabled = false;
            }
            
            lColorMapping.IsEnabled = mappingEnabled;
            chkColorMapping.IsEnabled = mappingEnabled;
            lDepthMapping.IsEnabled = mappingEnabled;
            chkDepthMapping.IsEnabled = mappingEnabled;

            Settings.SetSetting("mainCboDevices", cboDevices.SelectedItem.ToString());
        }
        private void chkColorMapping_Checked(object sender, RoutedEventArgs e)
        {
            Settings.SetSetting("mainChkColorMapping", ((bool)chkColorMapping.IsChecked).ToString());
        }
        private void chkDepthMapping_Checked(object sender, RoutedEventArgs e)
        {
            Settings.SetSetting("mainChkDepthMapping", ((bool)chkDepthMapping.IsChecked).ToString());
        }

        private void bStartStopDevices_Click(object sender, RoutedEventArgs e)
        {
            bool isKinectSelected = false;
            bool isPS3EyeSelected = false;
            Context.IsKinectActive = false;
            Context.IsPS3EyeActive = false;
            Context.ProcessColorMapping = chkColorMapping.IsChecked == true;
            Context.ProcessDepthMapping = chkDepthMapping.IsChecked == true;
            Utils.ClearTotalLost();

            if (cboDevices.SelectedIndex == 0)
            {
                isKinectSelected = true;
            }
            else if (cboDevices.SelectedIndex == 1)
            {
                isPS3EyeSelected = true;
            }
            else
            {
                isKinectSelected = true;
                isPS3EyeSelected = true;
            }

            if (!isDevicesRunning)
            {
                Context.IsKinectActive = isKinectSelected;
                Context.IsPS3EyeActive = isPS3EyeSelected;

                imgColor.Source = null;
                imgDepth.Source = null;
                imgPS3Eye.Source = null;
                try
                {
                    if (isKinectSelected) AcquisitionManager.Instance.StartKinect();
                    if (isPS3EyeSelected) AcquisitionManager.Instance.StartPS3Eye(isKinectSelected);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Couldn't start selected devices: " + ex.Message);          // warning: exception from PS3Eye can't be cought (probably because it originates in an unmanaged library)
                    return;
                }
                bStartStopDevices.Content = "Stop devices";
                cboDevices.IsEnabled = false;
                isDevicesRunning = true;
                elDevicesRunning.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                if (isKinectSelected) AcquisitionManager.Instance.StopKinect();
                if (isPS3EyeSelected) AcquisitionManager.Instance.StopPS3Eye();
                bStartStopDevices.Content = "Start devices";
                cboDevices.IsEnabled = true;
                isDevicesRunning = false;
                elDevicesRunning.Visibility = System.Windows.Visibility.Hidden;
                ClearLabelsAndDisplays();

                Context.IsKinectActive = false;
                Context.IsPS3EyeActive = false;
            }
        }
        private void bStartStopGlove_Click(object sender, RoutedEventArgs e)
        {
            // go glove!
            if (!isGloveRunning)
            {
                try
                {
                    AccGloveManager.Instance.Start();
                    bStartStopGlove.Content = "Stop glove";
                    isGloveRunning = true;
                    elGloveRunning.Visibility = System.Windows.Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {                    
                    AccGloveManager.Instance.Stop();
                    bStartStopGlove.Content = "Start glove";
                    isGloveRunning = false;
                    lAccGloveValues.Content = "";
                    multiGraphAcc.ClearData();
                    elGloveRunning.Visibility = System.Windows.Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void bRecord_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                if (Context.Use8bitDepth)
                {
                    MessageBox.Show("Warning - recording 8bit depth");
                }

                //frameNb = -1;
                try
                {
                    ProcessingManager.Instance.StartRecording();
                    AccGloveManager.Instance.StartAccRecording();
                    bRecord.Content = "Stop recording";
                    isRecording = true;                                        
                    ProcessingManager.Instance.ClearLastRecordingHandle();
                    AccGloveManager.Instance.ClearLastRecordingHandle();
                    bRemoveLast.IsEnabled = false;
                    elRecording.Visibility = System.Windows.Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                
            }
            else
            {
                ProcessingManager.Instance.StopRecording();
                AccGloveManager.Instance.StopAccRecording();
                bRecord.Content = "Start recording";
                isRecording = false;

                countRep++;
                lCount.Content = countRep + "";
                bRemoveLast.IsEnabled = true;
                elRecording.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        private void bClearCount_Click(object sender, RoutedEventArgs e)
        {
            countRep = 0;
            lCount.Content = countRep + "";
        }
        private void bRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            countRep--;
            lCount.Content = countRep + "";

            try
            {
                ProcessingManager.Instance.RemoveLastRecording();
            }
            catch(Exception ex1)
            {
                MessageBox.Show("Error: " + ex1.Message);
            }
            try
            {                
                AccGloveManager.Instance.RemoveLastRecording();
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Error: " + ex2.Message);
            }
            bRemoveLast.IsEnabled = false;
        }
        private void bSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            WiTKoM.Common.Settings.SaveSettings();
        }
        private void ClearLabelsAndDisplays()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (s, e) => {
                Thread.Sleep(300);
                Dispatcher.Invoke(() =>
                {
                    lTimes.Content = "";
                    lQueues.Content = "";
                    lCounters.Content = "";
                    lRunning.Content = "";
                    lNthFrame.Content = "";
                    imgColor.Source = null;
                    imgDepth.Source = null;
                    imgPS3Eye.Source = null;
                });
            };
            bgWorker.RunWorkerAsync();
        }

        // --- theme
        private void InitThemes()
        {
            colorLightButtonBg = bSaveSettings.Background;
            colorLightWindowBg = grid.Background;
            colorLightTextFg = bSaveSettings.Foreground;
            colorLightTextBoxBg = new SolidColorBrush(Colors.White);
            colorLightCboBg = cboDevices.Background;
            LoadTheme();
        }
        public void LoadTheme()
        {
            isDarkTheme = Context.DarkTheme;

            // --- choose
            Brush colorWindowBg;
            Brush colorTextFg;
            Brush colorButtonBg;
            Brush colorTextBoxBg;
            Brush colorCboBg;
            
            if (!isDarkTheme)
            {
                colorWindowBg = colorLightWindowBg;
                colorTextFg = colorLightTextFg;
                colorButtonBg = colorLightButtonBg;
                colorTextBoxBg = colorLightTextBoxBg;
                colorCboBg = colorLightCboBg;
            }
            else
            {
                colorWindowBg = colorDarkWindowBg;
                colorTextFg = colorDarkTextFg;
                colorButtonBg = colorDarkButtonBg;
                colorTextBoxBg = colorDarkTextBoxBg;
                colorCboBg = colorDarkCboBg;
            }

            // --- change

            // window
            grid.Background = colorWindowBg;

            // labels
            foreach (Label label in FindVisualChildren<Label>(this))
            {
                label.Foreground = colorTextFg;
            }
            // buttons
            foreach (Button button in FindVisualChildren<Button>(this))
            {                
                button.Background = colorButtonBg;                
            }
            // combos
            foreach (ComboBox cbo in FindVisualChildren<ComboBox>(this))
            {
                //cbo.Foreground = colorTextFg;
                //cbo.Background = colorCboBg;
            }

            // textboxes
            foreach (TextBox tb in FindVisualChildren<TextBox>(this))
            {
                //tb.Foreground = colorTextFg;
                tb.Background = colorTextBoxBg;
            }

            // checkboxes
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(this))
            {
                cb.Foreground = colorTextFg;
            }

            // listboxes
            foreach (ListBox lb in FindVisualChildren<ListBox>(this))
            {
                lb.Background = colorTextBoxBg;
            }
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        // --- grid
        private void InitGrids()
        {
            Brush brush = new SolidColorBrush(Colors.Orange);
            double thickness = 0.8;

            InitCanvasGrid(canvasColor, 4, 8, brush, thickness);
            InitCanvasGrid(canvasDepth, 4, 8, brush, thickness);
            InitCanvasGrid(canvasPS3Eye, 4, 8, brush, thickness);

            SwitchVisibilityGrids();
        }
        private void InitCanvasGrid(Canvas canvas, int cols, int rows, Brush brush, double thickness)
        {
            int w = (int)canvas.Width;
            int h = (int)canvas.Height;

            int colSize = w / cols;

            for (int i = 1; i < cols; i++)
            {
                Line line = new Line();
                line.Y1 = 0;
                line.Y2 = h;
                line.X1 = colSize * i;
                line.X2 = line.X1;
                line.Stroke = brush;
                line.StrokeThickness = thickness;
                canvas.Children.Add(line);
            }

            int rowsSize = h / rows;

            for (int i = 1; i < rows; i++)
            {
                Line line = new Line();
                line.Y1 = rowsSize * i;
                line.Y2 = line.Y1;
                line.X1 = 0;
                line.X2 = w;
                line.Stroke = brush;
                line.StrokeThickness = thickness;
                canvas.Children.Add(line);
            }
        }
        public void SwitchVisibilityGrids()
        {
            bool displayGrid = Context.DisplayGrid;
            
            canvasColor.Visibility = displayGrid ? Visibility.Visible : Visibility.Collapsed;
            canvasDepth.Visibility = displayGrid ? Visibility.Visible : Visibility.Collapsed;
            canvasPS3Eye.Visibility = displayGrid ? Visibility.Visible : Visibility.Collapsed;
        }
     
        // --- quick concept
        public void SwitchVisibilityQuickConcept()
        {
            var vis = Context.DisplayPerformance ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;

            cboQuickConcept.Visibility = vis;
            labelQuickAndPerf.Visibility = vis;
            lbQuickConcept.Visibility = vis;
            
        }

        private void InitQuickConcept()
        {            
            string path = "quick_action";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            foreach (var file in Directory.GetFiles(path))
            {
                string list = Path.GetFileNameWithoutExtension(file);
                List<string> concepts = ReadConcepts(file);

                dctQuickConcepts.Add(list, concepts);
                cboQuickConcept.Items.Add(list);
            }

            if (cboQuickConcept.Items.Count > 0)
            {
                cboQuickConcept.SelectedIndex = 0;
            }

            SwitchVisibilityQuickConcept();
        }
        private List<string> ReadConcepts(string filePath)
        {
            List<string> result = new List<string>();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null) result.Add(line);
            }
            return result;
        }
        private void lbQuickConcept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbQuickConcept.SelectedItem != null) recorderPanel.Concept = lbQuickConcept.SelectedItem.ToString();
        }
        private void cboQuickConcept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbQuickConcept.Items.Clear();
            string list = cboQuickConcept.SelectedItem as string;
            foreach (var concept in dctQuickConcepts[list])
            {
                lbQuickConcept.Items.Add(concept);
            }
        }

    }
}
