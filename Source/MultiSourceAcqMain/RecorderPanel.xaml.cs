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
using System.IO;
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

namespace MultiSourceAcqMain
{
    /// <summary>
    /// Interaction logic for RecorderPanel.xaml
    /// </summary>
    public partial class RecorderPanel : UserControl
    {
        public event Action ConceptChanged;

        public string Path 
        { 
            get { return tbPath.Text; } 
        }
        public string Concept
        {
            get { return tbConcept.Text; }
            set { tbConcept.Text = value; }
        }
        public string Person
        {
            get { return tbPerson.Text; }
        }
        public String Codec
        {
            get { return cboCodecs.SelectedValue.ToString(); }
        }
        public int Bitrate
        {
            get
            {
                try
                {
                    return int.Parse(tbBitrate.Text);
                }
                catch
                {
                    throw new Exception("Incorrect bitrate");
                }
            }
        }

        private System.Windows.Forms.FolderBrowserDialog dialog;
        private bool isInit;

        // --- init
        public RecorderPanel()
        {
            isInit = true;
            InitializeComponent();
            isInit = false;
            Loaded += new RoutedEventHandler(RecorderPanel_Loaded);
        }
        public void FillCodecCombo(List<string> codecs)
        {            
            codecs.Sort();
            codecs.ForEach(codec =>
            {
                cboCodecs.Items.Add(codec);
            });
        }

        private void RecorderPanel_Loaded(object sender, RoutedEventArgs e)
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            LoadSettings();
        }
        private void LoadSettings()
        {
            tbPath.Text = Settings.GetSetting("recorderPath");
            dialog.SelectedPath = tbPath.Text;
            cboCodecs.SelectedIndex = Settings.GetSettingInt("recorderCodec");
            tbConcept.Text = Settings.GetSetting("recorderConcept");
            tbPerson.Text = Settings.GetSetting("recorderPerson");
            tbBitrate.Text = Settings.GetSetting("recorderBitrate");
        }

        // --- gui
        private void bChooseDir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (Directory.Exists(dialog.SelectedPath))
                {
                    tbPath.Text = dialog.SelectedPath;
                }
            }
        }
        private void cboCodecs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) Settings.SetSetting("recorderCodec", cboCodecs.SelectedIndex.ToString());
        }
        private void tbPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInit) Settings.SetSetting("recorderPath", tbPath.Text);
        }
        private void tbConcept_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInit) Settings.SetSetting("recorderConcept", tbConcept.Text);
            OnConceptChanged();
        }
        private void tbPerson_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInit) Settings.SetSetting("recorderPerson", tbPerson.Text);
        }
        private void tbBitrate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInit)
            {
                int bitrate;
                if (int.TryParse(tbBitrate.Text, out bitrate))
                {
                    Settings.SetSetting("recorderBitrate", tbBitrate.Text);
                }
            }
        }

        private void OnConceptChanged()
        {
            if (ConceptChanged != null) ConceptChanged();
        }
        
    }
}
