using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.StockHolder
{
    /// <summary>
    ///     Interaction logic for StockHoldersBalancesView.xaml
    /// </summary>
    public partial class StockHoldersBalancesView : ThemedWindow
    {
        public StockHoldersBalancesView()
        {
            InitializeComponent();
        }

        private void GridControlStockHolder_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}