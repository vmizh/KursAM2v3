using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.DragAndDrop
{
    public class CustomGridDragDropManager : GridDragDropManager, IDragRowVisibilitySupport
    {
        public TableView TableView => View as TableView;
        public List<object> DraggingRowsData { get; set; }

        protected override IDragElement CreateDragElement(Point offset, FrameworkElement owner)
        {
            return new CustomDataControlDragElement(this, offset, owner);
        }

        public void SetDraggingRowsData()
        {
            DraggingRowsData = GetRowImagesHelper.GetRowImages(TableView);
        }

        protected override IList CalcDraggingRows(IndependentMouseEventArgs e)
        {
            SetDraggingRowsData();
            return base.CalcDraggingRows(e);
        }
    }

    public static class GetRowImagesHelper
    {
        public static List<object> GetRowImages(DataViewBase view)
        {
            var images = new List<object>();
            foreach (var rowHandle in view.DataControl.GetSelectedRowHandles())
            {
                var element = view.GetRowElementByRowHandle(rowHandle);
                if (element != null)
                {
                    element = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;
                    var bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96,
                        PixelFormats.Pbgra32);
                    bmp.Render(element);
                    images.Add(new Image
                        { ImageSource = bmp, Width = element.ActualWidth, Height = element.ActualHeight });
                }
            }

            return images;
        }
    }

    public class Image
    {
        public ImageSource ImageSource { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
