﻿using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;

namespace KursAM2.View.DialogUserControl.Invoices.UserControls
{
    /// <summary>
    ///     Interaction logic for InvoiceListSearchMultipleView.xaml
    /// </summary>
    public partial class InvoiceListSearchMultipleView
    {
        public InvoiceListSearchMultipleView()
        {
            InitializeComponent();
        }

        private void GridControlSearch_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (e.Column.Name == "IsSelected")
            {
                var checkEdit = new CheckEditSettings
                {
                    IsThreeState = true
                };
                e.Column.EditSettings = checkEdit;
            }
        }

        private void GridControlSearch_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
        }

        private void GridControlPosition_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void GridControlPosition_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
        }

        private void LayoutGroup_SelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            if (e.NewValue is LayoutGroup g)
                if (g.Name == "SelectedGroup")
                    if (DataContext is InvoiceClientSearchDialogViewModel dtx)
                        dtx.RaisePropertyChanged("SelectedItems");
        }

        private void gridViewSelected_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (DataContext is IUpdatechildItems dtx) dtx.UpdateSelectedItems();
        }

        private void gridViewPosition_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (DataContext is IUpdatechildItems dtx)
            {
                dtx.UpdatePositionItem();
                dtx.UpdateVisible();
            }
        }

        private void gridViewSearch_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (DataContext is IUpdatechildItems dtx)
            {
                dtx.UpdateInvoiceItem();
                dtx.UpdateVisible();
            }

            gridControlSearch.View.CloseEditor();
        }
    }
}
