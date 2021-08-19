using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Invoices;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.Managers.Invoices;
using KursAM2.ViewModel.Finance.Cash;

namespace KursAM2.Managers
{
    public class CashManager
    {
        private static readonly WindowManager winManager = new WindowManager();

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static void DeleteDocument(CashDocumentType docType, object doc)
        {
            var docDate = DateTime.Today;
            decimal docCRS = 0;
            decimal cashDC = 0;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        switch (docType)
                        {
                            case CashDocumentType.CashIn:
                                if (!(doc is CashIn delCashIn)) return;
                                docDate = (DateTime) delCashIn.DATE_ORD;
                                docCRS = (decimal) delCashIn.CRS_DC;
                                cashDC = (decimal) delCashIn.CA_DC;
                                var delIn = ctx.SD_33.FirstOrDefault(_ => _.DOC_CODE == delCashIn.DocCode);
                                if (delIn == null) return;
                                ctx.SD_33.Remove(delIn);
                                break;
                            case CashDocumentType.CashOut:
                                if (!(doc is CashOut delCashOut)) return;
                                var delOut = ctx.SD_34.FirstOrDefault(_ => _.DOC_CODE == delCashOut.DocCode);
                                if (delOut == null) return;
                                ctx.SD_34.Remove(delOut);
                                break;
                            case CashDocumentType.CurrencyExchange:
                                if (!(doc is CashCurrencyExchange delCrsExch)) return;
                                docDate = delCrsExch.CH_DATE;
                                docCRS = (decimal) delCrsExch.CH_CRS_IN_DC;
                                cashDC = delCrsExch.CH_CASH_DC;
                                var delCe = new SD_251 {DOC_CODE = delCrsExch.DocCode};
                                ctx.SD_251.Attach(delCe);
                                ctx.SD_251.Remove(delCe);
                                break;
                        }

                        ctx.SaveChanges();
                        if (docCRS != 0)
                        {
                            var daterems = ctx.SD_251.Where(_ => _.CH_DATE >= docDate).Select(_ => _.CH_DATE).Distinct()
                                .ToList();
                            foreach (var d in ctx.SD_34.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var d in ctx.SD_33.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var r in daterems)
                                if (GetCashCurrencyRemains(ctx, cashDC, docCRS, r) < 0)
                                {
                                    winManager.ShowWinUIMessageBox("В кассе возникли отрицательные остатки. " +
                                                                   "Сохранение не возможно", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    transaction.Rollback();
                                    return;
                                }
                        }

                        transaction.Commit();
                        if (doc is RSViewModelBase dd)
                            dd.myState = RowStatus.Deleted;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public static SD_33 CashInViewModelToEntity(CashIn ent)
        {
            var ret = new SD_33
            {
                DOC_CODE = ent.DocCode,
                NUM_ORD = ent.NUM_ORD,
                SUMM_ORD = ent.SUMM_ORD,
                NAME_ORD = ent.NAME_ORD,
                OSN_ORD = ent.OSN_ORD,
                NOTES_ORD = ent.NOTES_ORD,
                DATE_ORD = ent.DATE_ORD,
                CODE_CASS = ent.CODE_CASS,
                TABELNUMBER = ent.TABELNUMBER,
                OP_CODE = ent.OP_CODE,
                CA_DC = ent.CA_DC,
                KONTRAGENT_DC = ent.KONTRAGENT_DC,
                SHPZ_ORD = ent.SHPZ_ORD,
                SHPZ_DC = ent.Entity.SHPZ_DC,
                RASH_ORDER_FROM_DC = ent.RASH_ORDER_FROM_DC,
                SFACT_DC = ent.SFACT_DC,
                NCODE = ent.NCODE,
                POS_DC = ent.POS_DC,
                POS_PREV_OST = ent.POS_PREV_OST,
                POS_PRIHOD = ent.POS_PRIHOD,
                POS_NOW_OST = ent.POS_NOW_OST,
                KONTR_CRS_DC = ent.KONTR_CRS_DC ?? ent.CRS_DC,
                CRS_KOEF = ent.CRS_KOEF,
                CRS_SUMMA = ent.CRS_SUMMA,
                CHANGE_ORD = ent.CHANGE_ORD,
                CRS_DC = ent.CRS_DC,
                UCH_VALUTA_DC = ent.UCH_VALUTA_DC,
                SUMMA_V_UCH_VALUTE = ent.SUMMA_V_UCH_VALUTE,
                UCH_VALUTA_RATE = ent.UCH_VALUTA_RATE,
                SUM_NDS = ent.SUM_NDS,
                SFACT_OPLACHENO = ent.SFACT_OPLACHENO,
                SFACT_CRS_RATE = ent.SFACT_CRS_RATE,
                SFACT_CRS_DC = ent.SFACT_CRS_DC,
                BANK_RASCH_SCHET_DC = ent.BANK_RASCH_SCHET_DC,
                CREATOR = ent.CREATOR,
                RUB_SUMMA = ent.RUB_SUMMA,
                RUB_RATE = ent.RUB_RATE,
                OBRATNY_RASCHET = ent.OBRATNY_RASCHET,
                KONTR_CRS_SUM_CORRECT_PERCENT = ent.KONTR_CRS_SUM_CORRECT_PERCENT,
                V_TOM_CHISLE = ent.V_TOM_CHISLE,
                KONTR_FROM_DC = ent.KONTR_FROM_DC,
                SFACT_FLAG = ent.SFACT_FLAG
            };
            return ret;
        }

        public static CashIn LoadCashIn(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_33
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_34)
                        .Include(_ => _.VD_46)
                        .Include(_ => _.SD_90)
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_43)
                        .Include(_ => _.AccuredAmountForClientRow)
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    var doc = new CashIn(data)
                    {
                        myState = RowStatus.NotEdited
                    };
                    if (doc.SFACT_DC != null)
                    {
                        var inv = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == doc.SFACT_DC);
                        if (inv == null) return doc;
                        var pDocs = new List<InvoicePaymentDocument>();
                        foreach (var c in ctx.SD_33.Where(_ => _.SFACT_DC == doc.SFACT_DC).ToList())
                            pDocs.Add(new InvoicePaymentDocument
                            {
                                DocCode = c.DOC_CODE,
                                Code = 0,
                                DocumentType = DocumentType.CashIn,
                                DocumentName =
                                    // ReSharper disable once PossibleInvalidOperationException
                                    $"{c.NUM_ORD} от {c.DATE_ORD} на {c.SUMM_ORD} {MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                                // ReSharper disable once PossibleInvalidOperationException
                                Summa = (decimal) c.SUMM_ORD,
                                Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                                Note = c.NOTES_ORD
                            });
                        foreach (var c in ctx.TD_101.Include(_ => _.SD_101)
                            .Where(_ => _.VVT_SFACT_CLIENT_DC == doc.SFACT_DC).ToList())
                            pDocs.Add(new InvoicePaymentDocument
                            {
                                DocCode = c.DOC_CODE,
                                Code = c.CODE,
                                DocumentType = DocumentType.Bank,
                                DocumentName =
                                    // ReSharper disable once PossibleInvalidOperationException
                                    $"{c.SD_101.VV_START_DATE} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                                Summa = (decimal) c.VVT_VAL_PRIHOD,
                                Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                                Note = c.VVT_DOC_NUM
                            });
                        // ReSharper disable once PossibleInvalidOperationException
                        doc.MaxSumma = (decimal) (doc.SUMM_ORD + inv.SF_CRS_SUMMA_K_OPLATE - pDocs.Sum(_ => _.Summa));
                    }

                    doc.RaisePropertyChanged("IsSummaEnabled");
                    return doc;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return null;
        }

        public static bool CheckCashIn(CashIn doc)
        {
            return doc.Cash != null && doc.SUMM_ORD != null && doc.SUMM_ORD != 0 &&
                   !string.IsNullOrWhiteSpace(doc.Kontragent)
                   && doc.CA_DC != null && doc.CRS_DC != null && doc.DATE_ORD != null && doc.Entity.SHPZ_DC != null;
        }

        public static bool CheckCashOut(CashOut doc)
        {
            return doc.Cash != null && doc.SUMM_ORD != null && doc.SUMM_ORD != 0 &&
                   !string.IsNullOrWhiteSpace(doc.Kontragent)
                   && doc.CA_DC != null && doc.CRS_DC != null && doc.DATE_ORD != null && doc.SHPZ_DC != null;
        }

        public static bool CheckCashCurrencyExchange(CashCurrencyExchange doc)
        {
            return doc.Cash != null && !string.IsNullOrWhiteSpace(doc.KontragentName)
                                    && doc.SDRSchet != null;
        }

        public static CashIn NewCashIn()
        {
            var ret = new CashIn
            {
                DocCode = -1,
                DATE_ORD = DateTime.Today,
                CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                    ? GlobalOptions.UserInfo.FullName
                    : GlobalOptions.UserInfo.NickName,
                Currency = GlobalOptions.SystemProfile.NationalCurrency,
                KontragentType = CashKontragentType.NotChoice,
                State = RowStatus.NewRow,
                NAME_ORD = "",
                OP_CODE = 0,
                UCH_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                UCH_VALUTA_RATE = 1,
                Id = Guid.NewGuid(),
                CRS_KOEF = 1
            };
            return ret;
        }

        public static CashIn NewCopyCashIn(decimal dc)
        {
            var ret = LoadCashIn(dc);
            ret.DocCode = -1;
            ret.DATE_ORD = DateTime.Today;
            ret.SUMM_ORD = 0;
            //ret.Note = null;
            ret.CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                ? GlobalOptions.UserInfo.FullName
                : GlobalOptions.UserInfo.NickName;
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            switch (ret.KontragentType)
            {
                case CashKontragentType.Kontragent:
                    ret.RASH_ORDER_FROM_DC = null;
                    ret.BANK_RASCH_SCHET_DC = null;
                    break;
                case CashKontragentType.Bank:
                    ret.KONTRAGENT_DC = null;
                    ret.RASH_ORDER_FROM_DC = null;
                    break;
                case CashKontragentType.Cash:
                    ret.KONTRAGENT_DC = null;
                    ret.RASH_ORDER_FROM_DC = null;
                    ret.BANK_RASCH_SCHET_DC = null;
                    break;
            }

            ret.SFACT_DC = null;
            ret.SFactName = null;
            ret.RashodOrderFromName = null;
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            ret.State = RowStatus.NewRow;
            ret.Id = Guid.NewGuid();
            ret.RaisePropertyChanged("Kontragent");
            ret.RaisePropertyChanged("State");
            return ret;
        }

        public static CashIn NewRequisiteCashIn(decimal dc)
        {
            var ret = LoadCashIn(dc);
            ret.DocCode = -1;
            //ret.DATE_ORD = DateTime.Today;
            ret.NAME_ORD = null;
            ret.OSN_ORD = null;
            ret.Note = null;
            ret.SUMM_ORD = 0;
            ret.CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                ? GlobalOptions.UserInfo.FullName
                : GlobalOptions.UserInfo.NickName;
            ret.Id = Guid.NewGuid();
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            ret.KONTRAGENT_DC = null;
            ret.RASH_ORDER_FROM_DC = null;
            ret.BANK_RASCH_SCHET_DC = null;
            ret.SFACT_DC = null;
            ret.SFactName = null;
            ret.RashodOrderFromName = null;
            ret.State = RowStatus.NewRow;
            ret.RaisePropertyChanged("Kontragent");
            ret.RaisePropertyChanged("State");
            return ret;
        }

        public static CashOut NewCashOut()
        {
            var ret = new CashOut
            {
                DocCode = -1,
                DATE_ORD = DateTime.Today,
                CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                    ? GlobalOptions.UserInfo.FullName
                    : GlobalOptions.UserInfo.NickName,
                Currency = GlobalOptions.SystemProfile.NationalCurrency,
                KontragentType = CashKontragentType.NotChoice,
                State = RowStatus.NewRow,
                NAME_ORD = "",
                OP_CODE = 0,
                UCH_VALUTA_DC = GlobalOptions.SystemProfile.MainCurrency.DocCode,
                UCH_VALUTA_RATE = 1,
                Id = Guid.NewGuid(),
                CRS_KOEF = 1
            };
            return ret;
        }

        public static CashOut NewCopyCashOut(decimal dc)
        {
            var ret = LoadCashOut(dc);
            ret.DocCode = -1;
            ret.DATE_ORD = DateTime.Today;
            //ret.Note = null;
            ret.CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                ? GlobalOptions.UserInfo.FullName
                : GlobalOptions.UserInfo.NickName;
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            //ret.KONTRAGENT_DC = null;
            //ret.CASH_TO_DC = null;
            ret.BANK_RASCH_SCHET_DC = null;
            ret.SPOST_DC = null;
            ret.SPostName = null;
            //ret.TABELNUMBER = null;
            ret.State = RowStatus.NewRow;
            ret.Id = Guid.NewGuid();
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            ret.RaisePropertyChanged("State");
            return ret;
        }

        public static CashOut NewRequisisteCashOut(decimal dc)
        {
            var ret = LoadCashOut(dc);
            ret.DocCode = -1;
            ret.SUMM_ORD = 0;
            //ret.DATE_ORD = DateTime.Today;
            ret.OSN_ORD = null;
            ret.NAME_ORD = null;
            ret.Note = null;
            ret.CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                ? GlobalOptions.UserInfo.FullName
                : GlobalOptions.UserInfo.NickName;
            ret.State = RowStatus.NewRow;
            ret.Id = Guid.NewGuid();
            ret.CRS_KOEF = ret.CRS_KOEF ?? 1;
            ret.KONTRAGENT_DC = null;
            ret.CASH_TO_DC = null;
            ret.BANK_RASCH_SCHET_DC = null;
            ret.SPOST_DC = null;
            ret.SPostName = null;
            ret.TABELNUMBER = null;
            ret.RaisePropertyChanged("Kontragent");
            return ret;
        }

        public static CashCurrencyExchange NewCashCurrencyEchange()
        {
            var doc = new CashCurrencyExchange
            {
                DocCode = -1,
                CH_DATE = DateTime.Today,
                CH_DATE_IN = DateTime.Today,
                CH_DATE_OUT = DateTime.Today,
                CH_CRS_OUT_SUM = 0,
                CH_CRS_IN_SUM = 0,
                CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                    ? GlobalOptions.UserInfo.FullName
                    : GlobalOptions.UserInfo.NickName,
                State = RowStatus.NewRow
            };
            return doc;
        }

        public static CashCurrencyExchange NewCopyCashCurrencyExchange(decimal dc)
        {
            var ret = LoadCurrencyExchange(dc);
            ret.DocCode = -1;
            ret.CH_DATE = DateTime.Today;
            ret.CH_DATE_IN = DateTime.Today;
            ret.CH_DATE_OUT = DateTime.Today;
            ret.Note = null;
            ret.CREATOR = string.IsNullOrEmpty(GlobalOptions.UserInfo.NickName)
                ? GlobalOptions.UserInfo.FullName
                : GlobalOptions.UserInfo.NickName;
            ret.State = RowStatus.NewRow;
            return ret;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static void UpdateDocument(CashDocumentType docType, object doc, DateTime? oldDate = null,
            DateTime? oldDate2 = null)
        {
            var docDate = DateTime.Today;
            decimal docCRS = 0;
            decimal cashDC = 0;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        TD_22 cashstart;
                        switch (docType)
                        {
                            case CashDocumentType.CashIn:
                                if (!(doc is CashIn updateCashIn)) return;
                                docDate = (DateTime) updateCashIn.DATE_ORD;
                                docCRS = (decimal) updateCashIn.CRS_DC;
                                cashDC = (decimal) updateCashIn.CA_DC;
                                cashstart = ctx.TD_22.FirstOrDefault(_ => _.CRS_DC == updateCashIn.Currency.DocCode
                                                                          && updateCashIn.Cash.DocCode == _.DOC_CODE);
                                if (cashstart == null || updateCashIn.DATE_ORD < cashstart.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата документа раньше начальных остатков ({cashstart?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                if (updateCashIn.SFACT_DC != null)
                                {
                                    var sql =
                                        "SELECT s84.doc_code as DocCode, s84.SF_CRS_SUMMA_K_OPLATE as Summa, SUM(ISNULL(s33.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                        "FROM sd_84 s84 " +
                                        $"LEFT OUTER JOIN sd_33 s33 ON s33.SFACT_DC = s84.DOC_CODE AND s33.DOC_CODE != {CustomFormat.DecimalToSqlDecimal(updateCashIn.DocCode)} " +
                                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_CLIENT_DC = s84.DOC_CODE " +
                                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SFACT_DC = s84.DOC_CODE " +
                                        $"WHERE s84.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(updateCashIn.SFACT_DC)} " +
                                        "GROUP BY s84.doc_code, s84.SF_CRS_SUMMA_K_OPLATE ";
                                    var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                        .FirstOrDefault();
                                    if (pays != null)
                                        if (pays.Summa < pays.PaySumma + updateCashIn.SUMM_ORD)
                                        {
                                            var res = winManager.ShowWinUIMessageBox(
                                                $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + updateCashIn.SUMM_ORD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                            switch (res)
                                            {
                                                case MessageBoxResult.Yes:
                                                    updateCashIn.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    updateCashIn.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    break;
                                                case MessageBoxResult.No:
                                                    return;
                                            }
                                        }
                                }

                                ctx.Entry(CashInViewModelToEntity(updateCashIn)).State = EntityState.Modified;
                                ctx.SaveChanges();
                                break;
                            case CashDocumentType.CashOut:
                                if (!(doc is CashOut updateCashOut)) return;
                                docDate = (DateTime) updateCashOut.DATE_ORD;
                                docCRS = (decimal) updateCashOut.CRS_DC;
                                cashDC = (decimal) updateCashOut.CA_DC;
                                cashstart = ctx.TD_22.FirstOrDefault(_ => _.CRS_DC == updateCashOut.Currency.DocCode
                                                                          && updateCashOut.Cash.DocCode == _.DOC_CODE);
                                if (cashstart == null || updateCashOut.DATE_ORD < cashstart.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата документа раньше начальных остатков ({cashstart?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                if (updateCashOut.SPOST_DC != null)
                                {
                                    var old = ctx.ProviderInvoicePay.FirstOrDefault(_ =>
                                        _.CashDC == updateCashOut.DocCode
                                        && _.DocDC == updateCashOut.SPOST_DC);
                                    if (old == null)
                                        ctx.ProviderInvoicePay.Add(new ProviderInvoicePay
                                        {
                                            Id = Guid.NewGuid(),
                                            Rate = 1,
                                            // ReSharper disable once PossibleInvalidOperationException
                                            Summa = (decimal) updateCashOut.SUMM_ORD,
                                            CashDC = updateCashOut.DocCode,
                                            // ReSharper disable once PossibleInvalidOperationException
                                            DocDC = (decimal) updateCashOut.SPOST_DC
                                        });
                                    else
                                        // ReSharper disable once PossibleInvalidOperationException
                                        old.Summa = (decimal) updateCashOut.SUMM_ORD;
                                    var sql =
                                        "SELECT s26.doc_code as DocCode, s26.SF_CRS_SUMMA as Summa, SUM(ISNULL(s34.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_RASHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                        "FROM sd_26 s26 " +
                                        $"LEFT OUTER JOIN sd_34 s34 ON s34.SPOST_DC = s26.DOC_CODE AND s34.DOC_CODE != {CustomFormat.DecimalToSqlDecimal(updateCashOut.DocCode)} " +
                                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_POSTAV_DC = s26.DOC_CODE " +
                                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SPOST_DC = s26.DOC_CODE " +
                                        $"WHERE s26.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(updateCashOut.SPOST_DC)} " +
                                        "GROUP BY s26.doc_code, s26.SF_CRS_SUMMA ";
                                    var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                        .FirstOrDefault();
                                    if (pays != null)
                                        if (pays.Summa < pays.PaySumma + updateCashOut.SUMM_ORD)
                                        {
                                            var res = winManager.ShowWinUIMessageBox(
                                                $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + updateCashOut.SUMM_ORD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                            switch (res)
                                            {
                                                case MessageBoxResult.Yes:
                                                    updateCashOut.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    updateCashOut.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    break;
                                                case MessageBoxResult.No:
                                                    return;
                                            }
                                        }
                                }

                                ctx.Entry(CashOutViewModelToEntity(updateCashOut)).State = EntityState.Modified;
                                ctx.SaveChanges();
                                break;
                            case CashDocumentType.CurrencyExchange:
                                if (!(doc is CashCurrencyExchange udateCrsExch)) return;
                                docDate = udateCrsExch.CH_DATE;
                                docCRS = (decimal) udateCrsExch.CH_CRS_OUT_DC;
                                cashDC = udateCrsExch.CH_CASH_DC;
                                var cashstartIn = ctx.TD_22.FirstOrDefault(_ =>
                                    _.CRS_DC == udateCrsExch.CurrencyIn.DocCode
                                    && udateCrsExch.Cash.DocCode == _.DOC_CODE);
                                var cashstartOut = ctx.TD_22.FirstOrDefault(_ =>
                                    _.CRS_DC == udateCrsExch.CurrencyOut.DocCode
                                    && udateCrsExch.Cash.DocCode == _.DOC_CODE);
                                if (cashstartIn == null || udateCrsExch.CH_DATE_IN < cashstartIn.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата прихода раньше начальных остатков ({cashstartIn?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                if (cashstartOut == null || udateCrsExch.CH_DATE_IN < cashstartOut.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата выплаты раньше начальных остатков ({cashstartOut?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                ctx.Entry(CurrencyExchangeViewModelToEntity(udateCrsExch)).State = EntityState.Modified;
                                ctx.SaveChanges();
                                break;
                        }

                        if (docCRS != 0)
                        {
                            var daterems = ctx.SD_251.Where(_ => _.CH_DATE >= docDate).Select(_ => _.CH_DATE).Distinct()
                                .ToList();
                            foreach (var d in ctx.SD_34.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var d in ctx.SD_33.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var r in daterems)
                                if (GetCashCurrencyRemains(ctx, cashDC, docCRS, r) < 0)
                                {
                                    winManager.ShowWinUIMessageBox("В кассе возникли отрицательные остатки. " +
                                                                   "Сохранение не возможно", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    transaction.Rollback();
                                    return;
                                }
                        }

                        transaction.Commit();
                        if (doc is RSViewModelBase dd)
                        {
                            dd.myState = RowStatus.NotEdited;
                            dd.RaisePropertyChanged("State");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        /// <summary>
        ///     Создание набора валют для новой даты по кассе
        /// </summary>
        //private static void CreateNewSD_39(decimal cashDC, DateTime date, ALFAMEDIAEntities ctx)
        //{
        //    var crsList = ctx.TD_22.Where(_ => _.DOC_CODE == cashDC).ToList();
        //    var newDC = ctx.SD_39.Any() ? ctx.SD_39.Max(_ => _.DOC_CODE) : 10390000001;
        //    foreach (var crs in crsList)
        //    {
        //        newDC++;
        //        var sdate = ctx.SD_39
        //            .Where(_ => _.CA_DC == cashDC && _.CRS_DC == crs.CRS_DC && _.DATE_CASS < date)
        //            .Max(_ => _.DATE_CASS);
        //        var ssum = ctx.SD_39
        //            .FirstOrDefault(
        //                _ => _.CA_DC == cashDC && _.CRS_DC == crs.CRS_DC && _.DATE_CASS == sdate)
        //            ?.MONEY_STOP ?? 0;
        //        var newItem = new SD_39
        //        {
        //            DOC_CODE = newDC,
        //            CRS_DC = crs.CRS_DC,
        //            DATE_CASS = date,
        //            CA_DC = cashDC,
        //            MONEY_STOP = ssum,
        //            MONEY_START = ssum
        //        };
        //        ctx.SD_39.Add(newItem);
        //    }

        //    ctx.SaveChanges();
        //}
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static void InsertDocument(CashDocumentType docType, object doc)
        {
            var docDate = DateTime.Today;
            decimal docCRS = 0;
            decimal cashDC = 0;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        TD_22 cashstart;
                        switch (docType)
                        {
                            case CashDocumentType.CashIn:
                                if (!(doc is CashIn insertCashIn)) return;
                                cashstart = ctx.TD_22.FirstOrDefault(_ => _.CRS_DC == insertCashIn.Currency.DocCode
                                                                          && insertCashIn.Cash.DocCode == _.DOC_CODE);
                                if (cashstart == null || insertCashIn.DATE_ORD < cashstart.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата документа раньше начальных остатков ({cashstart?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                var ent = CashInViewModelToEntity(insertCashIn);
                                if (ent.DOC_CODE < 0)
                                {
                                    ent.DOC_CODE =
                                        ctx.SD_33.Any() ? ctx.SD_33.Max(_ => _.DOC_CODE) + 1 : 10330000001;
                                    ent.NUM_ORD =
                                        ctx.SD_33.Any(_ => _.DATE_ORD.Value.Year == insertCashIn.DATE_ORD.Value.Year)
                                            ? ctx.SD_33.Max(_ => _.NUM_ORD) + 1
                                            : 1;
                                }

                                if (insertCashIn.SFACT_DC != null)
                                {
                                    var sql =
                                        "SELECT s84.doc_code as DocCode, s84.SF_CRS_SUMMA_K_OPLATE as Summa, SUM(ISNULL(s33.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                        "FROM sd_84 s84 " +
                                        $"LEFT OUTER JOIN sd_33 s33 ON s33.SFACT_DC = s84.DOC_CODE AND s33.DOC_CODE != {CustomFormat.DecimalToSqlDecimal(ent.DOC_CODE)} " +
                                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_CLIENT_DC = s84.DOC_CODE " +
                                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SFACT_DC = s84.DOC_CODE " +
                                        $"WHERE s84.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(insertCashIn.SFACT_DC)} " +
                                        "GROUP BY s84.doc_code, s84.SF_CRS_SUMMA_K_OPLATE ";
                                    var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                        .FirstOrDefault();
                                    if (pays != null)
                                        if (pays.Summa < pays.PaySumma + insertCashIn.SUMM_ORD)
                                        {
                                            var res = winManager.ShowWinUIMessageBox(
                                                $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + insertCashIn.SUMM_ORD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                            switch (res)
                                            {
                                                case MessageBoxResult.Yes:
                                                    ent.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    ent.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    insertCashIn.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    insertCashIn.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    break;
                                                case MessageBoxResult.No:
                                                    return;
                                            }
                                        }
                                }

                                ctx.Entry(ent).State = EntityState.Added;
                                if (((CashIn) doc).AcrruedAmountRow != null)
                                {
                                    var old = ctx.AccuredAmountForClientRow.FirstOrDefault(_ =>
                                        _.Id == ((CashIn) doc).AcrruedAmountRow.Id);
                                    if (old != null)
                                        old.CashDC = ent.DOC_CODE;
                                }

                                ctx.SaveChanges();
                                if (insertCashIn.DocCode < 0)
                                {
                                    insertCashIn.DocCode = ent.DOC_CODE;
                                    insertCashIn.NUM_ORD = ent.NUM_ORD;
                                }

                                break;
                            case CashDocumentType.CashOut:
                                if (!(doc is CashOut insertCashOut)) return;
                                docDate = (DateTime) insertCashOut.DATE_ORD;
                                docCRS = (decimal) insertCashOut.CRS_DC;
                                cashDC = (decimal) insertCashOut.CA_DC;
                                cashstart = ctx.TD_22.FirstOrDefault(_ => _.CRS_DC == insertCashOut.Currency.DocCode
                                                                          && insertCashOut.Cash.DocCode == _.DOC_CODE);
                                if (cashstart == null || insertCashOut.DATE_ORD < cashstart.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата документа раньше начальных остатков ({cashstart?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                var ent1 = CashOutViewModelToEntity(insertCashOut);
                                if (insertCashOut.DocCode < 0)
                                {
                                    ent1.DOC_CODE =
                                        ctx.SD_34.Any() ? ctx.SD_34.Max(_ => _.DOC_CODE) + 1 : 10340000001;
                                    ent1.NUM_ORD =
                                        ctx.SD_34.Any(_ => _.DATE_ORD.Value.Year == insertCashOut.DATE_ORD.Value.Year)
                                            ? ctx.SD_34.Max(_ => _.NUM_ORD) + 1
                                            : 1;
                                }

                                if (insertCashOut.SPOST_DC != null)
                                {
                                    var old = ctx.ProviderInvoicePay.FirstOrDefault(_ =>
                                        _.CashDC == insertCashOut.DocCode
                                        && _.DocDC == insertCashOut.SPOST_DC);
                                    if (old == null)
                                        ctx.ProviderInvoicePay.Add(new ProviderInvoicePay
                                        {
                                            Id = Guid.NewGuid(),
                                            Rate = 1,
                                            // ReSharper disable once PossibleInvalidOperationException
                                            Summa = (decimal) insertCashOut.SUMM_ORD,
                                            CashDC = ent1.DOC_CODE,
                                            // ReSharper disable once PossibleInvalidOperationException
                                            DocDC = (decimal) insertCashOut.SPOST_DC
                                        });
                                    else
                                        // ReSharper disable once PossibleInvalidOperationException
                                        old.Summa = (decimal) insertCashOut.SUMM_ORD;
                                    var sql =
                                        "SELECT s26.doc_code as DocCode, s26.SF_CRS_SUMMA as Summa, SUM(ISNULL(s34.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_RASHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                                        "FROM sd_26 s26 " +
                                        $"LEFT OUTER JOIN sd_34 s34 ON s34.SPOST_DC = s26.DOC_CODE AND s34.DOC_CODE != {CustomFormat.DecimalToSqlDecimal(ent1.DOC_CODE)} " +
                                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_POSTAV_DC = s26.DOC_CODE " +
                                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SPOST_DC = s26.DOC_CODE " +
                                        $"WHERE s26.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(insertCashOut.SPOST_DC)} " +
                                        "GROUP BY s26.doc_code, s26.SF_CRS_SUMMA ";
                                    var pays = ctx.Database.SqlQuery<InvoicesManager.InvoicePayment>(sql)
                                        .FirstOrDefault();
                                    if (pays != null)
                                        if (pays.Summa < pays.PaySumma + insertCashOut.SUMM_ORD)
                                        {
                                            var res = winManager.ShowWinUIMessageBox(
                                                $"Сумма счета {pays.Summa:n2} меньше сумм платежей {pays.PaySumma + insertCashOut.SUMM_ORD:n2}. Установить максимально возможную сумму {pays.Summa - pays.PaySumma}?",
                                                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                            switch (res)
                                            {
                                                case MessageBoxResult.Yes:
                                                    ent1.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    ent1.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    insertCashOut.SUMM_ORD = pays.Summa - pays.PaySumma;
                                                    insertCashOut.CRS_SUMMA = pays.Summa - pays.PaySumma;
                                                    break;
                                                case MessageBoxResult.No:
                                                    return;
                                            }
                                        }
                                }

                                ctx.Entry(ent1).State = EntityState.Added;
                                ctx.SaveChanges();
                                if (insertCashOut.DocCode < 0)
                                {
                                    insertCashOut.DocCode = ent1.DOC_CODE;
                                    insertCashOut.NUM_ORD = ent1.NUM_ORD;
                                }

                                break;
                            case CashDocumentType.CurrencyExchange:
                                if (!(doc is CashCurrencyExchange insCrsExch)) return;
                                docDate = insCrsExch.CH_DATE;
                                docCRS = (decimal) insCrsExch.CH_CRS_OUT_DC;
                                cashDC = insCrsExch.CH_CASH_DC;
                                var cashstartIn = ctx.TD_22.FirstOrDefault(_ =>
                                    _.CRS_DC == insCrsExch.CurrencyIn.DocCode
                                    && insCrsExch.Cash.DocCode == _.DOC_CODE);
                                var cashstartOut = ctx.TD_22.FirstOrDefault(_ =>
                                    _.CRS_DC == insCrsExch.CurrencyOut.DocCode
                                    && insCrsExch.Cash.DocCode == _.DOC_CODE);
                                if (cashstartIn == null || insCrsExch.CH_DATE_IN < cashstartIn.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата прихода раньше начальных остатков ({cashstartIn?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                if (cashstartOut == null || insCrsExch.CH_DATE_IN < cashstartOut.DATE_START)
                                {
                                    transaction.Rollback();
                                    winManager.ShowWinUIMessageBox(
                                        $"Дата выплаты раньше начальных остатков ({cashstartOut?.DATE_START}) по кассе." +
                                        "Вставка документа не возможна.", "Ошибка", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                var ent2 = CurrencyExchangeViewModelToEntity(insCrsExch);
                                insCrsExch.DOC_CODE =
                                    ctx.SD_251.Any() ? ctx.SD_251.Max(_ => _.DOC_CODE) + 1 : 12510000001;
                                insCrsExch.CH_NUM_ORD =
                                    ctx.SD_251.Any(_ => _.CH_DATE.Year == insCrsExch.CH_DATE.Year)
                                        ? ctx.SD_251.Max(_ => _.CH_NUM_ORD) + 1
                                        : 1;
                                ctx.Entry(ent2).State = EntityState.Added;
                                ctx.SaveChanges();
                                insCrsExch.DocCode = ent2.DOC_CODE;
                                insCrsExch.CH_NUM_ORD = ent2.CH_NUM_ORD;
                                break;
                        }

                        if (docCRS != 0)
                        {
                            var daterems = ctx.SD_251.Where(_ => _.CH_DATE >= docDate).Select(_ => _.CH_DATE)
                                .Distinct().ToList();
                            foreach (var d in ctx.SD_34.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var d in ctx.SD_33.Where(_ => _.DATE_ORD >= docDate)
                                .Select(k => k.DATE_ORD)
                                .Distinct().ToList())
                            {
                                if (daterems.Contains((DateTime) d)) continue;
                                daterems.Add(d.Value);
                            }

                            foreach (var r in daterems)
                                if (GetCashCurrencyRemains(ctx, cashDC, docCRS, r) < 0)
                                {
                                    winManager.ShowWinUIMessageBox("В кассе возникли отрицательные остатки. " +
                                                                   "Сохранение не возможно", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    transaction.Rollback();
                                    return;
                                }
                        }

                        transaction.Commit();
                        if (doc is RSViewModelBase dd)
                        {
                            dd.myState = RowStatus.NotEdited;
                            dd.RaisePropertyChanged("State");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        //transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public static SD_34 CashOutViewModelToEntity(CashOut ent)
        {
            var ret = new SD_34
            {
                DOC_CODE = ent.DocCode,
                NUM_ORD = ent.NUM_ORD,
                DATE_ORD = ent.DATE_ORD,
                SUMM_ORD = ent.SUMM_ORD,
                DOCUM_ORD = ent.DOCUM_ORD,
                OSN_ORD = ent.OSN_ORD,
                NOTES_ORD = ent.NOTES_ORD,
                NAME_ORD = ent.NAME_ORD,
                CODE_CASS = ent.CODE_CASS,
                TABELNUMBER = ent.TABELNUMBER,
                OP_CODE = ent.OP_CODE,
                CA_DC = ent.CA_DC,
                KONTRAGENT_DC = ent.KONTRAGENT_DC,
                SHPZ_ORD = ent.SHPZ_ORD,
                SHPZ_DC = ent.SHPZ_DC,
                CASH_TO_DC = ent.CASH_TO_DC,
                SPOST_DC = ent.SPOST_DC,
                NCODE = ent.NCODE,
                KONTR_CRS_DC = ent.KONTR_CRS_DC ?? ent.CRS_DC,
                CRS_KOEF = ent.CRS_KOEF,
                CRS_SUMMA = ent.CRS_SUMMA,
                CHANGE_ORD = ent.CHANGE_ORD,
                CRS_DC = ent.CRS_DC,
                UCH_VALUTA_DC = ent.UCH_VALUTA_DC,
                SUMMA_V_UCH_VALUTE = ent.SUMMA_V_UCH_VALUTE,
                UCH_VALUTA_RATE = ent.UCH_VALUTA_RATE,
                SPOST_OPLACHENO = ent.SPOST_OPLACHENO,
                SPOST_CRS_DC = ent.SPOST_CRS_DC,
                SPOST_CRS_RATE = ent.SPOST_CRS_RATE,
                BANK_RASCH_SCHET_DC = ent.BANK_RASCH_SCHET_DC,
                CREATOR = ent.CREATOR,
                RUB_SUMMA = ent.RUB_SUMMA,
                RUB_RATE = ent.RUB_RATE,
                KONTR_CRS_SUM_CORRECT_PERCENT = ent.KONTR_CRS_SUM_CORRECT_PERCENT,
                OBRATNY_RASCHET = ent.OBRATNY_RASCHET,
                V_TOM_CHISLE = ent.V_TOM_CHISLE,
                PAY_ROLL_DC = ent.PAY_ROLL_DC,
                PAY_ROLL_ROW_CODE = ent.PAY_ROLL_ROW_CODE,
                KONTR_FROM_DC = ent.KONTR_FROM_DC,
                SFACT_FLAG = ent.SFACT_FLAG,
                TSTAMP = ent.TSTAMP,
                PLAT_VED_DC = ent.PLAT_VED_DC,
                PLAT_VED_ROW_CODE = ent.PLAT_VED_ROW_CODE
            };
            return ret;
        }

        public static CashOut LoadCashOut(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_34
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_221)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_33)
                        .Include(_ => _.SD_43)
                        .Include(_ => _.AccuredAmountOfSupplierRow)
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    return new CashOut(data)
                    {
                        myState = RowStatus.NotEdited
                    };
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            return null;
        }

        public static SD_251 CurrencyExchangeViewModelToEntity(CashCurrencyExchange ent)
        {
            var ret = new SD_251
            {
                DOC_CODE = ent.DOC_CODE,
                CH_NUM_ORD = ent.CH_NUM_ORD,
                CH_DATE = ent.CH_DATE,
                CH_KONTRAGENT_DC = ent.CH_KONTRAGENT_DC,
                TABELNUMBER = ent.TABELNUMBER,
                CH_NAME_ORD = ent.CH_NAME_ORD,
                CH_CASH_DC = ent.CH_CASH_DC,
                CH_CASH_DATE_OUT_DC = ent.CH_CASH_DATE_OUT_DC,
                CH_CASH_DATE_IN_DC = ent.CH_CASH_DATE_IN_DC,
                CH_CRS_OUT_DC = ent.CH_CRS_OUT_DC,
                CH_CRS_OUT_SUM = ent.CH_CRS_OUT_SUM,
                CH_CRS_IN_DC = ent.CH_CRS_IN_DC,
                CH_CRS_IN_SUM = ent.CH_CRS_IN_SUM,
                CH_NOTE = ent.CH_NOTE,
                CH_DATE_IN = ent.CH_DATE_IN,
                CH_DATE_OUT = ent.CH_DATE_OUT,
                CH_CROSS_RATE = ent.CH_CROSS_RATE,
                CH_DIRECTION = ent.CH_DIRECTION,
                CH_UCHET_VALUTA_DC = ent.CH_UCHET_VALUTA_DC,
                CH_IN_V_UCHET_VALUTE = ent.CH_IN_V_UCHET_VALUTE,
                CH_OUT_V_UCHET_VALUTE = ent.CH_OUT_V_UCHET_VALUTE,
                CH_IN_UCHET_VALUTA_RATE = ent.CH_IN_UCHET_VALUTA_RATE,
                CH_OUT_UCHET_VALUTA = ent.CH_OUT_UCHET_VALUTA,
                CREATOR = ent.CREATOR,
                CH_SHPZ_DC = ent.CH_SHPZ_DC
            };
            return ret;
        }

        public static CashCurrencyExchange LoadCurrencyExchange(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var doc = new CashCurrencyExchange(ctx.SD_251.FirstOrDefault(_ => _.DOC_CODE == dc));
                    var rateDate = ctx.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE <= doc.CH_DATE).Max(_ => _.RATE_DATE);
                    var rate = ctx.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE == rateDate);
                    doc.CrsInCBRate = doc.CH_CRS_IN_DC == GlobalOptions.SystemProfile.NationalCurrency.DocCode
                        ? 1
                        : rate.FirstOrDefault(_ => _.CRS_DC == doc.CH_CRS_IN_DC)?.RATE ?? 0;
                    doc.CrsOutCBRate = doc.CH_CRS_OUT_DC == GlobalOptions.SystemProfile.NationalCurrency.DocCode
                        ? 1
                        : rate.FirstOrDefault(_ => _.CRS_DC == doc.CH_CRS_OUT_DC)?.RATE ?? 0;
                    return doc;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return null;
        }

        public bool CheckRemains(Cash cash, DateTime date, Currency crs)
        {
            return false;
        }

        //public static bool RecalcCashRemains(ALFAMEDIAEntities ctx, Cash cash, DateTime date, decimal crsDC)
        //{
        //    if (cash == null || MainReferences.Currencies.Values.All(_ => _.DocCode != crsDC)) return false;

        //    decimal moneyTemp = 0;
        //    try
        //    {
        //        var sql =
        //            $"EXECUTE dbo.EXE_CASHEBOOK_RECALC '{CustomFormat.DateToString(date)}', '{CustomFormat.DecimalToSqlDecimal(cash.DocCode)}', '{CustomFormat.DecimalToSqlDecimal(crsDC)}'";
        //        ctx.Database.ExecuteSqlCommand(sql);
        //        //var codecass = ctx.SD_39.FirstOrDefault(_ =>
        //        //    _.CA_DC == cash.DocCode && _.CRS_DC == crsDC
        //        //                            && _.DATE_CASS == date);
        //        //if (codecass == null) CreateNewSD_39(cash.DocCode, date, ctx);
        //        //var minDate = ctx.SD_39.Where(_ =>
        //        //        _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_CASS < date)
        //        //    .Max(_ => _.DATE_CASS);
        //        //var first = ctx.SD_39.FirstOrDefault(_ =>
        //        //    _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_CASS == minDate);
        //        //if (first != null) moneyTemp = first.MONEY_STOP;
        //        //var rstart = ctx.TD_22.FirstOrDefault(_ =>
        //        //    _.DOC_CODE == cash.DocCode && _.CRS_DC == crsDC && _.DATE_START == date);
        //        //var datelist = ctx.SD_39.Where(_ => _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_CASS >= date)
        //        //    .Select(_ => _.DATE_CASS).ToList().Distinct();
        //        //var SumMove = new MoneySum
        //        //{
        //        //    In = rstart?.SUMMA_START ?? 0
        //        //};
        //        //foreach (var ddate in datelist)
        //        //{
        //        //    var upd = ctx.SD_39.Single(
        //        //        _ => _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_CASS == ddate);

        //        //    #region Поступления

        //        //    foreach (var cashIn in ctx.SD_33
        //        //        .Where(_ => _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_ORD == ddate).ToList())
        //        //    {
        //        //        SumMove.In += (decimal)cashIn.SUMM_ORD;
        //        //    }
        //        //    foreach (var cashExch in ctx.SD_251
        //        //        .Where(_ => _.CH_CASH_DC == cash.DocCode && _.CH_CRS_IN_DC == crsDC && _.CH_DATE_IN == ddate)
        //        //        .ToList())
        //        //    {
        //        //        SumMove.In += (decimal)cashExch.CH_CRS_IN_SUM;
        //        //    }

        //        //    #endregion

        //        //    #region Выплаты

        //        //    foreach (var cashOut in ctx.SD_34
        //        //        .Where(_ => _.CA_DC == cash.DocCode && _.CRS_DC == crsDC && _.DATE_ORD == ddate).ToList())
        //        //        SumMove.Out += (decimal)cashOut.SUMM_ORD;
        //        //    foreach (var cashExch in ctx.SD_251
        //        //        .Where(_ => _.CH_CASH_DC == cash.DocCode && _.CH_CRS_OUT_DC == crsDC && _.CH_DATE_OUT == ddate)
        //        //        .ToList())
        //        //        SumMove.Out += cashExch.CH_CRS_OUT_SUM;

        //        //    #endregion

        //        //    upd.MONEY_START = moneyTemp;
        //        //    upd.MONEY_STOP = moneyTemp + SumMove.In - SumMove.Out;
        //        //    moneyTemp = moneyTemp + SumMove.In - SumMove.Out;
        //        //    if (cash.CA_NEGATIVE_RESTS != 1 && moneyTemp < 0)
        //        //        return false;
        //        //    SumMove = new MoneySum();
        //        //}
        //        //ctx.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        WindowManager.ShowError(ex);
        //        return false;
        //    }
        //}

        /// <summary>
        ///     Загрузка приходных кассовых ордеров
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static List<CashBookDocument> LoadCashIn(Cash cash, DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            var ret = new List<CashBookDocument>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    IQueryable<SD_33> data;
                    if (searchText == null)
                        data = ctx.SD_33.Where(_ => _.CA_DC == cash.DocCode &&
                                                    _.DATE_ORD >= dateStart &&
                                                    _.DATE_ORD <= dateEnd);
                    else
                        data = ctx.SD_33.Where(_ => _.CA_DC == cash.DocCode)
                            .Include(_ => _.SD_43)
                            .Include(_ => _.SD_2)
                            .Include(_ => _.SD_114)
                            .Include(_ => _.SD_303)
                            .Where(_ => _.NUM_ORD.ToString().Contains(searchText)
                                        || _.CREATOR.Contains(searchText)
                                        || _.CRS_SUMMA.ToString().Contains(searchText)
                                        || _.NAME_ORD.Contains(searchText)
                                        || _.NOTES_ORD.Contains(searchText)
                                        || _.OSN_ORD.Contains(searchText)
                                        || _.SD_43.NAME.Contains(searchText)
                                        || _.SD_2.NAME.Contains(searchText)
                                        || _.SD_114.BA_ACC_SHORTNAME.Contains(searchText)
                                        || _.SD_303.SHPZ_NAME.Contains(searchText)
                                        || "Приходный ордер".Contains(searchText));
                    foreach (var d in data.OrderByDescending(_ => _.DATE_ORD).ToList())
                    {
                        var doc = new CashBookDocument
                        {
                            DocCode = d.DOC_CODE,
                            DocumnetTypeName = "Приходный ордер",
                            DocDate = (DateTime) d.DATE_ORD,
                            SummaIn = (decimal) d.SUMM_ORD,
                            SummaOut = 0,
                            CurrencyName = MainReferences.Currencies[(decimal) d.CRS_DC].Name,
                            DocNum = d.NUM_ORD.ToString(),
                            Note = d.NOTES_ORD,
                            IsSalary = d.NCODE == 100,
                            OsnOrd = d.OSN_ORD,
                            NameOrd = d.NAME_ORD
                        };
                        if (d.KONTRAGENT_DC != null)
                        {
                            doc.KontragnetTypeName = "Контрагент";
                            doc.KontragnetName = MainReferences.GetKontragent(d.KONTRAGENT_DC).Name;
                        }

                        if (d.TABELNUMBER != null)
                        {
                            doc.KontragnetTypeName = "Сотрудник";
                            doc.KontragnetName = MainReferences.Employees.Values
                                .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                        }

                        if (d.BANK_RASCH_SCHET_DC != null)
                        {
                            doc.KontragnetTypeName = "Банковский (расчетный) счет";
                            var bank = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == d.BANK_RASCH_SCHET_DC);
                            if (bank != null)
                                doc.KontragnetName = bank.BA_ACC_SHORTNAME + " - " + bank.BA_RASH_ACC;
                        }

                        if (d.RASH_ORDER_FROM_DC != null)
                        {
                            doc.KontragnetTypeName = "Расходный кассовый ордер";
                            var c = ctx.SD_34.FirstOrDefault(_ => _.DOC_CODE == d.RASH_ORDER_FROM_DC);
                            if (c != null) doc.KontragnetName = MainReferences.CashsAll[(decimal) c.CA_DC].Name;
                        }

                        ret.Add(doc);
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return null;
        }

        /// <summary>
        ///     Загрузка расходных кассовых ордеров
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static List<CashBookDocument> LoadCashOut(Cash cash, DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            var ret = new List<CashBookDocument>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    IQueryable<SD_34> data;
                    if (searchText == null)
                        data = ctx.SD_34.Where(_ => _.CA_DC == cash.DocCode &&
                                                    _.DATE_ORD >= dateStart &&
                                                    _.DATE_ORD <= dateEnd);
                    else
                        data = ctx.SD_34.Where(_ => _.CA_DC == cash.DocCode)
                            .Include(_ => _.SD_43)
                            .Include(_ => _.SD_2)
                            .Include(_ => _.SD_114)
                            .Include(_ => _.SD_303)
                            .Where(_ => _.NUM_ORD.ToString().Contains(searchText)
                                        || _.CREATOR.Contains(searchText)
                                        || _.CRS_SUMMA.ToString().Contains(searchText)
                                        || _.NAME_ORD.Contains(searchText)
                                        || _.NOTES_ORD.Contains(searchText)
                                        || _.OSN_ORD.Contains(searchText)
                                        || _.SD_43.NAME.Contains(searchText)
                                        || _.SD_2.NAME.Contains(searchText)
                                        || _.SD_114.BA_ACC_SHORTNAME.Contains(searchText)
                                        || _.SD_303.SHPZ_NAME.Contains(searchText)
                                        || "Расходный ордер".Contains(searchText));
                    foreach (var d in data.OrderByDescending(_ => _.DATE_ORD).ToList())
                    {
                        var doc = new CashBookDocument
                        {
                            DocCode = d.DOC_CODE,
                            DocumnetTypeName = "Расходный ордер",
                            DocDate = (DateTime) d.DATE_ORD,
                            SummaIn = 0,
                            SummaOut = (decimal) d.SUMM_ORD,
                            CurrencyName = MainReferences.Currencies[(decimal) d.CRS_DC].Name,
                            DocNum = d.NUM_ORD.ToString(),
                            Note = d.NOTES_ORD,
                            IsSalary = d.NCODE == 100,
                            OsnOrd = d.OSN_ORD,
                            NameOrd = d.NAME_ORD
                        };
                        if (d.KONTRAGENT_DC != null)
                        {
                            doc.KontragnetTypeName = "Контрагент";
                            doc.KontragnetName = MainReferences.GetKontragent(d.KONTRAGENT_DC).Name;
                        }

                        if (d.TABELNUMBER != null)
                        {
                            doc.KontragnetTypeName = "Сотрудник";
                            doc.KontragnetName = MainReferences.Employees.Values
                                .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                        }

                        if (d.BANK_RASCH_SCHET_DC != null)
                        {
                            doc.KontragnetTypeName = "Банковский (расчетный) счет";
                            var bank = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == d.BANK_RASCH_SCHET_DC);
                            if (bank != null)
                                doc.KontragnetName = bank.BA_ACC_SHORTNAME + " - " + bank.BA_RASH_ACC;
                        }

                        if (d.CASH_TO_DC != null)
                        {
                            doc.KontragnetTypeName = "Касса";
                            var prihOrder = ctx.SD_33.FirstOrDefault(_ => _.RASH_ORDER_FROM_DC == d.DOC_CODE);
                            if (prihOrder != null)
                                doc.KontragnetName = MainReferences.CashsAll[(decimal) prihOrder.CA_DC].Name;
                        }

                        ret.Add(doc);
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return null;
        }

        /// <summary>
        ///     Загрузка обмена валют
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static List<CashBookDocument> LoadCashCurrencyChange(Cash cash, DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            var ret = new List<CashBookDocument>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (searchText == null)
                    {
                        var dataIn = ctx.SD_251.Where(_ => _.CH_CASH_DC == cash.DocCode &&
                                                           _.CH_DATE_IN >= dateStart &&
                                                           _.CH_DATE_IN <= dateEnd).ToList();
                        var dataOut = ctx.SD_251.Where(_ => _.CH_CASH_DC == cash.DocCode &&
                                                            _.CH_DATE_OUT >= dateStart &&
                                                            _.CH_DATE_OUT <= dateEnd).ToList();
                        foreach (var d in dataIn.ToList())
                        {
                            var doc = new CashBookDocument
                            {
                                DocCode = d.DOC_CODE,
                                DocumnetTypeName = "Обмен валюты",
                                // ReSharper disable once PossibleInvalidOperationException
                                DocDate = (DateTime) d.CH_DATE_IN,
                                SummaIn = d.CH_CRS_IN_SUM ?? 0,
                                SummaOut = 0,
                                CurrencyName = MainReferences.Currencies[(decimal) d.CH_CRS_IN_DC].Name,
                                DocNum = d.CH_NUM_ORD.ToString(),
                                Note = d.CH_NOTE,
                                IsSalary = false
                            };
                            if (d.CH_KONTRAGENT_DC != null)
                            {
                                doc.KontragnetTypeName = "Контрагент";
                                doc.KontragnetName = MainReferences.GetKontragent(d.CH_KONTRAGENT_DC).Name;
                            }

                            if (d.TABELNUMBER != null)
                            {
                                doc.KontragnetTypeName = "Сотрудник";
                                doc.KontragnetName = MainReferences.Employees.Values
                                    .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                            }

                            ret.Add(doc);
                        }

                        foreach (var d in dataOut.ToList())
                        {
                            var doc = new CashBookDocument
                            {
                                DocCode = d.DOC_CODE,
                                DocumnetTypeName = "Обмен валюты",
                                DocDate = d.CH_DATE,
                                SummaIn = 0,
                                SummaOut = d.CH_CRS_OUT_SUM,
                                CurrencyName = MainReferences.Currencies[(decimal) d.CH_CRS_OUT_DC].Name,
                                DocNum = d.CH_NUM_ORD.ToString(),
                                Note = d.CH_NOTE,
                                IsSalary = false
                            };
                            if (d.CH_KONTRAGENT_DC != null)
                            {
                                doc.KontragnetTypeName = "Контрагент";
                                doc.KontragnetName = MainReferences.GetKontragent(d.CH_KONTRAGENT_DC).Name;
                            }

                            if (d.TABELNUMBER != null)
                            {
                                doc.KontragnetTypeName = "Сотрудник";
                                doc.KontragnetName = MainReferences.Employees.Values
                                    .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                            }

                            ret.Add(doc);
                        }
                    }
                    else
                    {
                        var data = ctx.SD_251.Where(_ => _.CH_CASH_DC == cash.DocCode)
                            .Include(_ => _.SD_303)
                            .Where(_ => _.CH_NUM_ORD.ToString().Contains(searchText)
                                        || _.CREATOR.Contains(searchText)
                                        || _.CH_NAME_ORD.Contains(searchText)
                                        || _.CH_NOTE.Contains(searchText)
                                        || _.SD_43.NAME.Contains(searchText)
                                        || _.SD_303.SHPZ_NAME.Contains(searchText)
                                        || "Обмен валюты".Contains(searchText)).ToList();
                        foreach (var d in data.ToList())
                        {
                            var doc = new CashBookDocument
                            {
                                DocCode = d.DOC_CODE,
                                DocumnetTypeName = "Обмен валюты",
                                DocDate = (DateTime) d.CH_DATE_IN,
                                SummaIn = d.CH_CRS_IN_SUM ?? 0,
                                SummaOut = 0,
                                CurrencyName = MainReferences.Currencies[(decimal) d.CH_CRS_IN_DC].Name,
                                DocNum = d.CH_NUM_ORD.ToString(),
                                Note = d.CH_NOTE,
                                IsSalary = false
                            };
                            if (d.CH_KONTRAGENT_DC != null)
                            {
                                doc.KontragnetTypeName = "Контрагент";
                                doc.KontragnetName = MainReferences.GetKontragent(d.CH_KONTRAGENT_DC).Name;
                            }

                            if (d.TABELNUMBER != null)
                            {
                                doc.KontragnetTypeName = "Сотрудник";
                                doc.KontragnetName = MainReferences.Employees.Values
                                    .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                            }

                            ret.Add(doc);
                            var doc1 = new CashBookDocument
                            {
                                DocCode = d.DOC_CODE,
                                DocumnetTypeName = "Обмен валюты",
                                DocDate = (DateTime) d.CH_DATE_OUT,
                                SummaIn = 0,
                                SummaOut = d.CH_CRS_OUT_SUM,
                                CurrencyName = MainReferences.Currencies[(decimal) d.CH_CRS_IN_DC].Name,
                                DocNum = d.CH_NUM_ORD.ToString(),
                                Note = d.CH_NOTE,
                                IsSalary = false
                            };
                            if (d.CH_KONTRAGENT_DC != null)
                            {
                                doc.KontragnetTypeName = "Контрагент";
                                doc.KontragnetName = MainReferences.GetKontragent(d.CH_KONTRAGENT_DC).Name;
                            }

                            if (d.TABELNUMBER != null)
                            {
                                doc.KontragnetTypeName = "Сотрудник";
                                doc.KontragnetName = MainReferences.Employees.Values
                                    .FirstOrDefault(_ => _.TabelNumber == d.TABELNUMBER)?.Name;
                            }

                            ret.Add(doc1);
                        }
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return null;
        }

        public static List<CashBookDocument> LoadDocuments(Cash cash, DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            var ret = new List<CashBookDocument>();
            if (cash == null) return null;
            ret.AddRange(LoadCashIn(cash, dateStart, dateEnd, searchText) ?? new List<CashBookDocument>());
            ret.AddRange(LoadCashOut(cash, dateStart, dateEnd, searchText) ?? new List<CashBookDocument>());
            ret.AddRange(LoadCashCurrencyChange(cash, dateStart, dateEnd, searchText) ?? new List<CashBookDocument>());
            return ret;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static List<MoneyRemains> GetRemains(Cash cash, DatePeriod period, List<CashBookDocument> documents)
        {
            var ret = new List<MoneyRemains>();
            try
            {
                if (period == null) return new List<MoneyRemains>();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var startDate = ctx.SD_39.FirstOrDefault(_ =>
                        _.CA_DC == cash.DocCode && _.DATE_CASS < period.DateStart)?.DATE_CASS;
                    if (startDate == null) return new List<MoneyRemains>();
                    var endDate = ctx.SD_39.Where(_ =>
                        _.CA_DC == cash.DocCode && _.DATE_CASS >= period.DateEnd).Max(_ => _.DATE_CASS);
                    var startrems = ctx.SD_39.Where(_ => _.CA_DC == cash.DocCode && _.DATE_CASS == startDate)
                        .ToList();
                    var endrems = ctx.SD_39.Where(_ => _.CA_DC == cash.DocCode && _.DATE_CASS == endDate)
                        .ToList();
                    foreach (var d in startrems)
                    {
                        var crsName = MainReferences.Currencies[(decimal) d.CRS_DC].Name;
                        ret.Add(new MoneyRemains
                        {
                            CurrencyName = crsName,
                            Start = d.MONEY_START,
                            End = d.MONEY_STOP,
                            In = documents.Where(_ => _.CurrencyName == crsName).Sum(_ => _.SummaIn),
                            Out = documents.Where(_ => _.CurrencyName == crsName).Sum(_ => _.SummaOut)
                        });
                    }

                    if (startDate != endDate)
                        foreach (var m in ret)
                        {
                            SD_39 first = null;
                            foreach (var d in endrems)
                            {
                                if (MainReferences.Currencies[(decimal) d.CRS_DC].Name != m.CurrencyName) continue;
                                first = d;
                                break;
                            }

                            m.End = first?.MONEY_STOP ?? 0;
                        }
                }

                return ret;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return new List<MoneyRemains>();
        }

        public static decimal GetCashCurrencyRemains(decimal cashDC, decimal crsDC, DateTime date)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var start = ctx.TD_22.FirstOrDefault(_ => _.DOC_CODE == cashDC && _.CRS_DC == crsDC);
                if (start == null || start.DATE_START > date) return 0;
                var cashIn = ctx.SD_33
                    .Where(_ => _.CA_DC == cashDC && _.CRS_DC == crsDC &&
                                _.DATE_ORD <= date).Sum(_ => _.SUMM_ORD);
                var cashOut = ctx.SD_34.Where(_ =>
                        _.CA_DC == cashDC && _.CRS_DC == crsDC && _.DATE_ORD <= date)
                    .Sum(_ => _.SUMM_ORD);
                var crsChangeIn = ctx.SD_251
                    .Where(_ => _.CH_CASH_DC == cashDC && _.CH_CRS_IN_DC == crsDC &&
                                _.CH_DATE <= date).Sum(_ => _.CH_CRS_IN_SUM);
                var crsChangeOutRow = ctx.SD_251
                    .Where(_ => _.CH_CASH_DC == cashDC && _.CH_CRS_OUT_DC == crsDC &&
                                _.CH_DATE <= date).ToList();
                var crsChangeOut = crsChangeOutRow.Sum(_ => _.CH_CRS_OUT_SUM);
                return start.SUMMA_START + (cashIn ?? 0) - (cashOut ?? 0) + (crsChangeIn ?? 0) - crsChangeOut;
            }
        }

        public static decimal GetCashCurrencyRemains(ALFAMEDIAEntities ctx, decimal cashDC, decimal crsDC,
            DateTime date)
        {
            var start = ctx.TD_22.FirstOrDefault(_ => _.DOC_CODE == cashDC && _.CRS_DC == crsDC);
            if (start == null || start.DATE_START > date) return 0;
            var cashIn = ctx.SD_33
                .Where(_ => _.CA_DC == cashDC && _.CRS_DC == crsDC &&
                            _.DATE_ORD <= date).Sum(_ => _.SUMM_ORD);
            var cashOut = ctx.SD_34.Where(_ =>
                    _.CA_DC == cashDC && _.CRS_DC == crsDC && _.DATE_ORD <= date)
                .Sum(_ => _.SUMM_ORD);
            var crsChangeIn = ctx.SD_251
                .Where(_ => _.CH_CASH_DC == cashDC && _.CH_CRS_IN_DC == crsDC &&
                            _.CH_DATE <= date).Sum(_ => _.CH_CRS_IN_SUM);
            var crsChangeOutRow = ctx.SD_251
                .Where(_ => _.CH_CASH_DC == cashDC && _.CH_CRS_OUT_DC == crsDC &&
                            _.CH_DATE <= date).ToList();
            var crsChangeOut = crsChangeOutRow.Sum(_ => _.CH_CRS_OUT_SUM);
            return start.SUMMA_START + (cashIn ?? 0) - (cashOut ?? 0) + (crsChangeIn ?? 0) - crsChangeOut;
        }

        public class MoneySum
        {
            public decimal In { set; get; }
            public decimal Out { set; get; }
        }
    }
}