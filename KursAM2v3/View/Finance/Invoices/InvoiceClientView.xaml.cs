using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Invoices;
using Core.WindowsManager;
using DevExpress.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.ViewModel.Finance.Invoices;

namespace KursAM2.View.Finance.Invoices
{
    /// <summary>
    ///     Interaction logic for InvoiceProvider.xaml
    /// </summary>
    public partial class InvoiceClientView
    {
        //public ButtonEdit KontrSelectButton;

        public InvoiceClientView()
        {
            InitializeComponent();
            
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ComboBoxEdit CurrencyItem { set; get; }
        private ClientWindowViewModel viewModel => DataContext as ClientWindowViewModel;

        //private void DelBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    var ctx = DataContext as ClientWindowViewModel;
        //    var doc = ctx?.Document;
        //    if (doc == null)
        //        return;
        //    doc.PersonaResponsible = null;
        //}

        //private void DilerBtnCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    var ctx = DataContext as ClientWindowViewModel;
        //    var doc = ctx?.Document;
        //    if (doc == null)
        //        return;
        //    doc.Diler = null;
        //    doc.SF_DILER_CRS_DC = null;
        //    doc.SF_DILER_SUMMA = 0;
        //    foreach (var r in doc.Rows) r.SFT_NACENKA_DILERA = 0;
        //}

        //private void Kontr_DefaultButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var ctx = DataContext as ClientWindowViewModel;
        //    var doc = ctx?.Document;
        //    if (doc == null)
        //        return;
        //    if (doc.ShipmentRows.Count > 0)
        //    {
        //        WindowManager.ShowMessage("По счету есть расходные накладные. Изменить контрагента нельзя.",
        //            "Предупреждение", MessageBoxImage.Information);
        //        return;
        //    }

        //    if (doc.PaySumma != 0)
        //    {
        //        WindowManager.ShowMessage("По счету есть Оплата. Изменить контрагента нельзя.",
        //            "Предупреждение", MessageBoxImage.Information);
        //        return;
        //    }

        //    var kontr = StandartDialogs.SelectKontragent(doc.Currency);
        //    if (kontr == null) return;
        //    if (doc.Rows.Any(_ => !_.IsUsluga && _.Nomenkl.Currency.DocCode != kontr.BalansCurrency.DocCode))
        //    {
        //        WindowManager.ShowMessage(
        //            "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
        //            "Предупреждение", MessageBoxImage.Information);
        //        return;
        //    }

        //    switch ((sender as ButtonEdit)?.Tag)
        //    {
        //        case "Client":
        //            doc.Client = kontr;
        //            doc.Currency = kontr.BalansCurrency;
        //            doc.Entity.SF_KONTR_CRS_RATE = 1;
        //            break;
        //        case "Receiver":
        //            doc.Receiver = kontr;
        //            break;
        //        case "Diler":
        //            doc.Diler = kontr;
        //            doc.SF_DILER_CRS_DC = kontr.BalansCurrency.DocCode;
        //            doc.SF_DILER_SUMMA = 0;
        //            doc.SF_DILER_RATE = 1;
        //            break;
        //    }
        //}

        private void GridRows_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(e.Column.FieldType))
                e.Column.EditSettings = new CalcEditSettings
                {
                    DisplayFormat = "n2",
                    Name = e.Column.FieldName + "Calc"
                };
            var ctx = DataContext as ClientWindowViewModel;
            var doc = ctx?.Document;
            if (doc == null)
                return;
            // ReSharper disable once LocalNameCapturedOnly
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once EntityNameCapturedOnly.Local
            var inv = new InvoiceClientRow();
            switch (e.Column.Name)
            {
                case nameof(inv.Nomenkl):
                    var nomenklEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    nomenklEdit.DefaultButtonClick += Nomenkl_DefaultButtonClick;
                    e.Column.EditSettings = nomenklEdit;
                    break;
                case nameof(inv.Note):
                    break;
                case nameof(inv.SDRSchet):
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        ItemsSource = MainReferences.SDRSchets.Values,
                        DisplayMember = "Name",
                        AutoComplete = true
                    };
                    break;
            }
        }

        private void Nomenkl_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as ClientWindowViewModel;
            if (ctx?.CurrentRow == null) return;
            if (ctx.CurrentRow.Shipped > 0)
            {
                WindowManager.ShowMessage(this,
                    "По данной позиции произведена отгрузка. Смена номенклатуры невозможна.", "Предупреждение",
                    MessageBoxImage.Stop);
                return;
            }

            var doc = ctx.Document;
            if (doc == null)
                return;
            var nomenkls = StandartDialogs.SelectNomenkls();
            if (nomenkls == null || nomenkls.Count <= 0) return;
            ctx.CurrentRow.Nomenkl = nomenkls.First();
        }

        private void GridFacts_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(e.Column.FieldType))
                e.Column.EditSettings = new CalcEditSettings
                {
                    DisplayFormat = "n2",
                    Name = e.Column.FieldName + "Calc"
                };
        }

        private void GridPays_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(e.Column.FieldType))
                e.Column.EditSettings = new CalcEditSettings
                {
                    DisplayFormat = "n2",
                    Name = e.Column.FieldName + "Calc"
                };
        }

        private void GridRows_OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            if (viewModel == null) return;
            var colPrice = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
            var colSumma = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
            if (colPrice == null || colSumma == null) return;
            if (viewModel.Document.IsNDSIncludeInPrice)
            {
                colPrice.ReadOnly = true;
                colSumma.ReadOnly = false;
            }
            else
            {
                colPrice.ReadOnly = false;
                colSumma.ReadOnly = true;
            }
        }

        private void BaseEdit_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (viewModel == null) return;
            var colPrice = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
            var colSumma = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
            if (colPrice == null || colSumma == null) return;
            if (viewModel.Document.IsNDSIncludeInPrice)
            {
                colPrice.ReadOnly = true;
                colSumma.ReadOnly = false;
            }
            else
            {
                colPrice.ReadOnly = false;
                colSumma.ReadOnly = true;
            }
        }

        private void GridRows_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            gridRows.TotalSummary.Clear();
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Count,
                ShowInColumn = "Nomenkl",
                DisplayFormat = "{0:n2}",
                FieldName = "Nomenkl"
            });
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                ShowInColumn = "Quantity",
                DisplayFormat = "{0:n2}",
                FieldName = "Quantity"
            });
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                ShowInColumn = "Summa",
                DisplayFormat = "{0:n2}",
                FieldName = "Summa"
            });
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                ShowInColumn = "SFT_SUMMA_NDS",
                DisplayFormat = "{0:n2}",
                FieldName = "SFT_SUMMA_NDS"
            });

        }

        private void TableViewRows_OnCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            gridRows.UpdateTotalSummary();
        }

        private void gridRows_Loaded(object sender, RoutedEventArgs e)
        {
            var colNomenkl = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Nomenkl");
            if (colNomenkl != null)
            {
                if (colNomenkl.EditSettings == null)
                {
                    var nomenklEdit = new ButtonEditSettings
                    {
                        Name = "PART_Editor",
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false
                    };
                    nomenklEdit.DefaultButtonClick += Nomenkl_DefaultButtonClick;
                    colNomenkl.EditSettings = nomenklEdit;
                }
            }

            var colSDR = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SDRSchet");
            if (colSDR != null)
            {
                colSDR.EditSettings = new ComboBoxEditSettings
                {
                    ItemsSource = MainReferences.SDRSchets.Values,
                    DisplayMember = "Name",
                    AutoComplete = true
                };
            }
        }
    }
}