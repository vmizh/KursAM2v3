using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Core;
using DevExpress.Spreadsheet;
using Helper;
using KursAM2.ViewModel.Finance.Invoices;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using Reports.Base;

namespace KursAM2.ReportManagers.SFClientAndWayBill
{
    public class SFClientZajavkaSkladReport : BaseReport
    {
        public SFClientZajavkaSkladReport(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public bool IsManagerPrint { set; get; } = true;

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            sheet.Cells["F1"].Value = GlobalOptions.SystemProfile.OwnerKontragent.Name;
            sheet.Cells["F2"].Value = GlobalOptions.SystemProfile.OwnerKontragent.ADDRESS;
            var sNum = (string.IsNullOrWhiteSpace(document.OuterNumber)
                ? document.InnerNumber.ToString()
                : document.OuterNumber) + " от " + document.DocDate.ToShortDateString();
            sheet.Cells["A5"].Value = $"Заявка на отгрузку по счету {sNum}";
            sheet.Cells["B7"].Value = document.Client?.Name;
            //sheet.Cells["B9"].Value =
            //    Convert.ToDouble(document.SF_CRS_SUMMA_K_OPLATE);
            //sheet.Cells["B9"].NumberFormat = "#,##0.00";
            sheet.Cells["G6"].Value = $"{document.DocDate.ToShortDateString()}";
            sheet.Cells["G7"].Value = $"{DateTime.Today:d}";
            sheet.Cells["H7"].Value = $"{DateTime.Now.ToShortTimeString()}";
            if (IsManagerPrint)
                sheet.Cells["G8"].Value = GlobalOptions.UserInfo.FullName;
            sheet.Cells["D9"].Value = document.Currency?.Name;
            var notOtgruzhenoRows = new List<InvoiceClientRow>();
            foreach (var item in document.Rows)
            {
                var r = GlobalOptions.GetEntities().TD_24.Where(_ => _.DDT_SFACT_DC == item.DocCode &&
                                                                     _.DDT_SFACT_ROW_CODE == item.Code).ToList();
                if (r.Sum(_ => _.DDT_KOL_RASHOD) < item.Quantity)
                    notOtgruzhenoRows.Add(new InvoiceClientRow
                    {
                        Nomenkl = item.Nomenkl,
                        Quantity = item.Quantity - r.Sum(_ => _.DDT_KOL_RASHOD),
                        Price = item.Price,
                        //Summa = item.Price * (decimal) item.Quantity - r.Sum(_ => _.DDT_KOL_RASHOD),
                        Name = item.Note
                    });
            }

            var startTableRow = 13;
            var row = 1;
            for (var i = 1; i <= notOtgruzhenoRows.Count; i++)
            {
                sheet.InsertCells(sheet.Range[string.Format("A{0}:J{0}", startTableRow + i)],
                    InsertCellsMode.ShiftCellsDown);
                sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows[startTableRow.ToString()]);
            }

            if (notOtgruzhenoRows.Count == 0) return;
            foreach (var item in notOtgruzhenoRows)
            {
                sheet.Cells[$"A{startTableRow + row - 1}"].Value = row;
                sheet.Cells[$"B{startTableRow + row - 1}"].Value = item.Nomenkl.Name;
                sheet.Cells[$"J{startTableRow + row - 1}"].Value = item.Nomenkl.Name;
                sheet.Cells[$"D{startTableRow + row - 1}"].Value = ((IName)item.Nomenkl.Unit)?.Name;
                sheet.Cells[$"E{startTableRow + row - 1}"].Value = item.Quantity;
                sheet.Cells[$"E{startTableRow + row - 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"F{startTableRow + row - 1}"].Value = Convert.ToDouble(item.Price);
                sheet.Cells[$"F{startTableRow + row - 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"G{startTableRow + row - 1}"].Value = Convert.ToDouble(item.Summa);
                sheet.Cells[$"G{startTableRow + row - 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"H{startTableRow + row - 1}"].Value = item.Note;
                sheet.Rows[startTableRow + row - 1].AutoFit();
                sheet.Cells[$"J{startTableRow + row - 1}"].Font.Color = Color.White;
                row++;
            }

            sheet.Rows.AutoFit(13, startTableRow + notOtgruzhenoRows.Count);
            sheet.Cells[$"G{notOtgruzhenoRows.Count + startTableRow + 1}"].Formula =
                $"SUM(G{startTableRow}:G{notOtgruzhenoRows.Count + startTableRow})";
            sheet.Cells[$"G{notOtgruzhenoRows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
            if (document.Currency != null)
                sheet.Cells[$"G{notOtgruzhenoRows.Count + startTableRow + 3}"].Value =
                    RuDateAndMoneyConverter.CurrencyToTxt(
                        Convert.ToDouble(notOtgruzhenoRows.Sum(_ => _.Price * _.Quantity)),
                        document.Currency.Name, true);
        }
    }
}
