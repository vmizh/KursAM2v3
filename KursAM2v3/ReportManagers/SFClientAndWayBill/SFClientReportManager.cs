using System;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using Core;
using Data;
using DevExpress.Spreadsheet;
using Helper;
using KursAM2.ViewModel.Finance.Invoices;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using KursDomain.References;
using Reports.Base;

namespace KursAM2.ReportManagers.SFClientAndWayBill
{
    public class SFClientExport : BaseReport
    {
        public SFClientExport(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            sheet.Cells["A1"].Value = "Внутренний номер";
            sheet.Cells["B1"].Value = document.InnerNumber;
            sheet.Cells["A2"].Value = "Внешний номер";
            sheet.Cells["B2"].Value = document.OuterNumber;
            sheet.Cells["A3"].Value = "Дата счета";
            sheet.Cells["B3"].Value = document.REGISTER_DATE ?? document.DocDate;
            sheet.Cells["A4"].Value = "Контрагент";
            sheet.Cells["B4"].Value = document.Client != null ? document.Client.Name : "";
            sheet.Cells["A5"].Value = "Центр ответственности";
            sheet.Cells["B5"].Value = document.CO != null ? document.CO.Name : "";
            sheet.Cells["A6"].Value = "К оплате";
            sheet.Cells["B6"].Value = Convert.ToDouble(document.Summa);
            sheet.Cells["B6"].NumberFormat = "#`##0.00";
            sheet.Cells["A7"].Value = "Валюта";
            sheet.Cells["B7"].Value = document.Currency != null ? document.Currency.Name : "";
            sheet.Cells["A8"].Value = "Отгружено";
            sheet.Cells["B8"].Value = Convert.ToDouble(vm.Otgruzheno);
            sheet.Cells["B8"].NumberFormat = "#`##0.00";
            sheet.Cells["A9"].Value = "Условия оплаты";
            sheet.Cells["B9"].Value = vm.Document.PayCondition != null ? vm.Document.PayCondition.Name : "";
            sheet.Cells["A10"].Value = "Тип продукции";
            sheet.Cells["B10"].Value = vm.Document.VzaimoraschetType != null ? vm.Document.VzaimoraschetType.Name : "";
            sheet.Cells["A11"].Value = "Форма расчетов";
            sheet.Cells["B11"].Value = vm.Document.FormRaschet != null ? vm.Document.FormRaschet.Name : "";
            sheet.Range["B1:B11"].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
            var startTableRow = 13;
            sheet.Rows[$"{startTableRow}"].Font.Bold = true;
            sheet.Cells[$"A{startTableRow}"].Value = "Ном.№";
            sheet.Cells[$"B{startTableRow}"].Value = "Номенклатура";
            sheet.Cells[$"C{startTableRow}"].Value = "Кол-во";
            sheet.Cells[$"D{startTableRow}"].Value = "Цена";
            sheet.Cells[$"E{startTableRow}"].Value = "Сумма";
            sheet.Cells[$"F{startTableRow}"].Value = "Страна";
            sheet.Cells[$"G{startTableRow}"].Value = "Примечания";
            var row = 1;
            foreach (var item in document.Rows.Cast<InvoiceClientRowViewModel>())
            {
                sheet.Cells[$"A{startTableRow + row}"].Value = item.Nomenkl.NomenklNumber;
                sheet.Cells[$"B{startTableRow + row}"].Value = item.Nomenkl.Name;
                sheet.Cells[$"C{startTableRow + row}"].Value = item.Quantity;
                sheet.Cells[$"C{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"D{startTableRow + row}"].Value = Convert.ToDouble(item.Price);
                sheet.Cells[$"D{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"E{startTableRow + row}"].Value = Convert.ToDouble(item.Summa);
                sheet.Cells[$"E{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"F{startTableRow + row}"].Value = item.SFT_STRANA_PROIS;
                sheet.Cells[$"G{startTableRow + row}"].Value = item.Note;
                row++;
            }

            sheet.Cells[$"E{document.Rows.Count + startTableRow + 1}"].Formula =
                $"SUM(E{startTableRow}:E{document.Rows.Count + startTableRow})";
            sheet.Cells[$"E{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
            //todo проверит обязательно!!!
            //sheet.Range["A:G"].AutoFitColumns();
            var cellTable = sheet.Range[$"A{startTableRow}:G{document.Rows.Count + startTableRow}"];
            cellTable.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
        }
    }

    // ReSharper disable once InconsistentNaming
    public class SFClientSFSChet : BaseReport
    {
        public SFClientSFSChet(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            Kontragent client, receiver;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var cl = ctx.SD_43.Find(document.Client.DocCode);
                client = new Kontragent();
                client.LoadFromEntity(cl, GlobalOptions.ReferencesCache);
                var r = ctx.SD_43.Find(document.Receiver.DocCode);
                receiver = new Kontragent();
                receiver.LoadFromEntity(r, GlobalOptions.ReferencesCache);
            }

            var receiverName = string.IsNullOrEmpty(receiver.FullName)
                ? receiver.Name
                : receiver.FullName;
            var clientName = string.IsNullOrEmpty(client.FullName)
                ? client.Name
                : client.FullName;
            sheet.Cells["B11"].Value =
                $"Счет на оплату № {document.OuterNumber} от {document.DocDate.ToLongDateString()}";
            sheet.Cells["D6"].Value = receiver.INN;
            sheet.Cells["M6"].Value = receiver.KPP;
            sheet.Cells["B7"].Value = receiver.FullName;
            sheet.Cells["H13"].Value =
                $"{receiverName}, ИНН {receiver.INN}, КПП {receiver.KPP}, {receiver.Address}, тел.{receiver.Phone}";
            sheet.Cells["H15"].Value =
                $"{receiverName}, ИНН {receiver.INN}, КПП {receiver.KPP}, {receiver.Address}, тел.{receiver.Phone}";
            sheet.Cells["H17"].Value =
                $"{clientName}, ИНН {client.INN}, КПП {client.KPP}, {client.Address}, тел.{client.Phone}";
            sheet.Cells["H19"].Value =
                $"{clientName}, ИНН {client.INN}, КПП {client.KPP}, {client.Address}, тел.{client.Phone}";
            //TODO Добавить банки в справочник контрагента
            /*var k = receiver.KontragentBanks != null && receiver.KontragentBanks.Count > 0
                ? receiver.KontragentBanks[0]
                : null;
            if (k != null)
            {
                sheet.Cells["B3"].Value = k.Bank.Name;
                sheet.Cells["W3"].Value = k.Bank.POST_CODE;
                sheet.Cells["W4"].Value = k.Bank.CORRESP_ACC;
                sheet.Cells["W6"].Value = k.Entity.RASCH_ACC;
            }*/

            sheet.Cells["AC29"].Value = receiver.Director;
            sheet.Cells["AC32"].Value = receiver.GlavBuh;
            sheet.Cells["AC35"].Value = receiver.Director;
            var startTableRow = 21;
            for (var i = 2; i <= document.Rows.Count; i++)
            {
                sheet.InsertCells(sheet.Range[string.Format("A{0}:AL{0}", startTableRow + i)],
                    InsertCellsMode.ShiftCellsDown);
                sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows["22"]);
            }

            var row = 1;
            foreach (var item in document.Rows)
            {
                sheet[startTableRow + row - 1, 1].Value = row;
                sheet[startTableRow + row - 1, 3].Value =
                    !string.IsNullOrEmpty(item.Nomenkl.FullName)
                        ? item.Nomenkl.FullName
                        : item.Nomenkl.Name;
                sheet.Cells[$"AB{startTableRow + row}"].Value = ((IName) item.Nomenkl.Unit)?.Name;
                sheet.Cells[$"Y{startTableRow + row}"].Value = item.Quantity;
                sheet.Cells[$"Y{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"AD{startTableRow + row}"].Value = Convert.ToDouble(item.Price);
                sheet.Cells[$"AD{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"AH{startTableRow + row}"].Value =
                    Convert.ToDouble(item.Summa);
                sheet.Cells[$"AH{startTableRow + row}"].NumberFormat = "#,##0.00";
                row++;
            }

            sheet.Cells[$"AH{document.Rows.Count + startTableRow + 2}"].Value =
                Convert.ToDouble(document.Summa);
            sheet.Cells[$"AH{document.Rows.Count + startTableRow + 3}"].Value =
                Convert.ToDouble(document.Rows.Sum(_ => _.SFT_SUMMA_NDS));
            //sheet.Cells[$"AH{document.Rows.Count + startTableRow + 4}"].Value =
            //    Convert.ToDouble(document.SF_CRS_SUMMA_K_OPLATE);
            sheet.Cells[$"B{document.Rows.Count + startTableRow + 5}"].Value =
                $"Всего наименований {document.Rows.Count()}, на сумму {document.Summa:n2} {document.Currency.FullName}";
            sheet.Cells[$"B{document.Rows.Count + startTableRow + 6}"].Value =
                $"{RuDateAndMoneyConverter.CurrencyToTxt(Convert.ToDouble(document.Summa), document.Currency.Name, true)} , в т.ч. НДС {Math.Round((double) (document.Rows.Sum(_ => _.SFT_SUMMA_NDS) ?? 0), 2)}";
        }
    }

    public class SFClientSFSсhetNew : BaseReport
    {
        public SFClientSFSсhetNew(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var vm = ViewModel as ClientWindowViewModel;
                if (vm == null) return;
                var document = vm.Document;
                var number = string.IsNullOrWhiteSpace(document.OuterNumber)
                    ? Convert.ToString(document.InnerNumber)
                    : document.OuterNumber;
                KontragentViewModel client, receiver;
                var cl = ctx.SD_43.Find(document.Client.DocCode);
                client = new KontragentViewModel(cl);
                var r = ctx.SD_43.Find(document.Receiver.DocCode);
                receiver = new KontragentViewModel(r);
                var receiverName = string.IsNullOrEmpty(receiver.FullName)
                    ? receiver.Name
                    : receiver.FullName;
                var clientName = string.IsNullOrEmpty(client.FullName)
                    ? client.Name
                    : client.FullName;
                sheet.Cells["B11"].Value =
                    $"Счет на оплату № {number} от {document.DocDate.ToLongDateString()}";
                sheet.Cells["D6"].Value = receiver.INN;
                sheet.Cells["M6"].Value = receiver.KPP;
                sheet.Cells["B7"].Value = receiver.FullName;
                sheet.Cells["H13"].Value =
                    $"{receiverName}, ИНН {receiver.INN}, КПП {receiver.KPP}, {receiver.ADDRESS}, тел.{receiver.TEL}";
                sheet.Cells["H15"].Value =
                    $"{receiverName}, ИНН {receiver.INN}, КПП {receiver.KPP}, {receiver.ADDRESS}, тел.{receiver.TEL}";
                sheet.Cells["H17"].Value =
                    $"{clientName}, ИНН {client.INN}, КПП {client.KPP}, {client.ADDRESS}, тел.{client.TEL}";
                sheet.Cells["H19"].Value =
                    $"{clientName}, ИНН {client.INN}, КПП {client.KPP}, {client.ADDRESS}, тел.{client.TEL}";
                var bs = ctx.TD_43.Include(_ => _.SD_44).Where(_ => _.DOC_CODE == receiver.DocCode);
                TD_43 k = null;
                if (bs.Any())
                {
                    if (bs.Count() == 1)
                    {
                        k = bs.First();
                    }
                    else
                    {
                        var k1 = bs.FirstOrDefault(_ => _.USE_FOR_TLAT_TREB == 1);
                        if (k1 == null)
                            k = bs.First();
                    }

                    if (k != null)
                    {
                        sheet.Cells["B3"].Value = k.SD_44.BANK_NAME;
                        sheet.Cells["W3"].Value = k.SD_44.POST_CODE;
                        sheet.Cells["W4"].Value = k.SD_44.CORRESP_ACC;
                        sheet.Cells["W6"].Value = k.RASCH_ACC;
                    }
                }

                sheet.Cells["H13"].Value = $"{receiverName}, ИНН {receiver.INN}, " +
                                           $"КПП {receiver.KPP}, {receiver.ADDRESS}";
                var grlist = ctx.SD_43_GRUZO.Where(_ => _.doc_code == receiver.DocCode);

                if (grlist.Any())
                    switch (grlist.Count())
                    {
                        case 1:
                            sheet.Cells["H15"].Value = grlist.First().GRUZO_TEXT_SF;
                            break;
                        default:
                        {
                            var g = grlist.FirstOrDefault(_ => _.IsDefault == true);
                            sheet.Cells["H15"].Value = g != null ? g.GRUZO_TEXT_SF : grlist.First().GRUZO_TEXT_SF;
                            break;
                        }
                    }

                var pgrlist = ctx.SD_43_GRUZO.Where(_ => _.doc_code == client.DocCode);

                if (pgrlist.Any())
                    switch (pgrlist.Count())
                    {
                        case 1:
                            sheet.Cells["H19"].Value = pgrlist.First().GRUZO_TEXT_SF;
                            break;
                        default:
                        {
                            var g = pgrlist.FirstOrDefault(_ => _.IsDefault == true);
                            sheet.Cells["H19"].Value = g != null ? g.GRUZO_TEXT_SF : pgrlist.First().GRUZO_TEXT_SF;
                            break;
                        }
                    }


                var startTableRow = 21;
                for (var i = 2; i <= document.Rows.Count; i++)
                {
                    sheet.InsertCells(sheet.Range[string.Format("A{0}:AL{0}", startTableRow + i)],
                        InsertCellsMode.ShiftCellsDown);
                    sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows["22"]);
                }

                var row = 1;
                foreach (var item in document.Rows)
                {
                    sheet[startTableRow + row - 1, 1].Value = row;
                    sheet[startTableRow + row - 1, 3].Value =
                        !string.IsNullOrEmpty(item.Nomenkl.FullName)
                            ? item.Nomenkl.FullName
                            : item.Nomenkl.Name;
                    sheet.Cells[$"AB{startTableRow + row}"].Value = ((IName) item.Nomenkl.Unit)?.Name;
                    sheet.Cells[$"Y{startTableRow + row}"].Value = item.Quantity;
                    sheet.Cells[$"Y{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"AD{startTableRow + row}"].Value = Convert.ToDouble(item.Price);
                    sheet.Cells[$"AD{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"AH{startTableRow + row}"].Value =
                        Convert.ToDouble(item.Summa);
                    sheet.Cells[$"AH{startTableRow + row}"].NumberFormat = "#,##0.00";
                    row++;
                }

                sheet.Cells[$"AH{document.Rows.Count + startTableRow + 2}"].Value =
                    Convert.ToDouble(document.Summa);
                sheet.Cells[$"AH{document.Rows.Count + startTableRow + 3}"].Value =
                    Convert.ToDouble(document.Rows.Sum(_ => _.SFT_SUMMA_NDS));
                sheet.Cells[$"AH{document.Rows.Count + startTableRow + 4}"].Value =
                    Convert.ToDouble(document.Summa);
                sheet.Cells[$"B{document.Rows.Count + startTableRow + 5}"].Value =
                    $"Всего наименований {document.Rows.Count()}, на сумму {document.Summa:n2} {document.Currency.FullName}";
                sheet.Cells[$"B{document.Rows.Count + startTableRow + 6}"].Value =
                    $"{RuDateAndMoneyConverter.CurrencyToTxt(Convert.ToDouble(document.Summa), document.Currency.Name, true)} , в т.ч. НДС {Math.Round((double) (document.Rows.Sum(_ => _.SFT_SUMMA_NDS) ?? 0), 2)}";

                sheet.Cells[$"AC{document.Rows.Count + startTableRow + 11}"].Value = receiver.Header;
                sheet.Cells[$"AC{document.Rows.Count + startTableRow + 14}"].Value = receiver.GlavBuh;
                sheet.Cells[$"AC{document.Rows.Count + startTableRow + 17}"].Value =
                    ((ClientWindowViewModel) ViewModel).Document.PersonaResponsible?.Name;
            }
        }
    }

    public class SFClientSchetFacturaReport : BaseReport
    {
        public SFClientSchetFacturaReport(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            sheet.Cells["J1"].Value = string.IsNullOrWhiteSpace(document.OuterNumber)
                ? document.InnerNumber.ToString()
                : document.OuterNumber;
            sheet.Cells["M1"].Value = $"от {document.DocDate:d}";
            sheet.Cells["B3"].Value = document.Receiver != null
                ? document.Receiver.FullName
                : "";
            sheet.Cells["E12"].Value = document.Currency != null
                ? $"{document.Currency.Name}, {document.Currency.Code}"
                : null;
            if (document.Receiver != null)
            {
                sheet.Cells["B4"].Value = document.Receiver.Address;
                sheet.Cells["B5"].Value = $"ИНН {document.Receiver.INN} / КПП {document.Receiver.KPP}";
                //TODO Добавить грузовые реквизиты в справочник контрагента
                /*sheet.Cells["E6"].Value = !string.IsNullOrEmpty(document.SF_GROZOOTPRAVITEL) ||
                                          document.SF_GROZOOTPRAVITEL != ""
                    ? document.SF_GROZOOTPRAVITEL
                    : document.Receiver.GruzoRequisiteForSchet;*/
            }

            if (document.Client != null)
            {
                sheet.Cells["B9"].Value = document.Client?.FullName;
                //TODO Добавить грузовые реквизиты в справочник контрагента
                /*sheet.Cells["E7"].Value = string.IsNullOrEmpty(document.SF_GRUZOPOLUCHATEL) ||
                                          document.SF_GRUZOPOLUCHATEL != ""
                    ? document.SF_GRUZOPOLUCHATEL
                    : vm.Document.Client.GruzoRequisiteForSchet;*/
                // ReSharper disable once PossibleNullReferenceException
                sheet.Cells["B10"].Value = document.Client.Address;
                sheet.Cells["B11"].Value = $"ИНН {document.Client.INN} / КПП {document.Client.KPP}";
            }

            sheet.Cells["J20"].Value = document.Receiver?.Director;
            sheet.Cells["AB20"].Value = document.Receiver?.GlavBuh;
            var startTableRow = 15;
            for (var i = 2; i <= document.Rows.Count; i++)
            {
                sheet.InsertCells(sheet.Range[string.Format("A{0}:AF{0}", startTableRow + i)],
                    InsertCellsMode.ShiftCellsDown);
                sheet.Rows[$"{startTableRow + i}"].CopyFrom(sheet.Rows["16"]);
            }

            var row = 1;
            foreach (var item in document.Rows.Cast<InvoiceClientRowViewModel>())
            {
                sheet[startTableRow + row - 1, 0].Value =
                    !string.IsNullOrEmpty(item.Nomenkl.FullName) && item.Nomenkl.FullName != ""
                        ? item.Nomenkl.FullName
                        : item.Nomenkl.Name;
                sheet[startTableRow + row - 1, 6].Value = item.Nomenkl.Unit?.OKEI_Code;
                sheet.Cells[$"H{startTableRow + row}"].Value = ((IName) item.Nomenkl.Unit)?.Name;
                sheet.Cells[$"K{startTableRow + row}"].Value = item.Quantity;
                sheet.Cells[$"K{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"M{startTableRow + row}"].Value = Convert.ToDouble(item.Price);
                sheet.Cells[$"M{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"O{startTableRow + row}"].Value =
                    Convert.ToDouble(item.Summa - item.SFT_SUMMA_NDS);
                sheet.Cells[$"O{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"S{startTableRow + row}"].Value = item.NDSPercent;
                sheet.Cells[$"U{startTableRow + row}"].Value =
                    Convert.ToDouble(item.SFT_SUMMA_NDS);
                sheet.Cells[$"U{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"V{startTableRow + row}"].Value =
                    Convert.ToDouble(item.Summa);
                sheet.Cells[$"V{startTableRow + row}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"Y{startTableRow + row}"].Value = item.SFT_COUNTRY_CODE;
                sheet[startTableRow + row - 1, 26].Value = item.SFT_STRANA_PROIS;
                sheet[startTableRow + row - 1, 28].Value = item.GruzoDeclaration;
                row++;
            }

            sheet.Cells[$"O{document.Rows.Count + startTableRow + 1}"].Formula =
                $"SUM(O{startTableRow}:O{document.Rows.Count + startTableRow})";
            sheet.Cells[$"O{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
            sheet.Cells[$"U{document.Rows.Count + startTableRow + 1}"].Formula =
                $"SUM(U{startTableRow}:U{document.Rows.Count + startTableRow})";
            sheet.Cells[$"U{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
            sheet.Cells[$"V{document.Rows.Count + startTableRow + 1}"].Formula =
                $"SUM(V{startTableRow}:V{document.Rows.Count + startTableRow})";
            sheet.Cells[$"V{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
        }
    }

    public class SFClientSchetFacturaReportNew : BaseReport
    {
        public SFClientSchetFacturaReportNew(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        public override void GenerateReport(Worksheet sheet)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var vm = ViewModel as ClientWindowViewModel;
                if (vm == null) return;
                var document = vm.Document;
                sheet.Cells["R2"].Value = string.IsNullOrWhiteSpace(document.OuterNumber)
                    ? document.InnerNumber.ToString()
                    : document.OuterNumber;
                sheet.Cells["AD2"].Value = $"{document.DocDate.ToLongDateString()}";
                sheet.Cells["AB5"].Value = document.Receiver != null
                    ? string.IsNullOrWhiteSpace(document.Receiver.FullName)
                        ? document.Receiver.Name
                        : document.Receiver.FullName
                    : "";
                sheet.Cells["AB15"].Value = document.Currency != null
                    ? $"{document.Currency.NalogName}, {document.Currency.NalogCode}"
                    : null;
                if (document.Receiver != null)
                {
                    sheet.Cells["AB6"].Value = document.Receiver.Address;
                    sheet.Cells["AB7"].Value = $"{document.Receiver.INN}/{document.Receiver.KPP}";
                    var grlist = ctx.SD_43_GRUZO.Where(_ => _.doc_code == document.Receiver.DocCode);

                    if (grlist.Any())
                        switch (grlist.Count())
                        {
                            case 1:
                                sheet.Cells["AB8"].Value = grlist.First().GRUZO_TEXT_SF;
                                break;
                            default:
                            {
                                var g = grlist.FirstOrDefault(_ => _.IsDefault == true);
                                sheet.Cells["AB8"].Value = g != null ? g.GRUZO_TEXT_SF : grlist.First().GRUZO_TEXT_SF;
                                break;
                            }
                        }
                    else
                        sheet.Cells["AB8"].Value = "он же";
                }

                if (document.Client != null)
                {
                    sheet.Cells["AB12"].Value = document.Client?.FullName ?? document.Client.Name;
                    var pgrlist = ctx.SD_43_GRUZO.Where(_ => _.doc_code == document.Client.DocCode);

                    if (pgrlist.Any())
                        switch (pgrlist.Count())
                        {
                            case 1:
                                sheet.Cells["AB9"].Value = pgrlist.First().GRUZO_TEXT_SF;
                                break;
                            default:
                            {
                                var g = pgrlist.FirstOrDefault(_ => _.IsDefault == true);
                                sheet.Cells["AB9"].Value = g != null ? g.GRUZO_TEXT_SF : pgrlist.First().GRUZO_TEXT_SF;
                                break;
                            }
                        }
                    else
                        sheet.Cells["AB9"].Value = "он же";

                    // ReSharper disable once PossibleNullReferenceException
                    sheet.Cells["AB13"].Value = document.Client.Address;
                    sheet.Cells["AB14"].Value = $"{document.Client.INN}/{document.Client.KPP}";
                }

                sheet.Cells["V13"].Value = $"{document.Currency?.NalogName},{document.Currency?.NalogCode}";
                sheet.Cells["AD22"].Value = document.Receiver?.Director;
                //var copy = sheet.Rows["21"];
                var startTableRow = 21;
                for (var i = 2; i <= document.Rows.Count; i++)
                {
                    sheet.Rows.Insert(startTableRow + i - 1);
                    //sheet.InsertCells(copy);
                    sheet.Rows[$"{startTableRow + i - 1}"].CopyFrom(sheet.Rows["21"]);
                }

                var defaultNDS = Convert.ToDecimal(GlobalOptions.SystemProfile.Profile.FirstOrDefault(_
                        => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")
                    ?.ITEM_VALUE);

                var row = 0;
                foreach (var item in document.Rows.Cast<InvoiceClientRowViewModel>())
                {
                    sheet.Cells[$"A{startTableRow + row}"].Value = row + 1;
                    sheet.Cells[$"I{startTableRow + row}"].Value =
                        !string.IsNullOrEmpty(item.Nomenkl.FullName) && item.Nomenkl.FullName != ""
                            ? item.Nomenkl.FullName
                            : item.Nomenkl.Name;
                    var w = sheet.Cells[$"I{startTableRow + row}"].DisplayText.Length / 40;
                    sheet.Cells[$"I{startTableRow + row}"].RowHeight = (w < 1 ? 1 : w) * 80;

                    sheet.Cells[$"C{startTableRow + row}"].Value = item.NomNomenkl;
                    sheet.Cells[$"Z{startTableRow + row}"].Value = item.Nomenkl.Unit?.OKEI_Code;
                    sheet.Cells[$"AB{startTableRow + row}"].Value = ((IName) item.Nomenkl.Unit)?.Name;
                    sheet.Cells[$"AI{startTableRow + row}"].Value = item.Quantity;
                    sheet.Cells[$"AI{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"AM{startTableRow + row}"].Value = Convert.ToDouble(item.Price);
                    sheet.Cells[$"AM{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"AR{startTableRow + row}"].Value =
                        Convert.ToDouble(item.Summa - item.SFT_SUMMA_NDS);
                    sheet.Cells[$"AR{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"BC{startTableRow + row}"].Value = item.NDSPercent == 0 ? defaultNDS : item.NDSPercent;
                    sheet.Cells[$"BG{startTableRow + row}"].Value =
                        Convert.ToDouble(item.SFT_SUMMA_NDS);
                    sheet.Cells[$"BG{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"BM{startTableRow + row}"].Value =
                        Convert.ToDouble(item.Summa);
                    sheet.Cells[$"BM{startTableRow + row}"].NumberFormat = "#,##0.00";
                    sheet.Cells[$"BT{startTableRow + row}"].Value = item.SFT_COUNTRY_CODE;
                    sheet[$"BX{startTableRow + row}"].Value = item.SFT_STRANA_PROIS;
                    sheet[$"CE{startTableRow + row}"].Value = item.GruzoDeclaration;
                    row++;
                }

                sheet.Cells[$"AR{document.Rows.Count + startTableRow + 1}"].Formula =
                    $"SUM(AR{startTableRow}:AR{document.Rows.Count + startTableRow})";
                sheet.Cells[$"AR{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"BG{document.Rows.Count + startTableRow + 1}"].Formula =
                    $"SUM(BG{startTableRow}:BG{document.Rows.Count + startTableRow})";
                sheet.Cells[$"BG{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"BM{document.Rows.Count + startTableRow + 1}"].Formula =
                    $"SUM(BM{startTableRow}:BM{document.Rows.Count + startTableRow})";
                sheet.Cells[$"BM{document.Rows.Count + startTableRow + 1}"].NumberFormat = "#,##0.00";
                sheet.Cells[$"AJ{document.Rows.Count + startTableRow + 3}"].Value = document.Receiver?.Director;
                sheet.Cells[$"BV{document.Rows.Count + startTableRow + 3}"].Value = document.Receiver?.GlavBuh;
                sheet.Cells[$"P{document.Rows.Count + startTableRow + 15}"].Value = document.DocDate.ToLongDateString();
                sheet.Cells[$"AU{document.Rows.Count + startTableRow + 23}"].Value =
                    $"{document?.Client?.FullName ?? document?.Client.Name} " +
                    $"ИНН/КПП {document?.Client.INN}/{document?.Client.KPP}";

                var options = sheet.HeaderFooterOptions;
                options.EvenFooter.Center = $"Стр. {"&P"} из {"&N"}";
                options.OddFooter.Center = $"Стр. {"&P"} из {"&N"}";
            }
        }
    }

    public class SFClientTorg12Report : BaseReport
    {
        public SFClientTorg12Report(ClientWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }

        private void WaibillSetRow(Worksheet sheet, int row, int rowId, InvoiceClientRowViewModel item)
        {
            sheet.Cells[$"A{rowId}"].Value = row;
            sheet.Cells[$"F{rowId}"].Value = string.IsNullOrEmpty(item.Nomenkl.FullName)
                ? item.Nomenkl.Name
                : item.Nomenkl.FullName;
            sheet.Cells[$"V{rowId}"].Value = ((IName) item.Nomenkl.Unit).Name;
            sheet.Cells[$"X{rowId}"].Value = item.Nomenkl.Unit.OKEI_Code;
            sheet.Cells[$"AD{rowId}"].Value = Convert.ToDouble(item.Quantity);
            sheet.Cells[$"AH{rowId}"].Value = Convert.ToDouble(item.Quantity);
            sheet.Cells[$"AQ{rowId}"].Value = Convert.ToDouble(item.Quantity);
            sheet.Cells[$"AV{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.Summa / item.Quantity), 2);
            sheet.Cells[$"BB{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble((item.Summa - item.SFT_SUMMA_NDS) / item.Quantity *
                                     item.Quantity), 2);
            sheet.Cells[$"BG{rowId}"].Value = item.NDSPercent;
            sheet.Cells[$"BQ{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.SFT_SUMMA_NDS *
                                     (item.Quantity / item.Quantity)), 2);
            sheet.Cells[$"BV{rowId}"].Value =
                Math.Round(
                    Convert.ToDouble(item.Summa *
                                     (item.Quantity / item.Quantity)), 2);
        }

        public override void GenerateReport(Worksheet sheet)
        {
            var vm = ViewModel as ClientWindowViewModel;
            if (vm == null) return;
            var document = vm.Document;
            //TODO Добавить грузовые реквизиты в справочник контрагента
            //sheet.Cells["A4"].Value = vm.Document.Receiver.GruzoRequisiteForWaybill;
            //sheet.Cells["K12"].Value = vm.Document.Client.GruzoRequisiteForWaybill;
            //sheet.Cells["K19"].Value = vm.Document.Receiver.GruzoRequisiteForWaybill;
            //sheet.Cells["K24"].Value = vm.Document.Client.GruzoRequisiteForWaybill;
            sheet["AE33"].Value = vm.Document.OuterNumber;
            sheet["AR33"].Value = vm.Document.DocDate.ToShortDateString();
            sheet["BX5"].Value = vm.Document.Receiver.OKPO;
            sheet["BX14"].Value = vm.Document.Client.OKPO;
            sheet["BX17"].Value = vm.Document.Receiver.OKPO;
            sheet["BX22"].Value = vm.Document.Client.OKPO;
            sheet["P79"].Value = vm.Document.DocDate.ToLongDateString();
            sheet["AZ79"].Value = vm.Document.DocDate.ToLongDateString();
            //var itog = sheet.Range["A41:CA41"];
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
            foreach (var item in document.Rows.Cast<InvoiceClientRowViewModel>())
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
                    document.Rows.Sum(_ => (_.Summa - _.SFT_SUMMA_NDS) *
                                           (_.Quantity / _.Quantity))), 2);
            sheet.Cells[$"BQ{document.Rows.Count + startTableRow + 1}"].Value =
                Math.Round(Convert.ToDouble(
                    document.Rows.Sum(_ => _.SFT_SUMMA_NDS *
                                           (_.Quantity / _.Quantity))), 2);
            sheet.Cells[$"BV{document.Rows.Count + startTableRow + 1}"].Value =
                Math.Round(Convert.ToDouble(
                    document.Rows.Sum(_ => _.Summa *
                                           (_.Quantity / _.Quantity))), 2);
            sheet.Cells[$"AB{document.Rows.Count + startTableRow + 4}"].Value = vm.Document.Rows.Count;
            sheet.Cells[$"O{document.Rows.Count + startTableRow + 30}"].Value = vm.Document.Receiver.GlavBuh;
            sheet.Cells[$"J{document.Rows.Count + startTableRow + 9}"].Value =
                RuDateAndMoneyConverter.NumeralsDoubleToTxt(Convert.ToDouble(document.Rows.Count), 0,
                    TextCase.Accusative, true);
            sheet.Cells[$"M{document.Rows.Count + startTableRow + 9}"].Value =
                RuDateAndMoneyConverter.CurrencyToTxt(
                    sheet.Cells[$"BV{document.Rows.Count + startTableRow + 1}"].Value.NumericValue,
                    document.Currency.Name,
                    true);
        }
    }
}
