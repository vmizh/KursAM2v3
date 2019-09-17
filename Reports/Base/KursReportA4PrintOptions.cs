using System.Drawing.Printing;
using DevExpress.Spreadsheet;

namespace Reports.Base
{
    public class KursReportA4PrintOptions : KursReportPrintOptions
    {
        public KursReportA4PrintOptions()
        {
            PageOrientation = PageOrientation.Portrait;
            PaperKind = PaperKind.A4;
            BlackAndWhite = true;
            PrintGridlines = false;
            FitToPage = true;
            FitToWidth = 1;
            ErrorsPrintMode = ErrorsPrintMode.Dash;
        }
    }
}