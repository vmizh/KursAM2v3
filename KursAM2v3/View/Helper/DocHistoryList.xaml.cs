using DevExpress.Xpf.Core;
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
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void GridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
        }
    }
}
