﻿using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for StandartSearchView.xaml
    /// </summary>
    public partial class StandartSearchView
    {
        public StandartSearchView()
        {
            InitializeComponent();
        }

        private void GridDocuments_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            switch (e.Column.FieldName)
            {
                case "State":
                    e.Column.Visible = false;
                    break;
                case "Note":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        AcceptsReturn = true
                    };
                    break;
            }
        }

        private void GridDocuments_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
        }
    }
}