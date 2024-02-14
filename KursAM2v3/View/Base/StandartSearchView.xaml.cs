using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using Helper;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for StandartSearchView.xaml
    /// </summary>
    public partial class StandartSearchView
    {
        static StandartSearchView()
        {
            GridControlLocalizer.Active = new CustomDXGridLocalizer();
        }

        public StandartSearchView()
        {
            InitializeComponent();
        }


        private void GridDocuments_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.SortMode = ColumnSortMode.DisplayText;
            e.Column.ColumnFilterMode = ColumnFilterMode.DisplayText;
        }
    }
}
