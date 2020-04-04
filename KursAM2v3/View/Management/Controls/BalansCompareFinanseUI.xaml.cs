using System.Linq;
using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using KursAM2.ViewModel.Management.ManagementBalans;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansCompareFinanseUI.xaml
    /// </summary>
    public partial class BalansCompareFinanseUI : ILayout, IResetCurrencyColumns
    {
        private readonly MemoEditSettings memoEdit = new MemoEditSettings
        {
            ShowIcon = false
        };

        public BalansCompareFinanseUI()
        {
            InitializeComponent();
            Loaded += BalansCompareFinanseUI_Loaded;
            Unloaded += BalansCompareFinanseUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager("KursAM2.View.Management.Controls.BalansCompareFinanseUI",
                gridFinanse);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void ResetCurrencyColumns()
        {
            var ctx = DataContext as ManagementBalansCompareWindowViewModel;
            foreach (var column in gridFinanse.Columns)
            {
                if (ctx == null) return;
                GridControlBand b;
                switch (column.FieldName)
                {
                    case "SummaEUR":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaEUR) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaEUR2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                    case "SummaUSD":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaUSD) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaUSD2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                    case "SummaRUB":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaRUB) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaRUB2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                    case "SummaGBP":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaGBP) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaGBP2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                    case "SummaCHF":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaCHF) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaCHF2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                    case "SummaSEK":
                        b =
                            gridFinanse.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                        if (b != null)
                            b.Visible = ctx.FinanseOperations.Sum(_ => _.SummaSEK) != 0 ||
                                        ctx.FinanseOperations.Sum(_ => _.SummaSEK2) != 0;
                        SetVisibleColumnsForBand(b);
                        break;
                }
            }
        }

        private void SetVisibleColumnsForBand(GridControlBand band)
        {
            foreach (var c in band.Columns)
            {
                c.Visible = band.Visible;
            }
        }

        private void BalansCompareFinanseUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareFinanseUI_Loaded(object sender, RoutedEventArgs e)
        {

            LayoutManager.Load(true);
            ResetCurrencyColumns();
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridFinanse.Bands)
                column.Name = "band" + k++;
            foreach (var column in gridFinanse.Columns)
                switch (column.FieldName)
                {
                    case "Currency":
                        column.SortMode = ColumnSortMode.DisplayText;
                        break;
                    case "Note":
                        column.EditSettings = memoEdit;
                        break;
                }
        }

        private void GridFinanse_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}