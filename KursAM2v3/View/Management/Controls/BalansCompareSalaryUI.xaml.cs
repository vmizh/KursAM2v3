using System.Linq;
using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management.ManagementBalans;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansCompareSalaryUI.xaml
    /// </summary>
    public partial class BalansCompareSalaryUI : ILayout
    {
        public BalansCompareSalaryUI()
        {
            InitializeComponent();
            Loaded += BalansCompareSalaryUI_Loaded;
            Unloaded += BalansCompareSalaryUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                "KursAM2.View.Management.Controls.BalansCompareSalaryUI",
                gridSalary);
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void BalansCompareSalaryUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareSalaryUI_Loaded(object sender, RoutedEventArgs e)
        {
            //LayoutManager.Load();
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            gridSalary.TotalSummary.Clear();
            var k = 0;
            foreach (var band in gridSalary.Bands)
            {
                band.Name = "band" + k++;
                foreach (var c in band.Columns)
                {
                    if (c.FieldType == typeof(decimal) || c.FieldType == typeof(decimal?))
                    {
                        gridSalary.TotalSummary.Add(new GridSummaryItem
                        {
                            FieldName = c.FieldName,
                            DisplayFormat = "n2",
                            SummaryType = SummaryItemType.Sum
                        });
                    }
                }
            }
            var ctx = DataContext as ManagementBalansCompareWindowViewModel;
            if (ctx == null) return;
            foreach (var b in gridSalary.Bands)
            {
                switch (b.Header)
                {
                    case "RUB":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaRUB != 0 || _.SummaRUB2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                    case "EUR":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaEUR != 0 || _.SummaEUR2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                    case "USD":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaUSD != 0 || _.SummaUSD2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                    case "SEK":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaSEK != 0 || _.SummaSEK2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                    case "CHF":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaCHF != 0 || _.SummaCHF2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                    case "GBP":
                        b.Visible = ctx.EmployeeSalaryList.Any(_ => _.SummaGBP != 0 || _.SummaGBP2 != 0);
                        foreach (var c in b.Columns)
                        {
                            c.Visible = b.Visible;
                        }
                        break;
                }
            }
        }

        private void GridSalary_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}