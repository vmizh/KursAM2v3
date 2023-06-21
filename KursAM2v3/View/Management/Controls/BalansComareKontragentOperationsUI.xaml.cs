using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.ManagementBalans;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansComareKontragentOperationsUI.xaml
    /// </summary>
    public partial class BalansComareKontragentOperationsUI : ILayout
    {
        private readonly MemoEditSettings myMemoEdit = new MemoEditSettings
        {
            ShowIcon = false
        };

        public BalansComareKontragentOperationsUI()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            Loaded += BalansComareKontragentOperationsUI_Loaded;
            Unloaded += BalansComareKontragentOperationsUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                GlobalOptions.KursSystem(),"KursAM2.View.Management.Controls.BalansComareKontragentOperationsUI",
                gridControlKontrOperations);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void BalansComareKontragentOperationsUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansComareKontragentOperationsUI_Loaded(object sender, RoutedEventArgs e)
        {
            //LayoutManager.Load(true);
        }

        private void OnInlineColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridControlKontrOperations.Bands)
                column.Name = "band" + k++;
            foreach (var column in gridControlKontrOperations.Columns)
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
            var ctx = DataContext as KontragentCompareBalansDeltaItem;
            foreach (var column in gridControlKontrOperations.Columns)
                switch (column.FieldName)
                {
                    case "LossEUR":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossEUR) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitEUR) != 0;
                        break;
                    case "LossUSD":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossUSD) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitUSD) != 0;
                        break;
                    case "LossRUB":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossRUB) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitRUB) != 0;
                        break;
                    case "LossGBP":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossGBP) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitGBP) != 0;
                        break;
                    case "LossCHF":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossCHF) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitCHF) != 0;
                        break;
                    case "LossSEK":
                        b =
                            gridControlKontrOperations.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                        if (b != null)
                            b.Visible = ctx == null ||
                                        ctx.KontragentOperations.Sum(_ => _.LossSEK) != 0 ||
                                        ctx.KontragentOperations.Sum(_ => _.ProfitSEK) != 0;
                        break;
                }
        }

        private void GridKontragentInLine_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void GridLayoutHelper_Trigger(object sender, MyEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}
