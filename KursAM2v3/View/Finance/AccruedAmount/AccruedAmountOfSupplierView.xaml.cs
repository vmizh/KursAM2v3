using System.Linq;
using System.Windows;
using System.Windows.Data;
using Core;
using DevExpress.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursAM2.View.KursReferences;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursAM2.ViewModel.Reference;

namespace KursAM2.View.Finance.AccruedAmount
{
    /// <summary>
    ///     Interaction logic for AccruedAmountOfSupplierView.xaml
    /// </summary>
    public partial class AccruedAmountOfSupplierView
    {
        private ComboBoxEditSettings typeEdit;

        public AccruedAmountOfSupplierView()
        {
            InitializeComponent();
        }

        private void GridRows_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            var ctx = DataContext as AccruedAmountOfSupplierWindowViewModel;
            e.Column.Name = e.Column.FieldName;
            switch (e.Column.FieldName)
            {
                case "SDRSchet":
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        IsTextEditable = false,
                        ItemsSource = MainReferences.SDRSchets.Values.ToList()
                    };
                    break;
                case "Summa":
                    e.Column.EditSettings = new CalcEditSettings
                    {
                        DisplayFormat = "n2",
                        MaskUseAsDisplayFormat = true
                    };
                    break;
                case "AccruedAmountType":
                    typeEdit = new ComboBoxEditSettings
                    {
                        Name = "PART_Editor",
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false,
                        AutoComplete = true
                    };
                    typeEdit.DefaultButtonClick += TypeEdit_DefaultButtonClick;
                    var bn = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Edit
                    };
                    bn.Click += Bn_Click;
                    typeEdit.Buttons.Add(bn);
                    e.Column.EditSettings = typeEdit;
                    break;
                case "CashDoc":
                    var cashEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false,
                        AllowDefaultButton = false,
                        AcceptsReturn = true
                    };
                    var buttonInfoAdd = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Plus,
                        ToolTip = "Создать приходный кассовый ордер",
                        
                    };
                    var buttonInfoOpen = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Edit,
                        ToolTip = "Открыть приходный кассовый ордер"
                    };
                    var buttonInfoDelete = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Cancel,
                        ToolTip = "Удалить связь с приходным кассовым ордером",
                        
                    };
                    if (ctx != null)
                    {
                        buttonInfoAdd.SetBinding(CommandButtonInfo.CommandProperty, new Binding("AddCashDocCommand"));
                        buttonInfoOpen.SetBinding(CommandButtonInfo.CommandProperty, new Binding("OpenCashDocCommand"));
                        buttonInfoDelete.SetBinding(CommandButtonInfo.CommandProperty, new Binding("DeleteCashDocCommand"));
                        cashEdit.Buttons.Add(buttonInfoAdd);
                        cashEdit.Buttons.Add(buttonInfoOpen);
                        cashEdit.Buttons.Add(buttonInfoDelete);
                    }
                    e.Column.EditSettings = cashEdit;
                    break;
                case "BankDoc":
                    var bankEdit = new ButtonEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsTextEditable = false,
                        AllowDefaultButton = false,
                        AcceptsReturn = true
                    };
                    var buttonInfoAdd2 = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Plus,
                        ToolTip = "Создать банковскую транзакцию",
                        
                    };
                    var buttonInfoOpen2 = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Edit,
                        ToolTip = "Открыть банковскую транзакцию"
                    };
                    var buttonInfoDelete2 = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Cancel,
                        ToolTip = "Удалить банковскую транзакцию",
                        
                    };
                    if (ctx != null)
                    {
                        buttonInfoAdd2.SetBinding(CommandButtonInfo.CommandProperty, new Binding("AddBankDocCommand"));
                        buttonInfoOpen2.SetBinding(CommandButtonInfo.CommandProperty, new Binding("OpenBankDocCommand"));
                        buttonInfoDelete2.SetBinding(CommandButtonInfo.CommandProperty, new Binding("DeleteBankDocCommand"));
                        bankEdit.AllowDefaultButton = false;
                        bankEdit.Buttons.Add(buttonInfoAdd2);
                        bankEdit.Buttons.Add(buttonInfoOpen2);
                        bankEdit.Buttons.Add(buttonInfoDelete2);
                    }
                    e.Column.EditSettings = bankEdit;
                    break;
            }
        }
        private void TypeEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            typeEdit.ItemsSource = MainReferences.GetAllAccruedAmountType();
        }

        private void Bn_Click(object sender, RoutedEventArgs e)
        {
            var aat = new AccruedAmountTypeWindowViewModel();
            var form = new AccruedAmountTypeView
            {
                Owner = Application.Current.MainWindow,
                DataContext = aat
            };
            aat.Form = form;
            form.Show();
        }

        private void GridRows_OnAutoGeneratedColumns(object sender, RoutedEventArgs e)
        {
            gridRows.TotalSummary.Add(new GridSummaryItem
            {
                FieldName = "Summa",
                SummaryType = SummaryItemType.Sum,
                DisplayFormat = "n2"
            });
        }

        private void TableViewRows_OnCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            //gridRows.UpdateTotalSummary();
        }

        private void TableViewRows_OnCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            gridRows.UpdateTotalSummary();
        }
    }
}