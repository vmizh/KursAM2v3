﻿using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Grid;


namespace KursAM2.View.Projects
{
    /// <summary>
    /// Interaction logic for SelectProjectDialogView.xaml
    /// </summary>
    public partial class SelectProjectDialogView : ThemedWindow
    {
        public SelectProjectDialogView()
        {
            InitializeComponent();
        }

        private void GridProjects_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
        }

        private void GridProjects_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
        }
    }
}
