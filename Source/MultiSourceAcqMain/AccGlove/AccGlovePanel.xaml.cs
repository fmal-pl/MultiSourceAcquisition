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
using System.IO.Ports;
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

namespace MultiSourceAcqMain.AccGlove
{
    /// <summary>
    /// Interaction logic for AccGlovePanel.xaml
    /// </summary>
    public partial class AccGlovePanel : UserControl
    {
        private string[] ports;
        public string SelectedPort
        {
            get
            {
                if (ports == null || ports.Length == 0) return null;
                return ports[cboPorts.SelectedIndex];
            }
        }
        public bool DoLevel { get; set; }
        public AccGloveManager AccGloveManager { get; set; }
        public bool IsCheckEnabled
        {
            get { return bCheck.IsEnabled; }
            set { bCheck.IsEnabled = value; }
        }

        public AccGlovePanel()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(AccGlovePanel_Loaded);
        }
        private void AccGlovePanel_Loaded(object sender, RoutedEventArgs e)
        {
            cboFrequency.Items.Add("400Hz");
            cboFrequency.SelectedIndex = 0;

            ports = SerialPort.GetPortNames();
            foreach (var p in ports) cboPorts.Items.Add(p);
            if (cboPorts.Items.Count > 0) cboPorts.SelectedIndex = 0;
        }

        public void SetInfo(string info)
        {
            lCheck.Content = info;
        }
        public void ClearCheckLabel()
        {
            lCheck.Content = "";
        }

        private void bLevel_Click(object sender, RoutedEventArgs e)
        {
            DoLevel = true;
        }
        private void bCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lCheck.Content = "";
                lCheck.Content = AccGloveManager.Check();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
