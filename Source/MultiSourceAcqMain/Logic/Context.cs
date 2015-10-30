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
using System.Windows.Media;
using Microsoft.Kinect;
using WiTKoM.Common;

namespace MultiSourceAcqMain.Logic
{
    public class Context
    {
        public static MainWindow GUI { get; set; }        
        public static KinectSensor KinectSensor { get; set; }        

        public static int DepthWidth { get; set; }
        public static int DepthHeight { get; set; }

        public static int ColorWidth { get; set; }
        public static int ColorHeight { get; set; }

        public static int PsWidth { get; set; }
        public static int PsHeight { get; set; }

        public static int ColorResizedWidth { get; set; }
        public static int ColorResizedHeight { get; set; }        
        public static int ColorResizedWidthTmp { get; set; }
        public static BitmapScalingMode ResizeQuality { get; set; }        

        public static bool ProcessColorResize { get; set; } 
        public static bool ProcessColorMapping { get; set; }
        public static bool ProcessColorBitmap { get; set; }

        public static bool ProcessDepthData { get; set; }
        public static bool ProcessDepthMapping { get; set; }
        public static bool ProcessDepthBitmap { get; set; }

        public static bool ProcessBody { get; set; }
        public static bool ProcessPS3EyeBitmap { get; set; }

        public static bool StartKinect { get; set; }
        public static bool StartPS3Eye { get; set; }

        public static bool IsKinectActive { get; set; }
        public static bool IsPS3EyeActive { get; set; }

        private static bool displayColor;
        private static bool displayDepth;
        private static bool displayPS3Eye;
        private static bool displayAccGloveGraph;
        private static bool displayAccGloveValues;
        private static bool displayPerformance;
        private static bool displayGrid;
        private static bool darkTheme;
        private static bool use8bitDepth;

        public static bool DisplayColor
        {
            get { return Context.displayColor; }
            set { Context.displayColor = value; Settings.SetSetting("contextDisplayColor", value.ToString()); }
        }
        public static bool DisplayDepth
        {
            get { return Context.displayDepth; }
            set { Context.displayDepth = value; Settings.SetSetting("contextDisplayDepth", value.ToString()); }
        }
        public static bool DisplayPS3Eye
        {
            get { return Context.displayPS3Eye; }
            set { Context.displayPS3Eye = value; Settings.SetSetting("contextDisplayPS3Eye", value.ToString()); }
        }
        public static bool DisplayAccGloveGraph
        {
            get { return Context.displayAccGloveGraph; }
            set { Context.displayAccGloveGraph = value; Settings.SetSetting("contextDisplayAccGloveGraph", value.ToString()); }
        }
        public static bool DisplayAccGloveValues
        {
            get { return Context.displayAccGloveValues; }
            set { Context.displayAccGloveValues = value; Settings.SetSetting("contextDisplayAccGloveValues", value.ToString()); }
        }
        public static bool DisplayPerformance
        {
            get { return Context.displayPerformance; }
            set { Context.displayPerformance = value; Settings.SetSetting("contextDisplayPerformance", value.ToString()); }
        }
        public static bool DisplayGrid
        {
            get { return Context.displayGrid; }
            set { Context.displayGrid = value; Settings.SetSetting("contextDisplayGrid", value.ToString()); }
        }
        public static bool DarkTheme
        {
            get { return Context.darkTheme; }
            set { Context.darkTheme = value; Settings.SetSetting("contextDarkTheme", value.ToString()); }
        }
        public static bool Use8bitDepth 
        {
            get { return use8bitDepth; }
            set { Context.use8bitDepth = value; Settings.SetSetting("contextUse8bitDepth", value.ToString()); }
        }

        public static int DisplayEveryNthFrame { get; set; }

        static Context()
        {
            LoadSettings();
            
            // other settings are available to set in GUI
            StartKinect = true;
            StartPS3Eye = true;

            ProcessColorResize = true;
            ProcessColorBitmap = true;

            ProcessDepthData = true;
            ProcessDepthBitmap = true;

            ProcessBody = true;
            ProcessPS3EyeBitmap = true;

            // change here requires changes in ProcessingManager -> ProcessColorResize
            // (2 places -> color resize & depth resize)

            // no crop version 1
            //ColorResizedWidth = 640;
            //ColorResizedHeight = 360;

            // no crop version 2
            //ColorResizedWidth = 853;
            //ColorResizedHeight = 480;

            // crop version
            ColorResizedWidth = 640;
            ColorResizedHeight = 480;
            ColorResizedWidthTmp = 853;
            ResizeQuality = BitmapScalingMode.LowQuality;

            DisplayEveryNthFrame = 1;
            
            PsWidth = 640;
            PsHeight = 480;
        }

        private static void LoadSettings()
        {
            displayColor = Settings.GetSettingBool("contextDisplayColor");
            displayDepth = Settings.GetSettingBool("contextDisplayDepth");
            displayPS3Eye = Settings.GetSettingBool("contextDisplayPS3Eye");
            displayAccGloveGraph = Settings.GetSettingBool("contextDisplayAccGloveGraph");
            displayAccGloveValues = Settings.GetSettingBool("contextDisplayAccGloveValues");
            displayPerformance = Settings.GetSettingBool("contextDisplayPerformance");
            displayGrid = Settings.GetSettingBool("contextDisplayGrid");
            darkTheme = Settings.GetSettingBool("contextDarkTheme");
            use8bitDepth = Settings.GetSettingBool("contextUse8bitDepth");
        }
        
    }
}
