// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
using System.Collections.Generic;

namespace KursAM2.DragAndDrop
{
    public interface IDragRowVisibilitySupport
    {
        List<object> DraggingRowsData { get; set; }
    }
}
