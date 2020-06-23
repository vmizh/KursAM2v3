using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for BalansCompareSalaryUI.xaml
    /// </summary>
    public partial class BalansCompareMoneyInPathUI : ILayout
    {
        public BalansCompareMoneyInPathUI()
        {
            InitializeComponent();
            Loaded += BalansCompareMoneyInPathUI_Loaded;
            Unloaded += BalansCompareMoneyInPathUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                "KursAM2.View.Management.Controls.BalansCompareMoneyInPathUI",
                gridMoneyInPath);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
 
        private void BalansCompareMoneyInPathUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareMoneyInPathUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridMoneyInPath.Bands)
            {
                column.Name = "band" + k++;
            }
            gridMoneyInPath.TotalSummary.Clear();
            foreach (var col in gridMoneyInPath.Columns)
            {
                if (KursGridControlHelper.ColumnFieldTypeCheckDecimal(col.FieldType))
                {
                    var summary = new GridSummaryItem
                    {
                        SummaryType = SummaryItemType.Sum,
                        ShowInColumn = col.FieldName,
                        DisplayFormat = "{0:n2}",
                        FieldName = col.FieldName
                    };
                    gridMoneyInPath.TotalSummary.Add(summary);
                    col.EditSettings = new CalcEditSettings
                    {
                        DisplayFormat = "n2",
                        Name = col.FieldName + "Calc"
                    };
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