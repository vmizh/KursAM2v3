using DevExpress.Xpf.Grid;

namespace KursAM2.View.Helper
{
    /// <summary>
    ///     Interaction logic for DocHistoryList.xaml
    /// </summary>
    public partial class DocHistoryList
    {
        public DocHistoryList()
        {
            InitializeComponent();
        }

        private void GridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
        }
    }
}