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
                //case nameof(inv.SFT_NACENKA_DILERA):
                //    e.Column.ReadOnly = ctx.Document.Diler == null;
                //    if(e.Column.EditSettings is CalcEditSettings ed)
                //        ed.IsTextEditable = ctx.Document.Diler == null;
                //    break;
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

        private void BaseEdit_OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (viewModel == null) return;
            var colPrice = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
            var colSumma = gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
            if (colPrice == null || colSumma == null) return;
            colPrice.ReadOnly = false;
            colSumma.ReadOnly = true;
        }

        private void GridRows_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            gridRows.TotalSummary.Clear();
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Count,
                ShowInColumn = "Nomenkl",
                DisplayFormat = "{0:n0}",
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
            if (DataContext is ClientWindowViewModel ctx)
            {
                ctx.Document.RaisePropertyChanged("DilerSumma");
            }
        }

        private void tableViewRows_ShownEditor(object sender, EditorEventArgs e)
        {
            if (e.Column.FieldName == "SFT_NACENKA_DILERA")
            {
                if (DataContext is ClientWindowViewModel ctx)
                {
                    e.Editor.IsReadOnly = ctx.Document.Diler == null;
                    e.Column.ReadOnly = ctx.Document.Diler == null;
                }
            }
        }
    }
}