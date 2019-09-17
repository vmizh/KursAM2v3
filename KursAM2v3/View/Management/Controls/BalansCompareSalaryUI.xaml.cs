using System.Windows;
using DevExpress.Xpf.Grid;
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
            LayoutManager.Load();
        }

        private void OnColumnsGenerated(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in gridSalary.Bands)
                column.Name = "band" + k++;
        }

        private void GridSalary_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}