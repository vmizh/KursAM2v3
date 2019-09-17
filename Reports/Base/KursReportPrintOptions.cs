using System.Drawing.Printing;
using DevExpress.Spreadsheet;

namespace Reports.Base
{
    public abstract class KursReportPrintOptions
    {
        public PageOrientation PageOrientation { set; get; }
        public PaperKind PaperKind { set; get; }
        public bool BlackAndWhite { set; get; }
        public bool PrintGridlines { set; get; }
        public bool FitToPage { set; get; }
        public int FitToWidth { set; get; }
        public ErrorsPrintMode ErrorsPrintMode { set; get; }
    }
}