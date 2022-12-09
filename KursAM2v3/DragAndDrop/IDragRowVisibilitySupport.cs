using System.Collections.Generic;

namespace KursAM2.DragAndDrop
{
    public interface IDragRowVisibilitySupport
    {
        List<object> DraggingRowsData { get; set; }
    }
}
