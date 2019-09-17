using System;
using System.IO;
using System.Windows;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Spreadsheet;

namespace Reports.Base
{
    public abstract class BaseReport
    {
        private IWorkbook Workbook;

        public KursReportPrintOptions PrintOptions { get; set; }
        public string XlsFileName { set; get; }
        public RSViewModelBase ViewModel { set; get; }

        public ReportShowType ShowType { set; get; }
        public abstract void GenerateReport(Worksheet sheet);

        public virtual void ShowSpreadsheet()
        {
            var file = $"{Environment.CurrentDirectory}\\Reports\\{XlsFileName}.xlsx";
            var view = new ExportView {Owner = Application.Current.MainWindow};
            Workbook = view.Sreadsheet.Document;
            if (XlsFileName != null)
                if (File.Exists(file))
                {
                    Workbook.LoadDocument(file, DocumentFormat.Xlsx);
                }
                else
                {
                    WindowManager.ShowMessage(null,$"Файл - {file} не найден","Сообщение",MessageBoxImage.Information);
                    return;
                }

            Workbook.BeginUpdate();
            try
            {
                var worksheet = Workbook.Worksheets.ActiveWorksheet;
                GenerateReport(worksheet);
                view.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            finally
            {
                Workbook.EndUpdate();
            }
        }

        public virtual void Export()
        {
            var view = new ExportView {Owner = Application.Current.MainWindow};
            Workbook = view.Sreadsheet.Document;
            Workbook.BeginUpdate();
            try
            {
                var worksheet = Workbook.Worksheets.ActiveWorksheet;
                GenerateReport(worksheet);
                view.Show();
            }
            finally
            {
                Workbook.EndUpdate();
            }
        }

        public virtual void ShowReport()
        {
            Workbook = new SpreadsheetControl().Document;
            var file = $"{Environment.CurrentDirectory}\\Reports\\{XlsFileName}.xlsx";
            var view = new ExportView {Owner = Application.Current.MainWindow};
            Workbook = view.Sreadsheet.Document;
            Workbook.BeginUpdate();
            if (XlsFileName != null)
                if (File.Exists(file))
                {
                    Workbook.LoadDocument(file, DocumentFormat.OpenXml);
                }
                else
                {
                    WindowManager.ShowMessage(null, $"Файл - {file} не найден", "Сообщение", MessageBoxImage.Information);
                    return;
                }
            try
            {
                var worksheet = Workbook.Worksheets[0];
                GenerateReport(worksheet);
                Workbook.EndUpdate();
                worksheet.ActiveView.Orientation = PrintOptions.PageOrientation;
                worksheet.ActiveView.ShowHeadings = false;
                worksheet.ActiveView.PaperKind = PrintOptions.PaperKind;

                var printOptions = worksheet.PrintOptions;
                printOptions.BlackAndWhite = PrintOptions.BlackAndWhite;
                printOptions.PrintGridlines = PrintOptions.PrintGridlines;
                printOptions.FitToPage = PrintOptions.FitToPage;
                printOptions.FitToWidth = PrintOptions.FitToWidth;
                printOptions.ErrorsPrintMode = ErrorsPrintMode.Dash;

                #region #PrintWorkbook

                var link = new LegacyPrintableComponentLink(Workbook);

                link.CreateDocument();
                link.ShowPrintPreview(Application.Current.MainWindow);

                #endregion #PrintWorkbook
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null,ex);
            }
        }

        public virtual void Show()
        {
            switch (ShowType)
            {
                case ReportShowType.Export:
                    Export();
                    break;
                case ReportShowType.Report:
                    ShowReport();
                    break;
                case ReportShowType.Spreadsheet:
                    ShowSpreadsheet();
                    break;
            }
        }
    }

    public enum ReportShowType
    {
        Report = 0,
        Export = 1,
        Spreadsheet = 2
    }
}