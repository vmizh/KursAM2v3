using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;

namespace KursAM2.DragAndDrop
{
    public class CustomDataControlDragElement : DataControlDragElement
    {
        public CustomDataControlDragElement(DragDropManagerBase dragDropManager, Point offset, FrameworkElement owner)
            : base(dragDropManager, offset, owner)
        {
            var content = container.Content as ContentPresenter;
            var oldInfo = content.Content as DragDropViewInfo;
            var info = new ExternalDragDropViewInfo
            {
                DraggingRows = (dragDropManager as IDragRowVisibilitySupport).DraggingRowsData,
                DropTargetRow = oldInfo.DropTargetRow,
                DropTargetType = oldInfo.DropTargetType,
                FirstDraggingObject = oldInfo.FirstDraggingObject,
                GroupInfo = oldInfo.GroupInfo
            };
            content.Content = info;
        }
    }
}
