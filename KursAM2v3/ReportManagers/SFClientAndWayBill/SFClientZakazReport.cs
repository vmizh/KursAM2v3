using System;
using Core;
using DevExpress.Spreadsheet;
using Helper;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Finance.Invoices;
using Reports.Base;

namespace KursAM2.ReportManagers.SFClientAndWayBill
{
    public class SFClientZakazReport : BaseReport
    {
        public SFClientZakazReport(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public bool IsManagerPrint { set; get; } = true;

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            sheet.Cells["G1"].Value = GlobalOptions.SystemProfile.OwnerKontragent.Name;
            sheet.Cells["G2"].Value = GlobalOptions.SystemProfile.OwnerKontragent.ADDRESS;
            sheet.Cells["C5"].Value = string.IsNullOrWhiteSpace(document.OuterNumber)
                ? document.InnerNumber.ToString()
                : document.OuterNumber;
            sheet.Cells["C7"].Value = document.Client?.Name;
            sheet.Cells["C9"].Value =
                Convert.ToDouble(document.Summa);
            sheet.Cells["C9"].NumberFormat = "#,##0.00";
            sheet.Cells["H6"].Value = $"{document.DocDate.ToShortDateString()}";
            sheet.Cells["H7"].Value = $"{DateTime.Today:d}";
            sheet.Cells["I7"].Value = $"{DateTime.Now.ToShortTimeString()}";
            if (IsManagerPrint)
                sheet.Cells["H8"].Value = GlobalOptions.UserInfo.FullName;
            sheet.Cells["D9"].Value = document.Currency?.CRS_SHORTNAME;
            var startTableRow = 13;
            var row = 1;
            for (var i = 1; i <= document.Rows.Count; i++)
            {
                sheet.InsertCells(sheet.Range[string.Format("A{0}:I{0}", startTableRow + i)],
                    InsertCellsMode.ShiftCellsDown);
                sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows[startTableRow.ToString()]);
            }
            foreach (var item in document.Rows)
            {
                sheet.Cells[$"A{startTableRow + row}"].Value = row;
                sheet.Cells[$"B{startTableRow + row}"].Value = item.Nomenkl.NomenklNumber;
                sheet.Cells[$"C{startTableRow + row}"].Value = item.Nomenkl.Name;
                sheet.Cells[$"E{startTableRow + row}"].Value = item.Nomenkl.Unit?.ED_IZM_NAME;
                sheet.Cells[$"F{startTableRow + row}"].Value = item.SFT_KOL;
                sheet.Cells[$"F{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"G{startTableRow + row}"].Value = Convert.ToDouble(item.SFT_ED_CENA);
                sheet.Cells[$"G{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"H{startTableRow + row}"].Value = Convert.ToDouble(item.SFT_SUMMA_K_OPLATE);
                sheet.Cells[$"H{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"I{startTableRow + row}"].Value = item.SFT_TEXT;
                row++;
            }
            sheet.Cells[$"H{document.Rows.Count + startTableRow + 1}"].Formula =
                $"SUM(H{startTableRow}:H{document.Rows.Count + startTableRow})";
            sheet.Cells[$"H{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
            if (document.Currency != null)
                sheet.Cells[$"H{document.Rows.Count + startTableRow + 3}"].Value =
                    RuDateAndMoneyConverter.CurrencyToTxt(Convert.ToDouble(document.Summa),
                        document.Currency.Name, true);
        }
    }
}