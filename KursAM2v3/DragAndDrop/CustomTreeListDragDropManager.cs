using System.Collections;
using System.Collections.Generic;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.DragAndDrop
{
    public class CustomTreeListDragDropManager : TreeListDragDropManager, IDragRowVisibilitySupport
    {
        public List<object> DraggingRowsData { get; set; }

        protected override IDragElement CreateDragElement(Point offset, FrameworkElement owner)
        {
            return new CustomDataControlDragElement(this, offset, owner);
        }

        public void SetDraggingRowsData()
        {
            DraggingRowsData = GetRowImagesHelper.GetRowImages(TreeListView);
        }

        protected override IList CalcDraggingRows(IndependentMouseEventArgs e)
        {
            SetDraggingRowsData();
            return base.CalcDraggingRows(e);
        }
    }
}
