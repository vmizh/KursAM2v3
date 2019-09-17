using System.Linq;
using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.ManagementBalans;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansCompareStoreOperationsUI.xaml
    /// </summary>
    public partial class BalansCompareStoreOperationsUI : ILayout
    {
        private readonly MemoEditSettings myMemoEdit = new MemoEditSettings
        {
            ShowIcon = false
        };

        public BalansCompareStoreOperationsUI()
        {
            InitializeComponent();
            //Loaded += BalansComareKontragentOperationsUI_Loaded;
            //Unloaded += BalansComareKontragentOperationsUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                "KursAM2.View.Management.Controls.BalansCompareStoreOperationsUI",
                gridControlStoreOperations);
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void BalansComareKontragentOperationsUI_Unloaded(object sender, RoutedEventArgs e)
        {
            //LayoutManager.Save();
        }

        private void BalansComareKontragentOperationsUI_Loaded(object sender, RoutedEventArgs e)
        {
            // LayoutManager.Load(true);
        }

        private void OnInlineColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridControlStoreOperations.Bands)
                column.Name = "band" + k++;
            foreach (var column in gridControlStoreOperations.Columns)
                switch (column.FieldName)
                {
                    case "Currency":
                        column.SortMode = ColumnSortMode.DisplayText;
                        break;
                    case "Note":
                        column.EditSettings = myMemoEdit;
                        break;
                }
            LayoutManager.Load(true);
            GridControlBand b;
            var ctx = DataContext as NomenklCompareBalansDeltaItem;
            foreach (var column in gridControlStoreOperations.Columns)
                switch (column.FieldName)
                {
                    case "LossEUR":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaEUR) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaEUR2) != 0;
                        break;
                    case "LossUSD":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaUSD) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaUSD2) != 0;
                        break;
                    case "LossRUB":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaRUB) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaRUB2) != 0;
                        break;
                    case "LossGBP":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaGBP) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaGBP2) != 0;
                        break;
                    case "LossCHF":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaCHF) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaCHF2) != 0;
                        break;
                    case "LossSEK":
                        b =
                            gridControlStoreOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaSEK) != 0 ||
                                        ctx.NomenklOperations.Sum(_ => _.SummaSEK2) != 0;
                        break;
                }
        }

        private void GridKontragentInLine_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void GridLayoutHelper_Trigger(object sender, MyEventArgs e)
        {
            LayoutManager?.Save();
        }
    }
}