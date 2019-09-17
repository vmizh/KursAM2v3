using System.Drawing.Printing;
using DevExpress.Spreadsheet;

namespace Reports.Base
{
    public class KursReportLandscapeA4PrintOptions : KursReportPrintOptions
    {
        public KursReportLandscapeA4PrintOptions()
        {
            PageOrientation = PageOrientation.Landscape;
            PaperKind = PaperKind.A4;
            BlackAndWhite = true;
            PrintGridlines = false;
            FitToPage = false;
            FitToWidth = 0;
            ErrorsPrintMode = ErrorsPrintMode.Dash;
        }
    }
}