﻿using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.KursReferences.UC
{
    /// <summary>
    ///     Interaction logic for SelectKursMainMenuItem.xaml
    /// </summary>
    public partial class SelectKursMainMenuItem
    {
        public SelectKursMainMenuItem()
        {
            InitializeComponent();
            
        }

        private void gridKursMenuItem_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void gridKursMenuItem_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
        }
    }
}
