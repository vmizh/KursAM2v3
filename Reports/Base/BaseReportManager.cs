using System.Collections.Generic;

namespace Reports.Base
{
    public class ReportManager
    {
        public static Dictionary<string,BaseReport> Reports { set; get; }

        public ReportManager()
        {
            Reports = new Dictionary<string,BaseReport>();
        }

        public ReportManager(Dictionary<string,BaseReport> reports)
        {
            Reports = new Dictionary<string,BaseReport>();
        }
    }
}