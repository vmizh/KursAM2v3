using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for StandartSearchView.xaml
    /// </summary>
    public partial class StandartSearchView
    {
        public StandartSearchView()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            ThemeManager.SetThemeName(this, ThemeManager.ActualApplicationThemeName);
        }

        private void GridDocuments_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.SortMode = ColumnSortMode.DisplayText;
            e.Column.ColumnFilterMode = ColumnFilterMode.DisplayText;
        }
    }
}
