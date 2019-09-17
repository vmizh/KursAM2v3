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
    ///     Interaction logic for BalansCompareStoreUI.xaml
    /// </summary>
    public partial class BalansCompareStoreUI : ILayout, IResetCurrencyColumns
    {
        private readonly MemoEditSettings memoEdit = new MemoEditSettings
        {
            ShowIcon = false
        };

        public BalansCompareStoreUI()
        {
            InitializeComponent();
            Loaded += BalansCompareStoreUI_Loaded;
            Unloaded += BalansCompareStoreUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager("KursAM2.View.Management.Controls.BalansCompareStoreUI",
                gridStore);
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void ResetCurrencyColumns()
        {
            var ctx = DataContext as ManagementBalansCompareWindowViewModel;
            foreach (var column in gridStore.Columns)
            {
                GridControlBand b;
                switch (column.FieldName)
                {
                    case "LossEUR":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossEUR"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaEUR) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaEUR2) != 0;
                        break;
                    case "LossUSD":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossUSD"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaUSD) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaUSD2) != 0;
                        break;
                    case "LossRUB":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossRUB"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaRUB) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaRUB2) != 0;
                        break;
                    case "LossGBP":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossGBP"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaGBP) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaGBP2) != 0;
                        break;
                    case "LossCHF":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossCHF"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaCHF) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaCHF2) != 0;
                        break;
                    case "LossSEK":
                        b =
                            gridStore.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossSEK"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.NomenklDeltaList.Sum(_ => _.SummaSEK) != 0 ||
                                        ctx.NomenklDeltaList.Sum(_ => _.SummaSEK2) != 0;
                        break;
                }
            }
        }

        private void BalansCompareStoreUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareStoreUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load(true);
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridStore.Bands)
                column.Name = "band" + k++;
            foreach (var column in gridStore.Columns)
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

        private void GridStore_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}