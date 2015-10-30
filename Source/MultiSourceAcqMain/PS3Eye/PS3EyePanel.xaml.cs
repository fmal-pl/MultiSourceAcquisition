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
using System.Linq;
using System.Text;
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
using WiTKoM.Common;

namespace MultiSourceAcqMain.PS3Eye
{
    /// <summary>
    /// Interaction logic for PS3EyePanel.xaml
    /// </summary>
    public partial class PS3EyePanel : UserControl
    {
        public event Action ResolutionChanged;
        public event Action FramerateChanged;
        public event Action ExposureChanged;
        public event Action GainChanged;
        public event Action WhiteBalanceChanged;

        // device properties
        public bool IsAutoExposure
        {
            get { return vsExp.IsAuto; }
            set { vsExp.IsAuto = value; }
        }
        public int Exposure
        {
            get { return (int)vsExp.Value; }
            set { vsExp.Value = value; }
        }

        public bool IsAutoGain
        {
            get { return vsGain.IsAuto; }
            set { vsGain.IsAuto = value; }
        }
        public int Gain
        {
            get { return (int)vsGain.Value; }
            set { vsGain.Value = value; }
        }

        public bool IsAutoWhiteBalance
        {
            get { return vsWhiteBalance.IsAuto; }
            set { vsWhiteBalance.IsAuto = value; }
        }
        public int WhiteBalance1
        {
            get { return (int)vsWhiteBalance.Value1; }
            set { vsWhiteBalance.Value1 = value; }
        }
        public int WhiteBalance2
        {
            get { return (int)vsWhiteBalance.Value2; }
            set { vsWhiteBalance.Value2 = value; }
        }
        public int WhiteBalance3
        {
            get { return (int)vsWhiteBalance.Value3; }
            set { vsWhiteBalance.Value3 = value; }
        }

        private List<int> lstFrameRates = new List<int>();

        public int FrameRate
        {
            get 
            { 
                int framerate = 0;
                Dispatcher.Invoke(() => framerate = int.Parse(cboFramerate.SelectedValue.ToString()));
                return framerate;
            }
            set
            {
                cboFramerate.SelectedIndex = value;
            }
        }
        public CLEyeCameraResolution Resolution
        {
            get { return (cboResolution.SelectedIndex == 0 ? CLEyeCameraResolution.CLEYE_QVGA : CLEyeCameraResolution.CLEYE_VGA); }
            set { cboResolution.SelectedIndex = (value == CLEyeCameraResolution.CLEYE_QVGA ? 0 : 1); }
        }

        private bool isInitializing;

        // constructor & public
        public PS3EyePanel()
        {
            isInitializing = true;
            InitializeComponent();

            // fill resolution combos
            cboResolution.Items.Add("320x240");
            cboResolution.Items.Add("640x480");

            // fill fps combos
            lstFrameRates.Add(15);
            lstFrameRates.Add(25);
            lstFrameRates.Add(30);
            lstFrameRates.Add(50);
            lstFrameRates.Add(60);
            lstFrameRates.ForEach(val => cboFramerate.Items.Add(val.ToString()));

            // load settings
            LoadSettings();
            isInitializing = false;

            // todo?   
            // LEYE_QVGA - 15, 30, 60, 75, 100, 125
            // CLEYE_VGA - 15, 30, 40, 50, 60, 75
            SetMode(false);

        }
        private void LoadSettings()
        {
            cboFramerate.SelectedIndex = Settings.GetSettingInt("PS3EyePanelFramerate");
            cboResolution.SelectedIndex = Settings.GetSettingInt("PS3EyePanelResolution");

            vsExp.Value = Settings.GetSettingInt("PS3EyePanelExp");
            vsExp.IsAuto = Settings.GetSettingInt("PS3EyePanelExpAuto") == 1;

            vsGain.Value = Settings.GetSettingInt("PS3EyePanelGain");
            vsGain.IsAuto = Settings.GetSettingInt("PS3EyePanelGainAuto") == 1;

            vsWhiteBalance.Value1 = Settings.GetSettingInt("PS3EyePanelWhiteBalanceR");
            vsWhiteBalance.Value2 = Settings.GetSettingInt("PS3EyePanelWhiteBalanceG");
            vsWhiteBalance.Value3 = Settings.GetSettingInt("PS3EyePanelWhiteBalanceB");
            vsWhiteBalance.IsAuto = Settings.GetSettingInt("PS3EyePanelWhiteBalanceAuto") == 1;
        }
        public void SetMode(bool isDeviceRunning)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                cboResolution.IsEnabled = !isDeviceRunning;
                cboFramerate.IsEnabled = !isDeviceRunning;

                lResolution.IsEnabled = !isDeviceRunning;
                lFramerate.IsEnabled = !isDeviceRunning;

                vsExp.IsEnabled = isDeviceRunning;
                vsGain.IsEnabled = isDeviceRunning;
                vsWhiteBalance.IsEnabled = isDeviceRunning;

                lExp.IsEnabled = isDeviceRunning;
                lGain.IsEnabled = isDeviceRunning;
                lWhite.IsEnabled = isDeviceRunning;
                lBalance.IsEnabled = isDeviceRunning;
                lR.IsEnabled = isDeviceRunning;
                lG.IsEnabled = isDeviceRunning;
                lB.IsEnabled = isDeviceRunning;

                if (isDeviceRunning)
                {
                    //OnResolutionChanged();
                    OnExposureChanged();
                    OnFramerateChanged();
                    OnGainChanged();
                }                

            }));
        }

        #region event raisers

        public void OnResolutionChanged()
        {
            if (ResolutionChanged != null) ResolutionChanged();
        }
        public void OnFramerateChanged()
        {
            if (FramerateChanged != null) FramerateChanged();
        }
        public void OnExposureChanged()
        {
            if (ExposureChanged != null) ExposureChanged();
        }
        public void OnGainChanged()
        {
            if (GainChanged != null) GainChanged();
        }
        public void OnWhiteBalanceChanged()
        {
            if (WhiteBalanceChanged != null) WhiteBalanceChanged();
        }

        #endregion

        #region event handlers

        private void cboResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnResolutionChanged();
            Settings.SetSetting("PS3EyePanelResolution", cboResolution.SelectedIndex.ToString());
        }
        private void cboFramerate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnFramerateChanged();
            Settings.SetSetting("PS3EyePanelFramerate", cboFramerate.SelectedIndex.ToString());
        }

        private void vsExp_Changed()
        {
            if (!isInitializing)
            {
                OnExposureChanged();
                Settings.SetSetting("PS3EyePanelExp", ((int)vsExp.Value).ToString());
                Settings.SetSetting("PS3EyePanelExpAuto", (vsExp.IsAuto ? 1 : 0).ToString());
            }
        }
        private void vsGain_Changed()
        {
            if (!isInitializing)
            {
                OnGainChanged();
                Settings.SetSetting("PS3EyePanelGain", ((int)vsGain.Value).ToString());
                Settings.SetSetting("PS3EyePanelGainAuto", (vsGain.IsAuto ? 1 : 0).ToString());
            }
        }
        private void vsWhiteBalance_ValueOrAutoChanged()
        {
            if (!isInitializing)
            {
                OnWhiteBalanceChanged();
                Settings.SetSetting("PS3EyePanelWhiteBalanceR", ((int)vsWhiteBalance.Value1).ToString());
                Settings.SetSetting("PS3EyePanelWhiteBalanceG", ((int)vsWhiteBalance.Value2).ToString());
                Settings.SetSetting("PS3EyePanelWhiteBalanceB", ((int)vsWhiteBalance.Value3).ToString());
                Settings.SetSetting("PS3EyePanelWhiteBalanceAuto", (vsWhiteBalance.IsAuto ? 1 : 0).ToString());
            }
        }

        #endregion
    }
}
