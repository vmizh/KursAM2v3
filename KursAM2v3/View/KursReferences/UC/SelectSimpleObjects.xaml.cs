﻿using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.KursReferences.UC
{
    /// <summary>
    ///     Interaction logic for SelectSimpleObjects.xaml
    /// </summary>
    public partial class SelectSimpleObjects : ILayout
    {
        public SelectSimpleObjects()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, gridControlStore);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}
