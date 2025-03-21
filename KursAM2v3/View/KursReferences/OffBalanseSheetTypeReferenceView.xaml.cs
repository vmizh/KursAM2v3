﻿using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for OffBalanseSheetTypeReferenceView.xaml
    /// </summary>
    public partial class OffBalanseSheetTypeReferenceView : ILayout
    {
        public OffBalanseSheetTypeReferenceView()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, null);
            Loaded += myLoaded;
            Closing += myClosing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void myClosing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void myLoaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
