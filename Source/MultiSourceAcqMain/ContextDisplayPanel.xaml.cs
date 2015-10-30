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
using MultiSourceAcqMain.Logic;

namespace MultiSourceAcqMain
{
    /// <summary>
    /// Interaction logic for ContextDisplayPanel.xaml
    /// </summary>
    public partial class ContextDisplayPanel : UserControl
    {
        private bool isInit;

        public ContextDisplayPanel()
        {
            isInit = true;
            InitializeComponent();
            Init();
            isInit = false;
        }

        private void Init()
        {
            // --- display bitmaps
            cbDisplayKinectColor.IsChecked = Context.DisplayColor;
            cbDisplayKinectDepth.IsChecked = Context.DisplayDepth;
            cbDisplayPS3Eye.IsChecked = Context.DisplayPS3Eye;
            cbDisplayAccGloveGraph.IsChecked = Context.DisplayAccGloveGraph;
            cbDisplayAccGloveValues.IsChecked = Context.DisplayAccGloveValues;
            cbDisplayPerformance.IsChecked = Context.DisplayPerformance;
            cbDisplayGrid.IsChecked = Context.DisplayGrid;
            cbDarkTheme.IsChecked = Context.DarkTheme;
            cb8bitDepth.IsChecked = Context.Use8bitDepth;
        }

        private void cbDisplayKinectColor_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayColor = (bool)cbDisplayKinectColor.IsChecked;
        }
        private void cbDisplayKinectDepth_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayDepth = (bool)cbDisplayKinectDepth.IsChecked;
        }
        private void cbDisplayPS3Eye_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayPS3Eye = (bool)cbDisplayPS3Eye.IsChecked;
        }
        private void cbDisplayAccGloveGraph_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayAccGloveGraph = (bool)cbDisplayAccGloveGraph.IsChecked;
        }
        private void cbDisplayAccGloveValues_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayAccGloveValues = (bool)cbDisplayAccGloveValues.IsChecked;
        }
        private void cbDisplayPerformance_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayPerformance = (bool)cbDisplayPerformance.IsChecked;
            if (!isInit) Context.GUI.SwitchVisibilityQuickConcept();
        }
        private void cbDisplayGrid_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DisplayGrid = (bool)cbDisplayGrid.IsChecked;
            if (!isInit) Context.GUI.SwitchVisibilityGrids();
        }
        private void cbDarkTheme_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Context.DarkTheme = (bool)cbDarkTheme.IsChecked;
            if (!isInit) Context.GUI.LoadTheme();
        }
        private void cb8bitDepth_Checked(object sender, RoutedEventArgs e)
        {
            Context.Use8bitDepth = (bool)cb8bitDepth.IsChecked;
            Context.GUI.Update8bitDepthInfo();
        }
    }
}
