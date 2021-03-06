﻿#pragma checksum "..\..\ContextDisplayPanel.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "28A2A3CC2530A39B198E5B6EFEB38048"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace MultiSourceAcqMain {
    
    
    /// <summary>
    /// ContextDisplayPanel
    /// </summary>
    public partial class ContextDisplayPanel : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayKinectColor;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayKinectDepth;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayPS3Eye;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayAccGloveGraph;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayAccGloveValues;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayPerformance;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayGrid;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDarkTheme;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\ContextDisplayPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cb8bitDepth;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MultiSourceAcqMain;component/contextdisplaypanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ContextDisplayPanel.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.cbDisplayKinectColor = ((System.Windows.Controls.CheckBox)(target));
            
            #line 8 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayKinectColor.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectColor_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 8 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayKinectColor.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectColor_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cbDisplayKinectDepth = ((System.Windows.Controls.CheckBox)(target));
            
            #line 10 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayKinectDepth.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectDepth_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 10 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayKinectDepth.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectDepth_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cbDisplayPS3Eye = ((System.Windows.Controls.CheckBox)(target));
            
            #line 12 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayPS3Eye.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayPS3Eye_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 12 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayPS3Eye.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayPS3Eye_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cbDisplayAccGloveGraph = ((System.Windows.Controls.CheckBox)(target));
            
            #line 14 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayAccGloveGraph.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayAccGloveGraph_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 14 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayAccGloveGraph.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayAccGloveGraph_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.cbDisplayAccGloveValues = ((System.Windows.Controls.CheckBox)(target));
            
            #line 16 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayAccGloveValues.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayAccGloveValues_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 16 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayAccGloveValues.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayAccGloveValues_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cbDisplayPerformance = ((System.Windows.Controls.CheckBox)(target));
            
            #line 18 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayPerformance.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayPerformance_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 18 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayPerformance.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayPerformance_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cbDisplayGrid = ((System.Windows.Controls.CheckBox)(target));
            
            #line 20 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayGrid.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayGrid_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 20 "..\..\ContextDisplayPanel.xaml"
            this.cbDisplayGrid.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayGrid_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.cbDarkTheme = ((System.Windows.Controls.CheckBox)(target));
            
            #line 22 "..\..\ContextDisplayPanel.xaml"
            this.cbDarkTheme.Unchecked += new System.Windows.RoutedEventHandler(this.cbDarkTheme_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 22 "..\..\ContextDisplayPanel.xaml"
            this.cbDarkTheme.Checked += new System.Windows.RoutedEventHandler(this.cbDarkTheme_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.cb8bitDepth = ((System.Windows.Controls.CheckBox)(target));
            
            #line 24 "..\..\ContextDisplayPanel.xaml"
            this.cb8bitDepth.Checked += new System.Windows.RoutedEventHandler(this.cb8bitDepth_Checked);
            
            #line default
            #line hidden
            
            #line 24 "..\..\ContextDisplayPanel.xaml"
            this.cb8bitDepth.Unchecked += new System.Windows.RoutedEventHandler(this.cb8bitDepth_Checked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

