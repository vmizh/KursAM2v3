using DevExpress.Xpf.Spreadsheet;

namespace Reports.Base
{
    /// <summary>
    ///     Interaction logic for ExportView.xaml
    /// </summary>
    public partial class ExportView
    {
        public ExportView()
        {
            InitializeComponent();
        }

        public SpreadsheetControl Sreadsheet => spreadsheetControl1;
    }
}