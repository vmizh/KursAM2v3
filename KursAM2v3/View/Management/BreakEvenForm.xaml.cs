using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Core;
using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.ViewModel.Management.BreakEven;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for BreakEvenForm.xaml
    /// </summary>
    public partial class BreakEvenForm : ILayout
    {
        public BreakEvenForm()
        {
            InitializeComponent(); ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            NomGroup = new BreakEvenNomGroupViewModel();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += BreakEvenForm_Closing;
            Loaded += BreakEvenForm_Loaded;
            Title = "Рентабельность. База данных " + GlobalOptions.DataBaseName;
        }

        public BreakEvenNomGroupViewModel NomGroup { set; get; }
        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void BreakEvenForm_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            gridDocument.TotalSummary.Clear();
            foreach (var col in gridDocument.Columns)
            {
                if (!KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType)) continue;
                var summary = new GridSummaryItem
                {
                    SummaryType = SummaryItemType.Sum,
                    ShowInColumn = col.FieldName,
                    DisplayFormat = "{0:n2}",
                    FieldName = col.FieldName
                };
                gridDocument.TotalSummary.Add(summary);
            }
            gridNomenkl.TotalSummary.Clear();
            foreach (var col in gridNomenkl.Columns)
            {
                if (!KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType)) continue;
                var summary = new GridSummaryItem
                {
                    SummaryType = SummaryItemType.Sum,
                    ShowInColumn = col.FieldName,
                    DisplayFormat = "{0:n2}",
                    FieldName = col.FieldName
                };
                gridNomenkl.TotalSummary.Add(summary);
            }
            gridKontr.TotalSummary.Clear();
            foreach (var col in gridKontr.Columns)
            {
                if (!KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType)) continue;
                var summary = new GridSummaryItem
                {
                    SummaryType = SummaryItemType.Sum,
                    ShowInColumn = col.FieldName,
                    DisplayFormat = "{0:n2}",
                    FieldName = col.FieldName
                };
                gridKontr.TotalSummary.Add(summary);
            }
            gridCO.TotalSummary.Clear();
            foreach (var col in gridCO.Columns)
            {
                if (!KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType)) continue;
                var summary = new GridSummaryItem
                {
                    SummaryType = SummaryItemType.Sum,
                    ShowInColumn = col.FieldName,
                    DisplayFormat = "{0:n2}",
                    FieldName = col.FieldName
                };
                gridCO.TotalSummary.Add(summary);
            }
            gridManager.TotalSummary.Clear();
            foreach (var col in gridManager.Columns)
            {
                if (!KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType)) continue;
                var summary = new GridSummaryItem
                {
                    SummaryType = SummaryItemType.Sum,
                    ShowInColumn = col.FieldName,
                    DisplayFormat = "{0:n2}",
                    FieldName = col.FieldName
                };
                gridManager.TotalSummary.Add(summary);
            }

            gridCO.SelectionMode = MultiSelectMode.None;
            gridCurrencyDocument.SelectionMode = MultiSelectMode.None;
            gridDocument.SelectionMode = MultiSelectMode.None;
            gridKontr.SelectionMode = MultiSelectMode.None;
            gridManager.SelectionMode = MultiSelectMode.None;
            gridNomenkl.SelectionMode = MultiSelectMode.None;
        }

        private void BreakEvenForm_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void NomenklTableView_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (!(DataContext is BreakEvenWindowViewModel ctx)) return;
            if (!(selectedItemChangedEventArgs.NewItem is BreakEvenNomGroupViewModel row)) return;
            ctx.UpdateDocForNomenkl(row.NomenklNumber);
            gridDocument.RefreshData();
            gridCurrencyDocument.RefreshData();
        }

        private void COTableView_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (!(DataContext is BreakEvenWindowViewModel ctx)) return;
            if (!(selectedItemChangedEventArgs.NewItem is CommonRow row)) return;
            ctx.DocumentGroup.Clear();
            ctx.DocumentCurrencyGroup.Clear();
            foreach (var d in ctx.DataAll.Where(t => t.CentrOfResponsibility.Name == row.Name))
            {
                ctx.DocumentGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = MainReferences.GetNomenkl(d.Nomenkl.DocCode),
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs
                });
                ctx.DocumentCurrencyGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = d.Nomenkl,
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs,
                    SummaOperNomenkl = d.SummaOperNomenkl,
                    KontrOperSummaCrs = d.KontrOperSummaCrs,
                    SummaOperNomenklCrs = d.SummaOperNomenklCrs,
                    ResultEUR =
                        d.OperCurrency.Name == "EUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultRUB =
                        d.OperCurrency.Name == "RUB" || d.OperCurrency.Name == "RUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultUSD =
                        d.OperCurrency.Name == "USD"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0
                });
            }
            gridDocument.RefreshData();
            gridCurrencyDocument.RefreshData();
        }

        private void ManagerTableView_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (!(DataContext is BreakEvenWindowViewModel ctx)) return;
            if (!(selectedItemChangedEventArgs.NewItem is CommonRow row)) return;
            ctx.DocumentGroup.Clear();
            ctx.DocumentCurrencyGroup.Clear();
            foreach (var d in ctx.DataAll.Where(t => t.Manager == row.Name))
            {
                ctx.DocumentGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = MainReferences.GetNomenkl(d.Nomenkl.DocCode),
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs
                });
                ctx.DocumentCurrencyGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = d.Nomenkl,
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs,
                    SummaOperNomenkl = d.SummaOperNomenkl,
                    KontrOperSummaCrs = d.KontrOperSummaCrs,
                    SummaOperNomenklCrs = d.SummaOperNomenklCrs,
                    ResultEUR =
                        d.OperCurrency.Name == "EUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultRUB =
                        d.OperCurrency.Name == "RUB" || d.OperCurrency.Name == "RUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultUSD =
                        d.OperCurrency.Name == "USD"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0
                });
            }
            gridDocument.RefreshData();
            gridCurrencyDocument.RefreshData();
        }

        public void KontrTableView_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (!(DataContext is BreakEvenWindowViewModel ctx)) return;
            if (!(selectedItemChangedEventArgs.NewItem is CommonRow row)) return;
            ctx.DocumentGroup.Clear();
            foreach (var d in ctx.DataAll.Where(t => t.Kontragent == row.Name))
            {
                ctx.DocumentGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = MainReferences.GetNomenkl(d.Nomenkl.DocCode),
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs
                });
                ctx.DocumentCurrencyGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = d.Nomenkl,
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.NomenklSumWOReval, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs,
                    SummaOperNomenkl = d.SummaOperNomenkl,
                    KontrOperSummaCrs = d.KontrOperSummaCrs,
                    SummaOperNomenklCrs = d.SummaOperNomenklCrs,
                    ResultEUR = d.OperCurrency.Name == "EUR" ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs, 2) : 0,
                    ResultRUB =
                        d.OperCurrency.Name == "RUB" || d.OperCurrency.Name == "RUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs, 2)
                            : 0,
                    ResultUSD = d.OperCurrency.Name == "USD" ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs, 2) : 0
                });
            }
            gridDocument.RefreshData();
        }

        private void tabsView_SelectedTabChildChanged(object sender, ValueChangedEventArgs<FrameworkElement> e)
        {
            if (!(DataContext is BreakEvenWindowViewModel ctx)) return;
            ctx.DocumentGroup.Clear();
            ctx.DocumentCurrencyGroup.Clear();
            gridDocument.RefreshData();
            gridCurrencyDocument.RefreshData();
        }

        
    }

    public class DecimalToColorConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var summaryList = (List<GridTotalSummaryData>) value;
            if (summaryList == null || summaryList.Count == 0)
                return null;
            var sumValue = System.Convert.ToInt32(summaryList[0].Value);
            if (sumValue < 0)
                return new SolidColorBrush(Colors.Red);
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
