using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace KursDomain.View.Base;

/// <summary>
///     Interaction logic for BaseSearchWindow.xaml
/// </summary>
public partial class BaseSearchWindow
{
    public BaseSearchWindow()
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
