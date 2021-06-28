using System;
using System.Linq;
using Core;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Common;
using DevExpress.Spreadsheet;
using Helper;
using KursAM2.ViewModel.Logistiks.Warehouse;
using Reports.Base;

namespace KursAM2.ReportManagers.SFClientAndWayBill
{
    public class WaybillExport : BaseReport
    {
        public WaybillExport(WaybillWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            if (!(ViewModel is WaybillWindowViewModel vm)) return;
            // ReSharper disable once UnusedVariable
            var document = vm.Document;
        }
    }

    public class WaybillTorg12 : BaseReport
    {
        public WaybillTorg12(WaybillWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        private void WaibillSetRow(Worksheet sheet, int row, int rowId, WaybillRow item)
        {
            sheet.Cells[$"A{rowId}"].Value = row;
            sheet.Cells[$"F{rowId}"].Value = string.IsNullOrEmpty(item.Nomenkl.NameFull)
                ? item.Nomenkl.Name
                : item.Nomenkl.NameFull;
            sheet.Cells[$"V{rowId}"].Value = item.Nomenkl.Unit.Name;
            sheet.Cells[$"X{rowId}"].Value = item.Nomenkl.Unit.ED_IZM_OKEI_CODE;
            sheet.Cells[$"AD{rowId}"].Value = Convert.ToDouble(item.DDT_KOL_RASHOD);
            sheet.Cells[$"AH{rowId}"].Value = Convert.ToDouble(item.DDT_KOL_RASHOD);
            sheet.Cells[$"AQ{rowId}"].Value = Convert.ToDouble(item.DDT_KOL_RASHOD);
            sheet.Cells[$"AV{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.SchetLinkedRow.SFT_SUMMA_K_OPLATE / (decimal) item.SchetLinkedRow.SFT_KOL),
                    2);
            sheet.Cells[$"BB{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble((item.SchetLinkedRow.SFT_SUMMA_K_OPLATE - item.SchetLinkedRow.SFT_SUMMA_NDS) /
                                     (decimal) item.SchetLinkedRow.SFT_KOL * item.DDT_KOL_RASHOD), 2);
            sheet.Cells[$"BG{rowId}"].Value = item.SchetLinkedRow.NDSPercent;
            sheet.Cells[$"BQ{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.SchetLinkedRow.SFT_SUMMA_NDS *
                                     (item.DDT_KOL_RASHOD / (decimal) item.SchetLinkedRow.SFT_KOL)), 2);
            sheet.Cells[$"BV{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.SchetLinkedRow.SFT_SUMMA_K_OPLATE *
                                     (item.DDT_KOL_RASHOD / (decimal) item.SchetLinkedRow.SFT_KOL)), 2);
        }

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as WaybillWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            Kontragent receiver;
            receiver = vm.Document.InvoiceClient.Receiver ?? GlobalOptions.SystemProfile.OwnerKontragent;
            sheet.Cells["A4"].Value = receiver.GruzoRequisiteForWaybill;
            sheet.Cells["K12"].Value = vm.Document.KontragentReceiver.GruzoRequisiteForWaybill;
            sheet.Cells["K19"].Value = receiver.GruzoRequisiteForWaybill;
            sheet.Cells["K24"].Value = vm.Document.KontragentReceiver.GruzoRequisiteForWaybill;
            sheet["AE33"].Value = vm.Document.DD_EXT_NUM ?? vm.Document.DD_IN_NUM.ToString();
            sheet["AR33"].Value = vm.Document.DD_DATE.ToShortDateString();
            sheet["BX5"].Value = receiver.OKPO;
            sheet["BX14"].Value = vm.Document.KontragentReceiver.OKPO;
            sheet["BX17"].Value = receiver.OKPO;
            sheet["BX22"].Value = vm.Document.KontragentReceiver.OKPO;
            //sheet["P79"].Value = vm.Document.DD_DATE.ToLongDateString();
            //sheet["AZ79"].Value = vm.Document.DD_DATE.ToLongDateString();
            // ReSharper disable once UnusedVariable
            var itog = sheet.Range["A41:CA41"];
            var startTableRow = 40;
            var row = 1;
            for (var i = 1; i < document.Rows.Count; i++)
            {
                sheet.InsertCells(sheet.Range[string.Format("A{0}:CA{0}", startTableRow + i)],
                    InsertCellsMode.EntireRow);
                sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows[startTableRow.ToString()]);
            }
            if (document.Rows.Count > 3 && document.Rows.Count <= 20)
                sheet.HorizontalPageBreaks.Add(startTableRow + document.Rows.Count - 2);
            foreach (var item in document.Rows)
            {
                WaibillSetRow(sheet, row, startTableRow + row - 1, item);
                row++;
            }
            sheet.Cells[$"AH{document.Rows.Count + startTableRow}"].Formula =
                $"SUM(AH{startTableRow}:AH{document.Rows.Count + startTableRow - 1})";
            sheet.Cells[$"BB{document.Rows.Count + startTableRow}"].Formula =
                $"SUM(BB{startTableRow}:BB{document.Rows.Count + startTableRow - 1})";
            sheet.Cells[$"BQ{document.Rows.Count + startTableRow}"].Formula =
                $"SUM(BQ{startTableRow}:BQ{document.Rows.Count + startTableRow - 1})";
            sheet.Cells[$"BV{document.Rows.Count + startTableRow}"].Formula =
                $"SUM(BV{startTableRow}:BV{document.Rows.Count + startTableRow - 1})";
            sheet.Cells[$"AH{document.Rows.Count + startTableRow + 1}"].Value = document.Rows.Count;
            sheet.Cells[$"BB{document.Rows.Count + startTableRow + 1}"].Value =
                Math.Round(Convert.ToDouble(
                    document.Rows.Sum(_ => (_.SchetLinkedRow.SFT_SUMMA_K_OPLATE - _.SchetLinkedRow.SFT_SUMMA_NDS) *
                                           (_.DDT_KOL_RASHOD / (decimal) _.SchetLinkedRow.SFT_KOL))), 2);
            sheet.Cells[$"BQ{document.Rows.Count + startTableRow + 1}"].Value =
                Math.Round(Convert.ToDouble(
                    document.Rows.Sum(_ => _.SchetLinkedRow.SFT_SUMMA_NDS *
                                           (_.DDT_KOL_RASHOD / (decimal) _.SchetLinkedRow.SFT_KOL))), 2);
            sheet.Cells[$"BV{document.Rows.Count + startTableRow + 1}"].Value =
                Math.Round(Convert.ToDouble(
                    document.Rows.Sum(_ => _.SchetLinkedRow.SFT_SUMMA_K_OPLATE *
                                           (_.DDT_KOL_RASHOD / (decimal) _.SchetLinkedRow.SFT_KOL))), 2);
            sheet.Cells[$"AB{document.Rows.Count + startTableRow + 4}"].Value = vm.Document.Rows.Count;
            sheet.Cells[$"O{document.Rows.Count + startTableRow + 30}"].Value = receiver.GlavBuh;
            sheet.Cells[$"J{document.Rows.Count + startTableRow + 9}"].Value =
                RuDateAndMoneyConverter.NumeralsDoubleToTxt(Convert.ToDouble(document.Rows.Count), 0,
                    TextCase.Accusative, true);
            sheet.Cells[$"M{document.Rows.Count + startTableRow + 9}"].Value =
                RuDateAndMoneyConverter.CurrencyToTxt(
                    sheet.Cells[$"BV{document.Rows.Count + startTableRow + 1}"].Value.NumericValue,
                    document.InvoiceClient.Currency?.Name,
                    true);
        }
    }
}