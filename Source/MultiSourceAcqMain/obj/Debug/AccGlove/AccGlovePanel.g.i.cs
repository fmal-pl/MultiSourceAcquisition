﻿#pragma checksum "..\..\..\AccGlove\AccGlovePanel.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D4C8D441789EB1AC3DDA1B4EC16E2FCC"
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


namespace MultiSourceAcqMain.AccGlove {
    
    
    /// <summary>
    /// AccGlovePanel
    /// </summary>
    public partial class AccGlovePanel : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lResolution;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cboFrequency;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label1;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cboPorts;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lCheck;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button bCheck;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\AccGlove\AccGlovePanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button bLevel;
        
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
            System.Uri resourceLocater = new System.Uri("/MultiSourceAcqMain;component/accglove/accglovepanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AccGlove\AccGlovePanel.xaml"
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
            this.lResolution = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.cboFrequency = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.label1 = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.cboPorts = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.lCheck = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.bCheck = ((System.Windows.Controls.Button)(target));
            
            #line 15 "..\..\..\AccGlove\AccGlovePanel.xaml"
            this.bCheck.Click += new System.Windows.RoutedEventHandler(this.bCheck_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.bLevel = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\AccGlove\AccGlovePanel.xaml"
            this.bLevel.Click += new System.Windows.RoutedEventHandler(this.bLevel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
