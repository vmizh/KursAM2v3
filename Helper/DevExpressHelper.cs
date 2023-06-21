using System.Collections.Generic;
using DevExpress.Xpf.Grid;

namespace Helper
{
    public static class DevExpressHelper
    {
        public static List<GridSummaryItem> GetForName(
            this GridSummaryItemCollection summaries, string name)
        {
            List<GridSummaryItem> ret = new List<GridSummaryItem>();
            foreach (var col in summaries)
                if (col.FieldName == name) 
                    ret.Add(col);
            return ret;
        }


    }
}
