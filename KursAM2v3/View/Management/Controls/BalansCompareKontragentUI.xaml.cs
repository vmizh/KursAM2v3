using System.Linq;
using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management.ManagementBalans;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansCompareKontragentUI.xaml
    /// </summary>
    public partial class BalansCompareKontragentUI : ILayout, IResetCurrencyColumns
    {
        private readonly MemoEditSettings memoEdit = new MemoEditSettings
        {
            ShowIcon = false
        };

        public BalansCompareKontragentUI()
        {
            InitializeComponent();
            Loaded += BalansCompareKontragentUI_Loaded;
            Unloaded += BalansCompareKontragentUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                "KursAM2.View.Management.Controls.BalansCompareKontragentUI",
                gridKontragent);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void ResetCurrencyColumns()
        {
            var ctx = DataContext as ManagementBalansCompareWindowViewModel;
            foreach (var column in gridKontragent.Columns)
            {
                GridControlBand b;
                switch (column.FieldName)
                {
                    case "SummaEUR":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaEUR) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaEUR2) != 0;
                        break;
                    case "SummaUSD":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaUSD) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaUSD2) != 0;
                        break;
                    case "SummaRUB":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaRUB) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaRUB2) != 0;
                        break;
                    case "SummaGBP":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaGBP) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaGBP2) != 0;
                        break;
                    case "SummaCHF":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaCHF) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaCHF2) != 0;
                        break;
                    case "SummaSEK":
                        b =
                            gridKontragent.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                        if (b != null)
                            b.Visible = ctx == null || ctx.KontragentDeltaList.Sum(_ => _.SummaSEK) != 0 ||
                                        ctx.KontragentDeltaList.Sum(_ => _.SummaSEK2) != 0;
                        break;
                }
            }
        }

        private void BalansCompareKontragentUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareKontragentUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load(true);
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridKontragent.Bands)
                column.Name = "band" + k++;
        }

        private void GridKontragent_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}