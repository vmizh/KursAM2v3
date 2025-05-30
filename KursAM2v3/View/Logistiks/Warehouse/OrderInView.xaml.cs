﻿using System.Linq;
using System.Windows;
using System.Windows.Media;
using DevExpress.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.View.Logistiks.Warehouse
{
    /// <summary>
    ///     Interaction logic for OrderInView.xaml
    /// </summary>
    public partial class OrderInView
    {
        //private readonly LayoutManagerGridAutoGenerationColumns gridRowsLayout;
        public OrderInView()
        {
            InitializeComponent();
        }

        private void GridLayoutHelper_Trigger(object sender, MyEventArgs e)
        {
            var maxWidith = new GridColumnWidth(800);

            if (e.LayoutChangedTypes.Contains(LayoutChangedType.ColumnWidth))
                if (sender is GridLayoutHelper m)
                    if (m.AssociatedObject is GridControl grid)
                        foreach (var col in grid.Columns)
                            if (col.Width.Value > maxWidith.Value)
                                col.Width = maxWidith;
        }

        private void GridRows_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(e.Column.FieldType))
                e.Column.EditSettings = new CalcEditSettings
                {
                    DisplayFormat = "n2",
                    Name = e.Column.FieldName + "Calc"
                };
            var ctx = DataContext as OrderInWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null)
                return;
            // ReSharper disable once LocalNameCapturedOnly
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once EntityNameCapturedOnly.Local
            var inv = new WarehouseOrderInRow();
            switch (e.Column.Name)
            {
                case nameof(inv.DocDate):
                case nameof(inv.DocInNum):
                case nameof(inv.DocExtNum):
                case nameof(inv.Warehouse):
                    e.Column.Visible = false;
                    break;
                case nameof(inv.NomNomenkl):
                    e.Column.ReadOnly = true;
                    break;
                case nameof(inv.Note):
                    e.Column.EditSettings = new MemoEditSettings
                    {
                        ShowIcon = false
                    };
                    break;
                case nameof(inv.SDRSchet):
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        ItemsSource = GlobalOptions.ReferencesCache.GetSDRSchetAll().Cast<SDRSchet>().ToList(),
                        DisplayMember = "Name",
                        AutoComplete = true
                    };
                    break;
                case nameof(inv.InvoiceProvider):
                    e.Column.EditSettings = new MemoEditSettings
                    {
                        ShowIcon = false,
                        MemoTextWrapping = TextWrapping.Wrap
                    };
                    break;
            }
        }

        private void GridRows_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            if (gridRows.Bands != null && gridRows.Bands.Count > 0)
            {
                var bandid = 1;
                foreach (var b in gridRows.Bands)
                {
                    b.Name = "band" + bandid;
                    bandid++;
                }
            }

            gridRows.TotalSummary.Clear();
            foreach (var col in gridRows.Columns)
                if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                {
                    var summary = new GridSummaryItem
                    {
                        SummaryType = SummaryItemType.Sum,
                        ShowInColumn = col.FieldName,
                        DisplayFormat = "{0:n2}",
                        FieldName = col.FieldName
                    };
                    gridRows.TotalSummary.Add(summary);
                }
        }

        private void Nomenkl_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as OrderInWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null)
                return;
            var nomenkls = StandartDialogs.SelectNomenkls();
            if (nomenkls == null || nomenkls.Count <= 0) return;
            ctx.CurrentRow.Nomenkl = nomenkls.First();
        }

        private void ComboBoxEdit_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (DataContext is OrderInWindowViewModel dtx)
            {
                dtx.Document.KontragentSender = null;
                dtx.Document.WarehouseOut = null;
            }
        }

        private void State_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            switch ((RowStatus)(e.NewValue ?? RowStatus.NotEdited))
            {
                case RowStatus.NewRow:
                    ((ComboBoxEdit)sender).Foreground = Brushes.Red;
                    break;
                case RowStatus.NotEdited:
                    ((ComboBoxEdit)sender).Foreground = Brushes.Green;
                    break;
                case RowStatus.Edited:
                    ((ComboBoxEdit)sender).Foreground = Brushes.Blue;
                    break;
                default:
                    ((ComboBoxEdit)sender).Foreground = Brushes.Black;
                    break;
            }

            e.Handled = true;
        }
    }
}
