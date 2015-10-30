﻿#pragma checksum "..\..\ContextPanel.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F3D79DE840923C385CA0AACCCD81A01B"
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
    /// ContextPanel
    /// </summary>
    public partial class ContextPanel : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\ContextPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cboDisplayEveryNthFrame;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\ContextPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayKinectColor;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\ContextPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayKinectDepth;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\ContextPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbDisplayPS3Eye;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiSourceAcqMain;component/contextpanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ContextPanel.xaml"
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
            this.cboDisplayEveryNthFrame = ((System.Windows.Controls.ComboBox)(target));
            
            #line 9 "..\..\ContextPanel.xaml"
            this.cboDisplayEveryNthFrame.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cboDisplayEveryNthFrame_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cbDisplayKinectColor = ((System.Windows.Controls.CheckBox)(target));
            
            #line 11 "..\..\ContextPanel.xaml"
            this.cbDisplayKinectColor.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectColor_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 11 "..\..\ContextPanel.xaml"
            this.cbDisplayKinectColor.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectColor_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cbDisplayKinectDepth = ((System.Windows.Controls.CheckBox)(target));
            
            #line 13 "..\..\ContextPanel.xaml"
            this.cbDisplayKinectDepth.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectDepth_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 13 "..\..\ContextPanel.xaml"
            this.cbDisplayKinectDepth.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayKinectDepth_CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cbDisplayPS3Eye = ((System.Windows.Controls.CheckBox)(target));
            
            #line 15 "..\..\ContextPanel.xaml"
            this.cbDisplayPS3Eye.Unchecked += new System.Windows.RoutedEventHandler(this.cbDisplayPS3Eye_CheckedChanged);
            
            #line default
            #line hidden
            
            #line 15 "..\..\ContextPanel.xaml"
            this.cbDisplayPS3Eye.Checked += new System.Windows.RoutedEventHandler(this.cbDisplayPS3Eye_CheckedChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

