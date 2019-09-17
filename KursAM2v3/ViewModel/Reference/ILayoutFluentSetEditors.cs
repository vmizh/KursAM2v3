using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;

namespace KursAM2.ViewModel.Reference
{
    public interface ILayoutFluentSetEditors
    {
        void SetGridColumn(GridColumn col);
        void SetTreeListColumn(TreeListColumn col);
        void SetDataLayout(DataLayoutItem item, string propertyName);
    }
}