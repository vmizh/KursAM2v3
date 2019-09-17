using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.Finance;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.Finance.Invoices;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.Managers.Invoices
{
   public class InvoicesManager
    {
        private static readonly WindowManager winManager = new WindowManager();
        /// <summary>
        ///     Оплата по счетам
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public class InvoicePayment
        {
            public decimal DocCode { set; get; }
            public decimal Summa { set; get; }
            public decimal PaySumma { set; get; }
        }

        #region Счет-фактуры поставщика

        /// <summary>
        ///     Загрузить счет-фактуру поставщика по DocCode
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static InvoiceProvider GetInvoiceProvider(decimal dc)
        {
            var doc = new InvoiceProvider();
            var pDocs = new List<InvoicePaymentDocument>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var i = ctx.SD_26
                        .Include(_ => _.TD_26)
                        .Include("TD_26.TD_24")
                        .Include(_ => _.SD_43)
                        .Include(_ => _.SD_179)
                        .Include(_ => _.SD_77)
                        .Include(_ => _.SD_189)
                        .Include(_ => _.SD_40)
                        .Include("TD_26.SD_83")
                        .Include("TD_26.SD_175")
                        .Include("TD_26.SD_43")
                        .Include("TD_26.SD_165")
                        .Include("TD_26.SD_175")
                        .Include("TD_26.SD_1751")
                        .Include("TD_26.SD_26")
                        .Include("TD_26.SD_261")
                        .Include("TD_26.SD_301")
                        .Include("TD_26.SD_303").FirstOrDefault(_ => _.DOC_CODE == dc);
                    foreach (var c in ctx.SD_34.Where(_ => _.SPOST_DC == dc).ToList())
                        pDocs.Add(new InvoicePaymentDocument
                        {
                            DocCode = c.DOC_CODE,
                            Code = 0,
                            DocumentType = DocumentType.CashOut,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocumentName =
                                $"{c.NUM_ORD} от {c.DATE_ORD} на {c.SUMM_ORD} {MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal) c.SUMM_ORD,
                            Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                            Note = c.NOTES_ORD
                        });
                    foreach (var c in ctx.TD_101.Include(_ => _.SD_101).Where(_ => _.VVT_SFACT_POSTAV_DC == dc)
                        .ToList())
                        pDocs.Add(new InvoicePaymentDocument
                        {
                            DocCode = c.DOC_CODE,
                            Code = c.CODE,
                            DocumentType = DocumentType.Bank,
                            DocumentName =
                                // ReSharper disable once PossibleInvalidOperationException
                                $"{c.SD_101.VV_START_DATE} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal) c.VVT_VAL_RASHOD,
                            Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                            Note = c.VVT_DOC_NUM
                        });
                    foreach (var c in ctx.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SPOST_DC == dc).ToList())
                        pDocs.Add(new InvoicePaymentDocument
                        {
                            DocCode = c.DOC_CODE,
                            Code = c.CODE,
                            DocumentType = DocumentType.MutualAccounting,
                            DocumentName =
                                // ReSharper disable once PossibleInvalidOperationException
                                $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE} на {c.VZT_CRS_SUMMA}",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal) c.VZT_CRS_SUMMA,
                            Currency = MainReferences.Currencies[c.SD_110.CurrencyToDC],
                            Note = c.VZT_DOC_NOTES
                        });
                    decimal sum = 0;
                    if (i?.SD_24 != null)
                        sum += (from q in i.TD_26 from d in q.TD_24 select d.DDT_KOL_PRIHOD * q.SFT_ED_CENA ?? 0).Sum();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    doc = new InvoiceProvider(i)
                    {
                        SummaFact = sum,
                        myState = RowStatus.NotEdited
                    };
                    foreach (var p in pDocs) doc.PaymentDocs.Add(p);
                    foreach (var row in doc.Rows) row.myState = RowStatus.NotEdited;
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            return doc;
        }

        public static InvoiceProvider NewProvider()
        {
            var ret = new InvoiceProvider(null)
            {
                DocCode = -1,
                SF_POSTAV_DATE = DateTime.Today,
                SF_REGISTR_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                Currency = GlobalOptions.SystemProfile.MainCurrency,
                SF_POSTAV_NUM = null,
                myState = RowStatus.NewRow,
                Rows = new ObservableCollection<InvoiceProviderRow>()
            };
            return ret;
        }

        public static InvoiceProvider NewProviderCopy(InvoiceProvider doc)
        {
            var ret = new InvoiceProvider(doc?.Entity)
            {
                DocCode = -1,
                SF_POSTAV_NUM = null,
                SF_POSTAV_DATE = DateTime.Today,
                SF_REGISTR_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                myState = RowStatus.NewRow
            };
            var code = 1;
            foreach (var row in ret.Rows)
            {
                row.DocCode = -1;
                row.Code = code;
                row.myState = RowStatus.NewRow;
                code++;
            }

            ret.DeletedRows.Clear();
            ret.PaymentDocs.Clear();
            ret.Facts.Clear();
            return ret;
        }

        public static InvoiceProvider NewProviderCopy(decimal dc)
        {
            var newCopy = GetInvoiceProvider(dc);
            if (newCopy == null) return null;
            newCopy.DocCode = -1;
            newCopy.SF_POSTAV_NUM = null;
            newCopy.SF_POSTAV_DATE = DateTime.Today;
            newCopy.SF_REGISTR_DATE = DateTime.Today;
            newCopy.CREATOR = GlobalOptions.UserInfo.Name;
            newCopy.myState = RowStatus.NewRow;
            newCopy.PaymentDocs.Clear();
            newCopy.Facts.Clear();
            newCopy.IsAccepted = false;
            var code = 1;
            foreach (var row in newCopy.Rows)
            {
                row.DocCode = -1;
                row.Code = code;
                row.myState = RowStatus.NewRow;
                code++;
            }

            newCopy.DeletedRows.Clear();
            newCopy.PaymentDocs.Clear();
            newCopy.Facts.Clear();
            return newCopy;
        }

        public static InvoiceProvider NewProviderRequisite(decimal dc)
        {
            var ret = NewProviderCopy(dc);
            if (ret == null) return null;
            ret.PaymentDocs.Clear();
            ret.Facts.Clear();
            ret.Rows.Clear();
            return ret;
        }

        public static decimal SaveProvider(InvoiceProvider doc, ProviderSearchView searchWindow = null)
        {
            var newDC = doc.DocCode;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var d in doc.DeletedRows)
                        {
                            var oldrow = ctx.TD_26.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode && _.CODE == d.Code);
                            if (oldrow == null) continue;
                            ctx.TD_26.Remove(oldrow);
                        }

                        if (doc.DocCode == -1)
                        {
                            var guidId = Guid.NewGuid();
                            var inNum = ctx.SD_26.Any() ? ctx.SD_26.Max(_ => _.SF_IN_NUM) + 1 : 1;
                            newDC = ctx.SD_26.Any() ? ctx.SD_26.Max(_ => _.DOC_CODE) + 1 : 10260000001;
                            ctx.SD_26.Add(new SD_26
                            {
                                DOC_CODE = newDC,
                                SF_OPRIHOD_DATE = doc.SF_OPRIHOD_DATE,
                                SF_POSTAV_NUM = doc.SF_POSTAV_NUM,
                                SF_POSTAV_DATE = doc.SF_POSTAV_DATE,
                                SF_POST_DC = doc.SF_POST_DC,
                                SF_RUB_SUMMA = doc.SF_RUB_SUMMA,
                                SF_CRS_SUMMA = doc.SF_CRS_SUMMA,
                                SF_CRS_DC = doc.SF_CRS_DC,
                                SF_CRS_RATE = doc.SF_CRS_RATE,
                                V_CRS_ADD_PERCENT = doc.V_CRS_ADD_PERCENT,
                                SF_PAY_FLAG = doc.SF_PAY_FLAG,
                                SF_FACT_SUMMA = doc.SF_FACT_SUMMA,
                                SF_OLE = doc.SF_OLE,
                                SF_PAY_COND_DC = doc.SF_PAY_COND_DC,
                                TABELNUMBER = doc.TABELNUMBER,
                                SF_EXECUTED = doc.SF_EXECUTED,
                                SF_GRUZOOTPRAVITEL = doc.SF_GRUZOOTPRAVITEL,
                                SF_GRUZOPOLUCHATEL = doc.SF_GRUZOPOLUCHATEL,
                                SF_NAKLAD_METHOD = doc.SF_NAKLAD_METHOD,
                                SF_ACCEPTED = doc.SF_ACCEPTED,
                                SF_SUMMA_S_NDS = doc.SF_SUMMA_S_NDS,
                                SF_REGISTR_DATE = doc.SF_POSTAV_DATE,
                                SF_SCHET_FLAG = doc.SF_SCHET_FLAG,
                                SF_SCHET_FACT_FLAG = doc.SF_SCHET_FACT_FLAG,
                                SF_NOTES = doc.SF_NOTES,
                                CREATOR = doc.CREATOR,
                                SF_VZAIMOR_TYPE_DC = doc.SF_VZAIMOR_TYPE_DC,
                                SF_DOGOVOR_POKUPKI_DC = doc.SF_DOGOVOR_POKUPKI_DC,
                                SF_PREDOPL_DOC_DC = doc.SF_PREDOPL_DOC_DC,
                                SF_IN_NUM = inNum,
                                SF_FORM_RASCH_DC = doc.SF_FORM_RASCH_DC,
                                SF_VIPOL_RABOT_DATE = doc.SF_VIPOL_RABOT_DATE,
                                SF_TRANZIT = doc.SF_TRANZIT,
                                SF_KONTR_CRS_DC = doc.SF_CRS_DC,
                                SF_KONTR_CRS_RATE = 1,
                                SF_KONTR_CRS_SUMMA = doc.SF_CRS_SUMMA,
                                SF_UCHET_VALUTA_DC = doc.SF_UCHET_VALUTA_DC,
                                SF_UCHET_VALUTA_RATE = doc.SF_UCHET_VALUTA_RATE,
                                SF_SUMMA_V_UCHET_VALUTE = doc.SF_SUMMA_V_UCHET_VALUTE,
                                SF_NDS_VKL_V_CENU = doc.SF_NDS_VKL_V_CENU,
                                SF_SFACT_DC = doc.SF_SFACT_DC,
                                SF_CENTR_OTV_DC = doc.SF_CENTR_OTV_DC,
                                SF_POLUCH_KONTR_DC = doc.SF_POLUCH_KONTR_DC,
                                SF_PEREVOZCHIK_DC = doc.SF_PEREVOZCHIK_DC,
                                SF_PEREVOZCHIK_SUM = doc.SF_PEREVOZCHIK_SUM,
                                SF_AUTO_CREATE = doc.SF_AUTO_CREATE,
                                Id = guidId
                            });
                            if (doc.Rows.Count > 0)
                            {
                                var code = 1;
                                foreach (var items in doc.Rows)
                                {
                                    ctx.TD_26.Add(new TD_26
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        SFT_TEXT = items.SFT_TEXT ?? " ",
                                        SFT_POST_ED_IZM_DC = items.SFT_POST_ED_IZM_DC,
                                        SFT_POST_ED_CENA = items.SFT_POST_ED_CENA,
                                        SFT_POST_KOL = items.SFT_POST_KOL,
                                        SFT_NEMENKL_DC = items.SFT_NEMENKL_DC,
                                        SFT_UCHET_ED_IZM_DC = items.SFT_UCHET_ED_IZM_DC,
                                        SFT_ED_CENA = items.SFT_ED_CENA,
                                        SFT_KOL = items.SFT_KOL,
                                        SFT_SUMMA_CBOROV = items.SFT_SUMMA_CBOROV,
                                        SFT_NDS_PERCENT = items.SFT_NDS_PERCENT,
                                        SFT_SUMMA_NAKLAD = items.SFT_SUMMA_NAKLAD,
                                        SFT_SUMMA_NDS = items.SFT_SUMMA_NDS,
                                        SFT_SUMMA_K_OPLATE = items.SFT_SUMMA_K_OPLATE,
                                        SFT_ED_CENA_PRIHOD = items.SFT_ED_CENA_PRIHOD,
                                        SFT_IS_NAKLAD = items.SFT_IS_NAKLAD,
                                        SFT_VKLUCH_V_CENU = items.SFT_VKLUCH_V_CENU,
                                        SFT_AUTO_FLAG = items.SFT_AUTO_FLAG,
                                        SFT_STDP_DC = items.SFT_STDP_DC,
                                        SFT_NOM_CRS_DC = items.SFT_NOM_CRS_DC,
                                        SFT_NOM_CRS_RATE = items.SFT_NOM_CRS_RATE,
                                        SFT_NOM_CRS_CENA = items.SFT_NOM_CRS_CENA,
                                        SFT_CENA_V_UCHET_VALUTE = items.SFT_CENA_V_UCHET_VALUTE,
                                        SFT_SUMMA_V_UCHET_VALUTE = items.SFT_SUMMA_V_UCHET_VALUTE,
                                        SFT_DOG_POKUP_DC = items.SFT_DOG_POKUP_DC,
                                        SFT_DOG_POKUP_PLAN_ROW_CODE = items.SFT_DOG_POKUP_PLAN_ROW_CODE,
                                        SFT_SUMMA_K_OPLATE_KONTR_CRS = items.SFT_SUMMA_K_OPLATE,
                                        SFT_SHPZ_DC = items.SFT_SHPZ_DC,
                                        SFT_STRANA_PROIS = items.SFT_STRANA_PROIS,
                                        SFT_N_GRUZ_DECLAR = items.SFT_N_GRUZ_DECLAR,
                                        SFT_PEREVOZCHIK_POZITION = items.SFT_PEREVOZCHIK_POZITION,
                                        SFT_NAKLAD_KONTR_DC = items.SFT_NAKLAD_KONTR_DC,
                                        SFT_SALE_PRICE_IN_UCH_VAL = items.SFT_SALE_PRICE_IN_UCH_VAL,
                                        SFT_PERCENT = items.SFT_PERCENT,
                                        Id = Guid.NewGuid(),
                                        DocId = guidId,
                                        SchetRowNakladRashodId = items.SchetRowNakladRashodId,
                                        SchetRowNakladSumma = items.SchetRowNakladSumma,
                                        SchetRowNakladRate = items.SchetRowNakladRate
                                    });
                                    code++;
                                }
                            }
                        }
                        else
                        {
                            var old = ctx.SD_26.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            if (old == null) return doc.DocCode;
                            old.SF_OPRIHOD_DATE = doc.SF_OPRIHOD_DATE;
                            old.SF_POSTAV_NUM = doc.SF_POSTAV_NUM;
                            old.SF_POSTAV_DATE = doc.SF_POSTAV_DATE;
                            old.SF_POST_DC = doc.SF_POST_DC;
                            old.SF_RUB_SUMMA = doc.SF_RUB_SUMMA;
                            old.SF_CRS_SUMMA = doc.SF_CRS_SUMMA;
                            old.SF_CRS_DC = doc.SF_CRS_DC;
                            old.SF_CRS_RATE = doc.SF_CRS_RATE;
                            old.V_CRS_ADD_PERCENT = doc.V_CRS_ADD_PERCENT;
                            old.SF_PAY_FLAG = doc.SF_PAY_FLAG;
                            old.SF_FACT_SUMMA = doc.SF_FACT_SUMMA;
                            old.SF_OLE = doc.SF_OLE;
                            old.SF_PAY_COND_DC = doc.SF_PAY_COND_DC;
                            old.TABELNUMBER = doc.TABELNUMBER;
                            old.SF_EXECUTED = doc.SF_EXECUTED;
                            old.SF_GRUZOOTPRAVITEL = doc.SF_GRUZOOTPRAVITEL;
                            old.SF_GRUZOPOLUCHATEL = doc.SF_GRUZOPOLUCHATEL;
                            old.SF_NAKLAD_METHOD = doc.SF_NAKLAD_METHOD;
                            old.SF_ACCEPTED = doc.SF_ACCEPTED;
                            old.SF_SUMMA_S_NDS = doc.SF_SUMMA_S_NDS;
                            old.SF_REGISTR_DATE = doc.SF_POSTAV_DATE;
                            old.SF_SCHET_FLAG = doc.SF_SCHET_FLAG;
                            old.SF_SCHET_FACT_FLAG = doc.SF_SCHET_FACT_FLAG;
                            old.SF_NOTES = doc.SF_NOTES;
                            old.CREATOR = doc.CREATOR;
                            old.SF_VZAIMOR_TYPE_DC = doc.SF_VZAIMOR_TYPE_DC;
                            old.SF_DOGOVOR_POKUPKI_DC = doc.SF_DOGOVOR_POKUPKI_DC;
                            old.SF_PREDOPL_DOC_DC = doc.SF_PREDOPL_DOC_DC;
                            old.SF_IN_NUM = doc.SF_IN_NUM;
                            old.SF_FORM_RASCH_DC = doc.SF_FORM_RASCH_DC;
                            old.SF_VIPOL_RABOT_DATE = doc.SF_VIPOL_RABOT_DATE;
                            old.SF_TRANZIT = doc.SF_TRANZIT;
                            old.SF_KONTR_CRS_DC = doc.SF_KONTR_CRS_DC;
                            old.SF_KONTR_CRS_RATE = 1;
                            old.SF_KONTR_CRS_SUMMA = doc.SF_CRS_SUMMA;
                            old.SF_UCHET_VALUTA_DC = doc.SF_UCHET_VALUTA_DC;
                            old.SF_UCHET_VALUTA_RATE = doc.SF_UCHET_VALUTA_RATE;
                            old.SF_SUMMA_V_UCHET_VALUTE = doc.SF_SUMMA_V_UCHET_VALUTE;
                            old.SF_NDS_VKL_V_CENU = doc.SF_NDS_VKL_V_CENU;
                            old.SF_SFACT_DC = doc.SF_SFACT_DC;
                            old.SF_CENTR_OTV_DC = doc.SF_CENTR_OTV_DC;
                            old.SF_POLUCH_KONTR_DC = doc.SF_POLUCH_KONTR_DC;
                            old.SF_PEREVOZCHIK_DC = doc.SF_PEREVOZCHIK_DC;
                            old.SF_PEREVOZCHIK_SUM = doc.SF_PEREVOZCHIK_SUM;
                            old.SF_AUTO_CREATE = doc.SF_AUTO_CREATE;
                            foreach (var r in doc.Rows)
                            {
                                var oldRow = ctx.TD_26.FirstOrDefault(_ =>
                                    _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (oldRow == null)
                                {
                                    ctx.TD_26.Add(new TD_26
                                    {
                                        DOC_CODE = r.DocCode,
                                        CODE = r.Code,
                                        SFT_TEXT = r.SFT_TEXT ?? " ",
                                        SFT_POST_ED_IZM_DC = r.SFT_POST_ED_IZM_DC,
                                        SFT_POST_ED_CENA = r.SFT_POST_ED_CENA,
                                        SFT_POST_KOL = r.SFT_POST_KOL,
                                        SFT_NEMENKL_DC = r.SFT_NEMENKL_DC,
                                        SFT_UCHET_ED_IZM_DC = r.SFT_UCHET_ED_IZM_DC,
                                        SFT_ED_CENA = r.SFT_ED_CENA,
                                        SFT_KOL = r.SFT_KOL,
                                        SFT_SUMMA_CBOROV = r.SFT_SUMMA_CBOROV,
                                        SFT_NDS_PERCENT = r.SFT_NDS_PERCENT,
                                        SFT_SUMMA_NAKLAD = r.SFT_SUMMA_NAKLAD,
                                        SFT_SUMMA_NDS = r.SFT_SUMMA_NDS,
                                        SFT_SUMMA_K_OPLATE = r.SFT_SUMMA_K_OPLATE,
                                        SFT_ED_CENA_PRIHOD = r.SFT_ED_CENA_PRIHOD,
                                        SFT_IS_NAKLAD = r.SFT_IS_NAKLAD,
                                        SFT_VKLUCH_V_CENU = r.SFT_VKLUCH_V_CENU,
                                        SFT_AUTO_FLAG = r.SFT_AUTO_FLAG,
                                        SFT_STDP_DC = r.SFT_STDP_DC,
                                        SFT_NOM_CRS_DC = r.SFT_NOM_CRS_DC,
                                        SFT_NOM_CRS_RATE = r.SFT_NOM_CRS_RATE,
                                        SFT_NOM_CRS_CENA = r.SFT_NOM_CRS_CENA,
                                        SFT_CENA_V_UCHET_VALUTE = r.SFT_CENA_V_UCHET_VALUTE,
                                        SFT_SUMMA_V_UCHET_VALUTE = r.SFT_SUMMA_V_UCHET_VALUTE,
                                        SFT_DOG_POKUP_DC = r.SFT_DOG_POKUP_DC,
                                        SFT_DOG_POKUP_PLAN_ROW_CODE = r.SFT_DOG_POKUP_PLAN_ROW_CODE,
                                        SFT_SUMMA_K_OPLATE_KONTR_CRS = r.SFT_SUMMA_K_OPLATE,
                                        SFT_SHPZ_DC = r.SFT_SHPZ_DC,
                                        SFT_STRANA_PROIS = r.SFT_STRANA_PROIS,
                                        SFT_N_GRUZ_DECLAR = r.SFT_N_GRUZ_DECLAR,
                                        SFT_PEREVOZCHIK_POZITION = r.SFT_PEREVOZCHIK_POZITION,
                                        SFT_NAKLAD_KONTR_DC = r.SFT_NAKLAD_KONTR_DC,
                                        SFT_SALE_PRICE_IN_UCH_VAL = r.SFT_SALE_PRICE_IN_UCH_VAL,
                                        SFT_PERCENT = r.SFT_PERCENT,
                                        Id = Guid.NewGuid(),
                                        DocId = old.Id,
                                        SchetRowNakladRashodId = r.SchetRowNakladRashodId,
                                        SchetRowNakladSumma = r.SchetRowNakladSumma,
                                        SchetRowNakladRate = r.SchetRowNakladRate
                                    });
                                }
                                else
                                {
                                    oldRow.SFT_TEXT = r.SFT_TEXT ?? " ";
                                    oldRow.SFT_POST_ED_IZM_DC = r.SFT_POST_ED_IZM_DC;
                                    oldRow.SFT_POST_ED_CENA = r.SFT_POST_ED_CENA;
                                    oldRow.SFT_POST_KOL = r.SFT_POST_KOL;
                                    oldRow.SFT_NEMENKL_DC = r.SFT_NEMENKL_DC;
                                    oldRow.SFT_UCHET_ED_IZM_DC = r.SFT_UCHET_ED_IZM_DC;
                                    oldRow.SFT_ED_CENA = r.SFT_ED_CENA;
                                    oldRow.SFT_KOL = r.SFT_KOL;
                                    oldRow.SFT_SUMMA_CBOROV = r.SFT_SUMMA_CBOROV;
                                    oldRow.SFT_NDS_PERCENT = r.SFT_NDS_PERCENT;
                                    oldRow.SFT_SUMMA_NAKLAD = r.SFT_SUMMA_NAKLAD;
                                    oldRow.SFT_SUMMA_NDS = r.SFT_SUMMA_NDS;
                                    oldRow.SFT_SUMMA_K_OPLATE = r.SFT_SUMMA_K_OPLATE;
                                    oldRow.SFT_ED_CENA_PRIHOD = r.SFT_ED_CENA_PRIHOD;
                                    oldRow.SFT_IS_NAKLAD = r.SFT_IS_NAKLAD;
                                    oldRow.SFT_VKLUCH_V_CENU = r.SFT_VKLUCH_V_CENU;
                                    oldRow.SFT_AUTO_FLAG = r.SFT_AUTO_FLAG;
                                    oldRow.SFT_STDP_DC = r.SFT_STDP_DC;
                                    oldRow.SFT_NOM_CRS_DC = r.SFT_NOM_CRS_DC;
                                    oldRow.SFT_NOM_CRS_RATE = r.SFT_NOM_CRS_RATE;
                                    oldRow.SFT_NOM_CRS_CENA = r.SFT_NOM_CRS_CENA;
                                    oldRow.SFT_CENA_V_UCHET_VALUTE = r.SFT_CENA_V_UCHET_VALUTE;
                                    oldRow.SFT_SUMMA_V_UCHET_VALUTE = r.SFT_SUMMA_V_UCHET_VALUTE;
                                    oldRow.SFT_DOG_POKUP_DC = r.SFT_DOG_POKUP_DC;
                                    oldRow.SFT_DOG_POKUP_PLAN_ROW_CODE = r.SFT_DOG_POKUP_PLAN_ROW_CODE;
                                    oldRow.SFT_SUMMA_K_OPLATE_KONTR_CRS = r.SFT_SUMMA_K_OPLATE;
                                    oldRow.SFT_SHPZ_DC = r.SFT_SHPZ_DC;
                                    oldRow.SFT_STRANA_PROIS = r.SFT_STRANA_PROIS;
                                    oldRow.SFT_N_GRUZ_DECLAR = r.SFT_N_GRUZ_DECLAR;
                                    oldRow.SFT_PEREVOZCHIK_POZITION = r.SFT_PEREVOZCHIK_POZITION;
                                    oldRow.SFT_NAKLAD_KONTR_DC = r.SFT_NAKLAD_KONTR_DC;
                                    oldRow.SFT_SALE_PRICE_IN_UCH_VAL = r.SFT_SALE_PRICE_IN_UCH_VAL;
                                    oldRow.SFT_PERCENT = r.SFT_PERCENT;
                                    oldRow.SchetRowNakladRashodId = r.SchetRowNakladRashodId;
                                    oldRow.SchetRowNakladSumma = r.SchetRowNakladSumma;
                                    oldRow.SchetRowNakladRate = r.SchetRowNakladRate;
                                }
                            }
                        }

                        ctx.SaveChanges();
                        transaction.Commit();
                        RecalcKontragentBalans.CalcBalans(doc.SF_POST_DC, doc.SF_POSTAV_DATE);
                        foreach (var r in doc.Rows)
                        {
                            r.myState = RowStatus.NotEdited;
                            r.RaisePropertyChanged("State");
                        }
                        doc.myState = RowStatus.NotEdited;
                        doc.RaisePropertyChanged("State");
                        doc.DeletedRows.Clear();
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                        return -1;
                    }
                }
            }

            return newDC;
        }

        public static void DeleteProvider(decimal dc, ProviderSearchView searchWindow = null)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var doc = ctx.SD_26.Include(_ => _.TD_26).FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (doc == null) return;
                        ctx.SD_26.Remove(doc);
                        ctx.SaveChanges();
                        transaction.Commit();
                        RecalcKontragentBalans.CalcBalans(doc.SF_POST_DC, doc.SF_POSTAV_DATE);
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        /// <summary>
        ///     Список без учета дат
        /// </summary>
        /// <param name="isUsePayment"></param>
        /// <param name="isAccepted"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static List<InvoiceProvider> GetInvoicesProvider(bool isUsePayment, bool isAccepted,
            string searchText = null)
        {
            return GetInvoicesProvider(new DateTime(1900, 1, 1), new DateTime(2100, 1, 1), isUsePayment, searchText,
                isAccepted);
        }

        /// <summary>
        ///     Список с диапазаном дат
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <param name="isUsePayment"></param>
        /// <param name="searchText"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        public static List<InvoiceProvider> GetInvoicesProvider(DateTime dateStart, DateTime dateEnd, bool isUsePayment,
            string searchText = null, bool isAccepted = false)
        {
            var ret = new List<InvoiceProvider>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    List<SD_26> data;
                    if (string.IsNullOrEmpty(searchText))
                    {
                        if (isAccepted)
                            data = ctx.SD_26
                                .Include(_ => _.TD_26)
                                .Include("TD_26.TD_24")
                                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                                          && _.SF_ACCEPTED == 1).ToList();
                        else
                            data = ctx.SD_26
                                .Include(_ => _.TD_26)
                                .Include("TD_26.TD_24")
                                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd).ToList();
                    }
                    else
                    {
                        if (isAccepted)
                            data = (from sd26 in ctx.SD_26
                                    .Include(_ => _.TD_26)
                                    .Include("TD_26.TD_24")
                                    .Include(_ => _.SD_43)
                                    .Include(_ => _.SD_179)
                                    .Include(_ => _.SD_77)
                                    .Include(_ => _.SD_189)
                                    .Include(_ => _.SD_40)
                                    .AsNoTracking()
                                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                                              && _.SF_ACCEPTED == 1)
                                join td26 in ctx.TD_26
                                        .Include(_ => _.SD_83)
                                        .Include(_ => _.SD_175)
                                        .Include(_ => _.SD_43)
                                    on sd26.DOC_CODE equals td26.DOC_CODE
                                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                                where sd26.SF_CRS_SUMMA.ToString().Contains(searchText)
                                      || sd26.SF_CRS_RATE.ToString()
                                          .Contains(searchText)
                                      || sd26.SF_NOTES.Contains(searchText)
                                      || sd26.SF_IN_NUM.ToString().Contains(searchText)
                                      || sd26.SF_POSTAV_NUM.Contains(searchText)
                                      // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                                      || sd26.SF_POSTAV_DATE.ToString()
                                          .Contains(searchText)
                                      || sd26.SF_OPRIHOD_DATE.ToString()
                                          .Contains(searchText)
                                      || sd26.CREATOR.Contains(searchText)
                                      || sd26.SD_43.NAME.Contains(searchText)
                                      || sd26.SD_179.PT_NAME.Contains(searchText)
                                      || sd26.SD_77.TV_NAME.Contains(searchText)
                                      || sd26.SD_189.OOT_NAME.Contains(searchText)
                                      || sd26.SD_40.CENT_NAME.Contains(searchText)
                                      || td26.SFT_TEXT.Contains(searchText)
                                      || td26.SFT_ED_CENA.ToString().Contains(searchText)
                                      || td26.SFT_KOL.ToString().Contains(searchText)
                                      || td26.SFT_STRANA_PROIS.Contains(searchText)
                                      || td26.SFT_N_GRUZ_DECLAR.Contains(searchText)
                                      || td26.SD_83.NOM_NOMENKL.Contains(searchText)
                                      || td26.SD_83.NOM_NAME.Contains(searchText)
                                      || td26.SD_83.NOM_NOTES.Contains(searchText)
                                      || td26.SD_175.ED_IZM_NAME.Contains(searchText)
                                      || td26.SD_43.NAME.Contains(searchText)
                                select sd26).ToList();
                        else
                            data = (from sd26 in ctx.SD_26
                                    .Include(_ => _.TD_26)
                                    .Include("TD_26.TD_24")
                                    .Include(_ => _.SD_43)
                                    .Include(_ => _.SD_179)
                                    .Include(_ => _.SD_77)
                                    .Include(_ => _.SD_189)
                                    .Include(_ => _.SD_40)
                                    .AsNoTracking()
                                    .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd)
                                join td26 in ctx.TD_26
                                        .Include(_ => _.SD_83)
                                        .Include(_ => _.SD_175)
                                        .Include(_ => _.SD_43)
                                    on sd26.DOC_CODE equals td26.DOC_CODE
                                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                                where sd26.SF_CRS_SUMMA.ToString().Contains(searchText)
                                      || sd26.SF_CRS_RATE.ToString()
                                          .Contains(searchText)
                                      || sd26.SF_NOTES.Contains(searchText)
                                      || sd26.SF_IN_NUM.ToString().Contains(searchText)
                                      || sd26.SF_POSTAV_NUM.Contains(searchText)
                                      // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                                      || sd26.SF_POSTAV_DATE.ToString()
                                          .Contains(searchText)
                                      || sd26.SF_OPRIHOD_DATE.ToString()
                                          .Contains(searchText)
                                      || sd26.CREATOR.Contains(searchText)
                                      || sd26.SD_43.NAME.Contains(searchText)
                                      || sd26.SD_179.PT_NAME.Contains(searchText)
                                      || sd26.SD_77.TV_NAME.Contains(searchText)
                                      || sd26.SD_189.OOT_NAME.Contains(searchText)
                                      || sd26.SD_40.CENT_NAME.Contains(searchText)
                                      || td26.SFT_TEXT.Contains(searchText)
                                      || td26.SFT_ED_CENA.ToString().Contains(searchText)
                                      || td26.SFT_KOL.ToString().Contains(searchText)
                                      || td26.SFT_STRANA_PROIS.Contains(searchText)
                                      || td26.SFT_N_GRUZ_DECLAR.Contains(searchText)
                                      || td26.SD_83.NOM_NOMENKL.Contains(searchText)
                                      || td26.SD_83.NOM_NAME.Contains(searchText)
                                      || td26.SD_83.NOM_NOTES.Contains(searchText)
                                      || td26.SD_175.ED_IZM_NAME.Contains(searchText)
                                      || td26.SD_43.NAME.Contains(searchText)
                                select sd26).ToList();
                    }

                    var sql =
                        "SELECT s26.doc_code as DocCode, s26.SF_CRS_SUMMA as Summa, SUM(ISNULL(s34.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_RASHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                        "FROM sd_26 s26 " +
                        "LEFT OUTER JOIN sd_34 s34 ON s34.SPOST_DC = s26.DOC_CODE " +
                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_POSTAV_DC = s26.DOC_CODE " +
                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SPOST_DC = s26.DOC_CODE " +
                        $"WHERE s26.SF_POSTAV_DATE >= '{CustomFormat.DateToString(dateStart)}' AND s26.SF_POSTAV_DATE <= '{CustomFormat.DateToString(dateEnd)}'" +
                        "GROUP BY s26.doc_code, s26.SF_CRS_SUMMA ";
                    var pays = ctx.Database.SqlQuery<InvoicePayment>(sql).ToList();
                    foreach (var d in data.OrderByDescending(_ => _.SF_POSTAV_DATE))
                    {
                        var newDoc = new InvoiceProvider(d);
                        if (isUsePayment)
                        {
                            var pd = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode);
                            if (pd == null)
                            {
                                ret.Add(newDoc);
                                continue;
                            }

                            if (newDoc.SF_CRS_SUMMA > pd.PaySumma)
                            {
                                newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                                {
                                    Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                                });
                                ret.Add(newDoc);
                            }
                        }
                        else
                        {
                            newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                            {
                                Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                            });
                            ret.Add(newDoc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return ret;
        }

        public static List<InvoiceProvider> GetInvoicesProvider(DateTime dateStart, DateTime dateEnd, bool isUsePayment,
            decimal kontragentDC, bool isAccepted = false)
        {
            var ret = new List<InvoiceProvider>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    List<SD_26> data;
                    if (isAccepted)
                        data = (from sd26 in ctx.SD_26
                                .Include(_ => _.TD_26)
                                .Include("TD_26.TD_24")
                                .Include(_ => _.SD_43)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd &&
                                            _.SF_POST_DC == kontragentDC && _.SF_ACCEPTED == 1)
                            join td26 in ctx.TD_26
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                    .Include(_ => _.SD_43)
                                on sd26.DOC_CODE equals td26.DOC_CODE
                            //where 
                            select sd26).ToList();
                    else
                        data = (from sd26 in ctx.SD_26
                                .Include(_ => _.TD_26)
                                .Include("TD_26.TD_24")
                                .Include(_ => _.SD_43)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd &&
                                            _.SF_POST_DC == kontragentDC)
                            join td26 in ctx.TD_26
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                    .Include(_ => _.SD_43)
                                on sd26.DOC_CODE equals td26.DOC_CODE
                            //where 
                            select sd26).ToList();
                    var sql =
                        "SELECT s26.doc_code as DocCode, s26.SF_CRS_SUMMA as Summa, SUM(ISNULL(s34.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                        "FROM sd_26 s26 " +
                        "LEFT OUTER JOIN sd_34 s34 ON s34.SPOST_DC = s26.DOC_CODE " +
                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_POSTAV_DC = s26.DOC_CODE " +
                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SPOST_DC = s26.DOC_CODE " +
                        $"WHERE s26.SF_POSTAV_DATE >= '{CustomFormat.DateToString(dateStart)}' AND s26.SF_POSTAV_DATE <= '{CustomFormat.DateToString(dateEnd)}'" +
                        "GROUP BY s26.doc_code, s26.SF_CRS_SUMMA ";
                    var pays = ctx.Database.SqlQuery<InvoicePayment>(sql).ToList();
                    foreach (var d in data.OrderByDescending(_ => _.SF_POSTAV_DATE))
                    {
                        var newDoc = new InvoiceProvider(d);
                        if (isUsePayment)
                        {
                            var pd = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode);
                            if (pd == null)
                            {
                                ret.Add(newDoc);
                                continue;
                            }

                            if (newDoc.SF_CRS_SUMMA > pd.PaySumma)
                            {
                                newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                                {
                                    Summa = pd.PaySumma
                                });
                                ret.Add(newDoc);
                            }
                        }
                        else
                        {
                            newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                            {
                                Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                            });
                            ret.Add(newDoc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return ret;
        }

        #endregion

        #region Счет-фактура клиентам

        public static InvoiceClient GetInvoiceClient(decimal docCode)
        {
            //MainReferences.CheckUpdateKontragentAndLoad();
            // ReSharper disable once LocalVariableHidesMember
            decimal dc;
            if (docCode <= 0) return new InvoiceClient();
            dc = docCode;
            SD_84 data;
            var pDocs = new List<InvoicePaymentDocument>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                data = ctx.SD_84
                    .Include(_ => _.SD_431)
                    .Include(_ => _.SD_432)
                    .Include(_ => _.TD_84)
                    .Include("TD_84.SD_83")
                    .Include("TD_84.SD_83.SD_175")
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include("TD_84.TD_24")
                    .Include("TD_84.SD_303")
                    .AsNoTracking()
                    .SingleOrDefault(_ => _.DOC_CODE == dc);
                foreach (var c in ctx.SD_33.Where(_ => _.SFACT_DC == dc).ToList())
                    pDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = 0,
                        DocumentType = DocumentType.CashIn,
                        // ReSharper disable once PossibleInvalidOperationException
                        DocumentName =
                            $"{c.NUM_ORD} от {c.DATE_ORD} на {c.SUMM_ORD} {MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.SUMM_ORD,
                        Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                        Note = c.NOTES_ORD
                    });
                foreach (var c in ctx.TD_101.Include(_ => _.SD_101).Where(_ => _.VVT_SFACT_CLIENT_DC == dc).ToList())
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
                foreach (var c in ctx.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SFACT_DC == dc).ToList())
                    pDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.MutualAccounting,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE} на {c.VZT_CRS_SUMMA}",
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) c.VZT_CRS_SUMMA,
                        Currency = MainReferences.Currencies[c.SD_110.CurrencyFromDC],
                        Note = c.VZT_DOC_NOTES
                    });
            }

            var document = new InvoiceClient(data);
            foreach (var item in document.Rows)
            {
                var r = GlobalOptions.GetEntities()
                    .TD_24.Where(_ => _.DDT_SFACT_DC == item.DOC_CODE &&
                                      _.DDT_SFACT_ROW_CODE == item.Code)
                    .ToList();
                item.Shipped = r.Sum(_ => _.DDT_KOL_RASHOD);
                

                item.State = RowStatus.NotEdited;
                //var bilingItems = GlobalOptions.GetEntities()
                //    .TD_24.Where(_ => _.DDT_SFACT_DC == item.DOC_CODE && _.DDT_SFACT_ROW_CODE == item.Code);
                foreach (var i in r)
                    document.ShipmentRows.Add(new ShipmentRowViewModel(i));
            }

            document.REGISTER_DATE = DateTime.Today;
            document.DeletedRows = new List<InvoiceClientRow>();
            foreach (var p in pDocs) document.PaymentDocs.Add(p);
            document.State = RowStatus.NotEdited;
            foreach (var r in document.Rows) r.myState = RowStatus.NotEdited;
            return document;
        }

        public static InvoiceClient NewClient()
        {
            var ret = new InvoiceClient(null)
            {
                DocCode = -1,
                SF_DATE = DateTime.Today,
                REGISTER_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                //Currency = GlobalOptions.SystemProfile.MainCurrency,
                SF_IN_NUM = -1,
                SF_OUT_NUM = null,
                myState = RowStatus.NewRow,
                IsAccepted = false,
                Rows = new ObservableCollection<InvoiceClientRow>()
            };
            return ret;
        }

        public static InvoiceClient NewClientCopy(InvoiceClient doc)
        {
            InvoiceClient document;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx
                    .SD_84
                    .Include(_ => _.SD_431)
                    .Include(_ => _.SD_432)
                    .Include(_ => _.TD_84)
                    .Include("TD_84.SD_83")
                    .Include("TD_84.SD_83.SD_175")
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include("TD_84.TD_24")
                    .Include("TD_84.SD_303")
                    .AsNoTracking()
                    .SingleOrDefault(_ => _.DOC_CODE == doc.DocCode);
                document = new InvoiceClient(data)
                {
                    DocCode = -1,
                    SF_DATE = DateTime.Today,
                    REGISTER_DATE = DateTime.Today,
                    CREATOR = GlobalOptions.UserInfo.Name,
                    SF_IN_NUM = -1,
                    SF_OUT_NUM = null,
                    IsAccepted = false,
                    myState = RowStatus.NewRow
                };
                //Currency = GlobalOptions.SystemProfile.NationalCurrency,
                var newCode = 1;
                foreach (var item in document.Rows)
                {
                    item.DOC_CODE = -1;
                    item.DOC_CODE = newCode;
                    item.Shipped = 0;
                    var q =
                        GlobalOptions.GetEntities()
                            .NOM_PRICE.Where(_ => _.NOM_DC == item.SFT_NEMENKL_DC && _.DATE <= DateTime.Today)
                            .ToList();
                    if (q.Count == 0) continue;
                    var quanDate = q.Max(_ => _.DATE);
                    var firstOrDefault = GlobalOptions.GetEntities()
                        .NOM_PRICE.FirstOrDefault(_ => _.NOM_DC == item.SFT_NEMENKL_DC && _.DATE == quanDate);
                    if (firstOrDefault != null)
                    {
                        var quan =
                            firstOrDefault.NAKOPIT;
                        item.CurrentRemains = quan;
                    }

                    item.State = RowStatus.NewRow;
                    newCode++;
                }

                document.REGISTER_DATE = DateTime.Today;
                document.DeletedRows = new List<InvoiceClientRow>();
                document.State = RowStatus.NewRow;
                foreach (var r in document.Rows) r.myState = RowStatus.NewRow;
                document.PaymentDocs.Clear();
                document.ShipmentRows.Clear();
            }

            return document;
        }

        public static InvoiceClient NewClientCopy(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var d = ctx
                    .SD_84
                    .Include(_ => _.SD_431)
                    .Include(_ => _.SD_432)
                    .Include(_ => _.TD_84)
                    .Include("TD_84.SD_83")
                    .Include("TD_84.SD_83.SD_175")
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include("TD_84.TD_24")
                    .Include("TD_84.SD_303")
                    .AsNoTracking()
                    .SingleOrDefault(_ => _.DOC_CODE == dc);
                if (d == null) return null;
                var ret = new InvoiceClient(d)
                {
                    DocCode = -1,
                    SF_OUT_NUM = null,
                    SF_IN_NUM = -1,
                    REGISTER_DATE = DateTime.Today,
                    CREATOR = GlobalOptions.UserInfo.Name,
                    IsAccepted = false,
                    myState = RowStatus.NewRow
                };
                var code = 1;
                foreach (var row in ret.Rows)
                {
                    row.DocCode = -1;
                    row.Code = code;
                    row.myState = RowStatus.NewRow;
                    row.Shipped = 0;
                    var q =
                        GlobalOptions.GetEntities()
                            .NOM_PRICE.Where(_ => _.NOM_DC == row.SFT_NEMENKL_DC && _.DATE <= DateTime.Today)
                            .ToList();
                    if (q.Count == 0) continue;
                    var quanDate = q.Max(_ => _.DATE);
                    var firstOrDefault = GlobalOptions.GetEntities()
                        .NOM_PRICE.FirstOrDefault(_ => _.NOM_DC == row.SFT_NEMENKL_DC && _.DATE == quanDate);
                    if (firstOrDefault != null)
                    {
                        var quan =
                            firstOrDefault.NAKOPIT;
                        row.CurrentRemains = quan;
                    }

                    code++;
                }

                ret.PaymentDocs.Clear();
                ret.ShipmentRows.Clear();
                return ret;
            }
        }

        public static InvoiceClient NewClientRequisite(decimal dc)
        {
            var ret = NewClientCopy(dc);
            if (ret == null) return null;
            ret.Rows.Clear();
            ret.PaymentDocs.Clear();
            ret.ShipmentRows.Clear();
            return ret;
        }

        public static InvoiceClient NewClientRequisite(InvoiceClient doc)
        {
            var ret = NewClientCopy(doc.DocCode);
            if (ret == null) return null;
            ret.Rows.Clear();
            ret.PaymentDocs.Clear();
            ret.ShipmentRows.Clear();
            return ret;
        }

        /// <summary>
        ///     Список без учета дат
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="isUsePayment"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        public static List<InvoiceClient> GetInvoicesClient(string searchText = null, bool isUsePayment = false,
            bool isAccepted = false)
        {
            return GetInvoicesClient(new DateTime(1900, 1, 1), new DateTime(2100, 1, 1), isUsePayment, searchText,
                isAccepted);
        }

        /// <summary>
        ///     Список с диапазаном дат
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <param name="isUsePayment"></param>
        /// <param name="searchText"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        public static List<InvoiceClient> GetInvoicesClient(DateTime dateStart, DateTime dateEnd, bool isUsePayment,
            string searchText = null, bool isAccepted = false)
        {
            //MainReferences.CheckUpdateKontragentAndLoad();
            var ret = new List<InvoiceClient>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    List<SD_84> data;
                    if (string.IsNullOrEmpty(searchText))
                        if (isAccepted)
                            data = ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include(_ => _.SD_432)
                                .Include("TD_84.TD_24")
                                .Include("TD_84.SD_303")
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd
                                                                   && _.SF_ACCEPTED == 1).ToList();
                        else
                            data = ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include(_ => _.SD_432)
                                .Include("TD_84.TD_24")
                                .Include("TD_84.SD_303")
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd).ToList();
                    else if (isAccepted)
                        data = (from sd84 in ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include("TD_84.TD_24")
                                .Include(_ => _.SD_432)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Include("TD_84.SD_303")
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd
                                                                   && _.SF_ACCEPTED == 1)
                            join td84 in ctx.TD_84
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                on sd84.DOC_CODE equals td84.DOC_CODE
                            where sd84.SF_CRS_DC.ToString().Contains(searchText)
                                  || sd84.SF_CRS_RATE.ToString()
                                      .Contains(searchText)
                                  || sd84.SF_NOTE.Contains(searchText)
                                  || sd84.SF_IN_NUM.ToString().Contains(searchText)
                                  || sd84.SF_OUT_NUM.Contains(searchText)
                                  || sd84.SF_DATE.ToString()
                                      .Contains(searchText)
                                  || sd84.CREATOR.Contains(searchText)
                                  || sd84.SD_432.NAME.Contains(searchText)
                                  || sd84.SD_179.PT_NAME.Contains(searchText)
                                  || sd84.SD_77.TV_NAME.Contains(searchText)
                                  || sd84.SD_189.OOT_NAME.Contains(searchText)
                                  || sd84.SD_40.CENT_NAME.Contains(searchText)
                                  || td84.SFT_TEXT.Contains(searchText)
                                  || td84.SFT_ED_CENA.ToString().Contains(searchText)
                                  || td84.SFT_KOL.ToString().Contains(searchText)
                                  || td84.SFT_STRANA_PROIS.Contains(searchText)
                                  || td84.SFT_N_GRUZ_DECLAR.Contains(searchText)
                                  || td84.SD_83.NOM_NOMENKL.Contains(searchText)
                                  || td84.SD_83.NOM_NAME.Contains(searchText)
                                  || td84.SD_83.NOM_NOTES.Contains(searchText)
                                  || td84.SD_175.ED_IZM_NAME.Contains(searchText)
                            select sd84).ToList();
                    else
                        data = (from sd84 in ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include("TD_84.TD_24")
                                .Include(_ => _.SD_432)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Include("TD_84.SD_303")
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd)
                            join td84 in ctx.TD_84
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                    .Include(_ => _.SD_303)
                                on sd84.DOC_CODE equals td84.DOC_CODE
                            where sd84.SF_CRS_DC.ToString().Contains(searchText)
                                  || sd84.SF_CRS_RATE.ToString()
                                      .Contains(searchText)
                                  || sd84.SF_NOTE.Contains(searchText)
                                  || sd84.SF_IN_NUM.ToString().Contains(searchText)
                                  || sd84.SF_OUT_NUM.Contains(searchText)
                                  || sd84.SF_DATE.ToString()
                                      .Contains(searchText)
                                  || sd84.CREATOR.Contains(searchText)
                                  || sd84.SD_432.NAME.Contains(searchText)
                                  || sd84.SD_179.PT_NAME.Contains(searchText)
                                  || sd84.SD_77.TV_NAME.Contains(searchText)
                                  || sd84.SD_189.OOT_NAME.Contains(searchText)
                                  || sd84.SD_40.CENT_NAME.Contains(searchText)
                                  || td84.SFT_TEXT.Contains(searchText)
                                  || td84.SFT_ED_CENA.ToString().Contains(searchText)
                                  || td84.SFT_KOL.ToString().Contains(searchText)
                                  || td84.SFT_STRANA_PROIS.Contains(searchText)
                                  || td84.SFT_N_GRUZ_DECLAR.Contains(searchText)
                                  || td84.SD_83.NOM_NOMENKL.Contains(searchText)
                                  || td84.SD_83.NOM_NAME.Contains(searchText)
                                  || td84.SD_83.NOM_NOTES.Contains(searchText)
                                  || td84.SD_175.ED_IZM_NAME.Contains(searchText)
                            select sd84).ToList();
                    var sql =
                        "SELECT s84.doc_code as DocCode, s84.SF_CRS_SUMMA_K_OPLATE as Summa, SUM(ISNULL(s33.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                        "FROM sd_84 s84 " +
                        "LEFT OUTER JOIN sd_33 s33 ON s33.SFACT_DC = s84.DOC_CODE " +
                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_CLIENT_DC = s84.DOC_CODE " +
                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SFACT_DC = s84.DOC_CODE " +
                        $"WHERE s84.SF_DATE >= '{CustomFormat.DateToString(dateStart)}' AND s84.SF_DATE <= '{CustomFormat.DateToString(dateEnd)}'" +
                        "GROUP BY s84.doc_code, s84.SF_CRS_SUMMA_K_OPLATE ";
                    var pays = ctx.Database.SqlQuery<InvoicePayment>(sql).ToList();
                    //MainReferences.UpdateNomenkl();
                    foreach (var d in data.OrderByDescending(_ => _.SF_DATE))
                    {
                        if (ret.Any(_ => _.DocCode == d.DOC_CODE)) continue;
                        var newDoc = new InvoiceClient(d)
                        {
                            State = RowStatus.NotEdited
                        };
                        if (isUsePayment)
                        {
                            var pd = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode);
                            if (pd == null)
                            {
                                ret.Add(newDoc);
                                continue;
                            }

                            if (newDoc.SF_CRS_SUMMA_K_OPLATE > pd.PaySumma)
                            {
                                newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                                {
                                    Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                                });
                                ret.Add(newDoc);
                            }
                        }
                        else
                        {
                            newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                            {
                                Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                            });
                            ret.Add(newDoc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return ret;
        }

        public static List<InvoiceClient> GetInvoicesClient(DateTime dateStart, DateTime dateEnd, bool isUsePayment,
            decimal kontragentDC, bool isAccepted = false)
        {
            //MainReferences.CheckUpdateKontragentAndLoad();
            var ret = new List<InvoiceClient>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    List<SD_84> data;
                    if (isAccepted)
                        data = (from sd84 in ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include("TD_84.TD_24")
                                .Include(_ => _.SD_432)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd
                                                                   && _.SF_ACCEPTED == 1)
                            join td84 in ctx.TD_84
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                on sd84.DOC_CODE equals td84.DOC_CODE
                            where sd84.SF_CLIENT_DC == kontragentDC
                            select sd84).ToList();
                    else
                        data = (from sd84 in ctx.SD_84
                                .Include(_ => _.TD_84)
                                .Include("TD_84.TD_24")
                                .Include(_ => _.SD_432)
                                .Include(_ => _.SD_179)
                                .Include(_ => _.SD_77)
                                .Include(_ => _.SD_189)
                                .Include(_ => _.SD_40)
                                .Where(_ => _.SF_DATE >= dateStart && _.SF_DATE <= dateEnd)
                            join td84 in ctx.TD_84
                                    .Include(_ => _.SD_83)
                                    .Include(_ => _.SD_175)
                                on sd84.DOC_CODE equals td84.DOC_CODE
                            where sd84.SF_CLIENT_DC == kontragentDC
                            select sd84).ToList();
                    var sql =
                        "SELECT s84.doc_code as DocCode, s84.SF_CRS_SUMMA_K_OPLATE as Summa, SUM(ISNULL(s33.CRS_SUMMA,0)+ISNULL(t101.VVT_VAL_PRIHOD,0) + ISNULL(t110.VZT_CRS_SUMMA,0)) AS PaySumma " +
                        "FROM sd_84 s84 " +
                        "LEFT OUTER JOIN sd_33 s33 ON s33.SFACT_DC = s84.DOC_CODE " +
                        "LEFT OUTER JOIN td_101 t101 ON t101.VVT_SFACT_CLIENT_DC = s84.DOC_CODE " +
                        "LEFT OUTER JOIN td_110 t110 ON t110.VZT_SFACT_DC = s84.DOC_CODE " +
                        $"WHERE s84.SF_DATE >= '{CustomFormat.DateToString(dateStart)}' AND s84.SF_DATE <= '{CustomFormat.DateToString(dateEnd)}'" +
                        "GROUP BY s84.doc_code, s84.SF_CRS_SUMMA_K_OPLATE ";

                    var pays = ctx.Database.SqlQuery<InvoicePayment>(sql).ToList();
                    foreach (var d in data.OrderByDescending(_ => _.SF_DATE))
                    {
                        if (ret.Any(_ => _.DocCode == d.DOC_CODE)) continue;
                        var newDoc = new InvoiceClient(d)
                        {
                            State = RowStatus.NotEdited
                        };


                        if (isUsePayment)
                        {
                            var pd = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode);
                            if (pd == null)
                            {
                                ret.Add(newDoc);
                                continue;
                            }

                            if (newDoc.SF_CRS_SUMMA_K_OPLATE > pd.PaySumma)
                            {
                                newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                                {
                                    Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                                });
                                ret.Add(newDoc);
                            }
                        }
                        else
                        {
                            newDoc.PaymentDocs.Add(new InvoicePaymentDocument
                            {
                                Summa = pays.FirstOrDefault(_ => _.DocCode == newDoc.DocCode)?.PaySumma ?? 0
                            });
                            ret.Add(newDoc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return ret;
        }

        public static void DeleteClient(decimal dc, SearchInvoiceClientView searchWindow = null)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var doc = ctx.SD_84.Include(_ => _.TD_84).FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (doc == null) return;
                        ctx.SD_84.Remove(doc);
                        ctx.SaveChanges();
                        transaction.Commit();
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

        public static decimal SaveClient(InvoiceClient doc, ProviderSearchView searchWindow = null)
        {
            decimal dc;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var dtx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (doc.DocCode == -1)
                        {
                            var guidId = Guid.NewGuid();
                            var inNum = ctx.SD_84.Any() ? ctx.SD_84.Max(_ => _.SF_IN_NUM) + 1 : 1;
                            dc = ctx.SD_84.Any() ? ctx.SD_84.Max(_ => _.DOC_CODE) + 1 : 10840000001;
                            ctx.SD_84.Add(new SD_84
                            {
                                Id = guidId,
                                DOC_CODE = dc,
                                SF_IN_NUM = inNum,
                                SF_OUT_NUM = doc.SF_OUT_NUM,
                                SF_DATE = doc.SF_DATE,
                                REGISTER_DATE = doc.REGISTER_DATE,
                                SF_PAY_COND_DC = doc.SF_PAY_COND_DC,
                                SF_CRS_SUMMA_K_OPLATE = doc.SF_CRS_SUMMA_K_OPLATE,
                                SF_KONTR_CRS_DC = doc.SF_KONTR_CRS_DC,
                                SF_KONTR_CRS_RATE = 1,
                                SF_KONTR_CRS_SUMMA = doc.SF_CRS_SUMMA_K_OPLATE,
                                SF_CRS_DC = doc.SF_CRS_DC,
                                SF_CRS_RATE = doc.SF_CRS_RATE,
                                SF_CLIENT_DC = doc.SF_CLIENT_DC,
                                SF_CLIENT_NAME = doc.SF_CLIENT_NAME,
                                SF_DILER_SUMMA = doc.SF_DILER_SUMMA,
                                SF_DILER_DC = doc.SF_DILER_DC,
                                SF_DILER_RATE = 1,
                                SF_DILER_CRS_DC = doc.SF_CRS_DC,
                                SF_ACCEPTED = doc.SF_ACCEPTED,
                                SF_CENTR_OTV_DC = doc.SF_CENTR_OTV_DC,
                                CREATOR = GlobalOptions.UserInfo.NickName,
                                SF_VZAIMOR_TYPE_DC = doc.SF_VZAIMOR_TYPE_DC,
                                SF_FORM_RASCH_DC = doc.SF_FORM_RASCH_DC,
                                SF_NOTE = doc.SF_NOTE,
                                SF_RECEIVER_KONTR_DC = doc.SF_RECEIVER_KONTR_DC,
                                SF_NDS_1INCLUD_0NO = doc.SF_NDS_1INCLUD_0NO
                            });
                            if (doc.Rows.Count > 0)
                            {
                                var code = 1;
                                foreach (var items in doc.Rows)
                                {
                                    ctx.TD_84.Add(new TD_84
                                    {
                                        DOC_CODE = dc,
                                        CODE = code,
                                        DocId = guidId,
                                        Id = Guid.NewGuid(),
                                        SFT_TEXT = items.SFT_TEXT ?? " ",
                                        SFT_NEMENKL_DC = items.SFT_NEMENKL_DC,
                                        SFT_KOL = items.SFT_KOL,
                                        SFT_NDS_PERCENT = items.SFT_NDS_PERCENT,
                                        OLD_NOM_NAME = items.OLD_NOM_NAME,
                                        OLD_NOM_NOMENKL = items.OLD_NOM_NOMENKL,
                                        OLD_OVERHEAD_CRS_NAME = items.OLD_OVERHEAD_CRS_NAME,
                                        OLD_OVERHEAD_NAME = items.OLD_OVERHEAD_NAME,
                                        OLD_UNIT_NAME = items.OLD_UNIT_NAME,
                                        SFT_ACCIZ = items.SFT_ACCIZ,
                                        SFT_COUNTRY_CODE = items.SFT_COUNTRY_CODE,
                                        SFT_DOG_OTGR_DC = items.SFT_DOG_OTGR_DC,
                                        SFT_DOG_OTGR_PLAN_CODE = items.SFT_DOG_OTGR_PLAN_CODE,
                                        SFT_ED_CENA = items.SFT_ED_CENA,
                                        SFT_KOMPLEKT = items.SFT_KOMPLEKT,
                                        SFT_UCHET_ED_IZM_DC = items.SFT_UCHET_ED_IZM_DC,
                                        SFT_NACENKA_DILERA = items.SFT_NACENKA_DILERA,
                                        SFT_NALOG_NA_PROD = items.SFT_NALOG_NA_PROD,
                                        SFT_N_GRUZ_DECLAR = items.SFT_N_GRUZ_DECLAR,
                                        SFT_PROCENT_ZS_RASHODOV = items.SFT_PROCENT_ZS_RASHODOV,
                                        SFT_SHPZ_DC = items.SFT_SHPZ_DC,
                                        SFT_STDP_DC = items.SFT_STDP_DC,
                                        SFT_STRANA_PROIS = items.SFT_STRANA_PROIS,
                                        SFT_SUMMA_K_OPLATE = items.SFT_SUMMA_K_OPLATE,
                                        SFT_SUMMA_K_OPLATE_KONTR_CRS = items.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                                        SFT_SUMMA_NDS = items.SFT_SUMMA_NDS,
                                        SFT_TARA_DC = items.SFT_TARA_DC,
                                        SFT_TARA_FLAG = items.SFT_TARA_FLAG,
                                        SFT_TARA_MEST = items.SFT_TARA_MEST
                                    });
                                    code += 1;
                                }
                            }

                            if (doc.DeletedRows.Count > 0)
                                foreach (var item in doc.DeletedRows)
                                {
                                    var deletedItem = ctx.TD_84.FirstOrDefault(_ => _.Id == item.Id);
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    ctx.TD_84.Remove(deletedItem);
                                }
                        }
                        else
                        {
                            dc = doc.DocCode;
                            var item = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            // ReSharper disable once PossibleNullReferenceException
                            item.SF_OUT_NUM = doc.SF_OUT_NUM;
                            item.SF_DATE = doc.SF_DATE;
                            item.REGISTER_DATE = doc.REGISTER_DATE;
                            item.SF_PAY_COND_DC = doc.SF_PAY_COND_DC;
                            item.SF_CRS_SUMMA_K_OPLATE = doc.SF_CRS_SUMMA_K_OPLATE;
                            item.SF_CRS_DC = doc.SF_CRS_DC;
                            item.SF_CRS_RATE = doc.SF_CRS_RATE;
                            item.SF_CLIENT_DC = doc.SF_CLIENT_DC;
                            item.SF_CLIENT_NAME = doc.SF_CLIENT_NAME;
                            item.SF_KONTR_CRS_DC = doc.SF_KONTR_CRS_DC;
                            item.SF_KONTR_CRS_RATE = 1;
                            item.SF_KONTR_CRS_SUMMA = doc.SF_CRS_SUMMA_K_OPLATE;
                            item.SF_DILER_SUMMA = doc.SF_DILER_SUMMA;
                            item.SF_DILER_DC = doc.SF_DILER_DC;
                            item.SF_DILER_SUMMA = doc.SF_DILER_SUMMA;
                            item.SF_DILER_RATE = doc.SF_DILER_RATE;
                            item.SF_DILER_CRS_DC = doc.SF_DILER_CRS_DC;
                            item.SF_ACCEPTED = doc.SF_ACCEPTED;
                            item.SF_CENTR_OTV_DC = doc.SF_CENTR_OTV_DC;
                            item.CREATOR = GlobalOptions.UserInfo.NickName;
                            item.SF_VZAIMOR_TYPE_DC = doc.SF_VZAIMOR_TYPE_DC;
                            item.SF_FORM_RASCH_DC = doc.SF_FORM_RASCH_DC;
                            item.SF_NOTE = doc.SF_NOTE;
                            item.SF_RECEIVER_KONTR_DC = doc.SF_RECEIVER_KONTR_DC;
                            item.SF_NDS_1INCLUD_0NO = doc.SF_NDS_1INCLUD_0NO;
                            if (doc.Rows.Count > 0)
                            {
                                var code = doc.Rows.Count > 0 ? doc.Rows.Max(_ => _.Code) : 0;
                                foreach (var items in doc.Rows)
                                {
                                    // ReSharper disable once PossibleNullReferenceException
                                    var docGuid = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode).Id;
                                    var data = ctx.TD_84.FirstOrDefault(_ =>
                                        _.DOC_CODE == items.DOC_CODE && _.CODE == items.Code);
                                    if (data == null)
                                    {
                                        ctx.TD_84.Add(new TD_84
                                        {
                                            DOC_CODE = doc.DocCode,
                                            CODE = code,
                                            DocId = docGuid,
                                            SFT_TEXT = items.SFT_TEXT ?? " ",
                                            SFT_NEMENKL_DC = items.SFT_NEMENKL_DC,
                                            SFT_KOL = items.SFT_KOL,
                                            SFT_NDS_PERCENT = items.SFT_NDS_PERCENT,
                                            Id = Guid.NewGuid(),
                                            OLD_NOM_NAME = items.OLD_NOM_NAME,
                                            OLD_NOM_NOMENKL = items.OLD_NOM_NOMENKL,
                                            OLD_OVERHEAD_CRS_NAME = items.OLD_OVERHEAD_CRS_NAME,
                                            OLD_OVERHEAD_NAME = items.OLD_OVERHEAD_NAME,
                                            OLD_UNIT_NAME = items.OLD_UNIT_NAME,
                                            SFT_ACCIZ = items.SFT_ACCIZ,
                                            SFT_COUNTRY_CODE = items.SFT_COUNTRY_CODE,
                                            SFT_DOG_OTGR_DC = items.SFT_DOG_OTGR_DC,
                                            SFT_DOG_OTGR_PLAN_CODE = items.SFT_DOG_OTGR_PLAN_CODE,
                                            SFT_ED_CENA = items.SFT_ED_CENA,
                                            SFT_KOMPLEKT = items.SFT_KOMPLEKT,
                                            SFT_UCHET_ED_IZM_DC = items.SFT_UCHET_ED_IZM_DC,
                                            SFT_NACENKA_DILERA = items.SFT_NACENKA_DILERA,
                                            SFT_NALOG_NA_PROD = items.SFT_NALOG_NA_PROD,
                                            SFT_N_GRUZ_DECLAR = items.SFT_N_GRUZ_DECLAR,
                                            SFT_PROCENT_ZS_RASHODOV = items.SFT_PROCENT_ZS_RASHODOV,
                                            SFT_SHPZ_DC = items.SFT_SHPZ_DC,
                                            SFT_STDP_DC = items.SFT_STDP_DC,
                                            SFT_STRANA_PROIS = items.SFT_STRANA_PROIS,
                                            SFT_SUMMA_K_OPLATE = items.SFT_SUMMA_K_OPLATE,
                                            SFT_SUMMA_K_OPLATE_KONTR_CRS = items.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                                            SFT_SUMMA_NDS = items.SFT_SUMMA_NDS,
                                            SFT_TARA_DC = items.SFT_TARA_DC,
                                            SFT_TARA_FLAG = items.SFT_TARA_FLAG,
                                            SFT_TARA_MEST = items.SFT_TARA_MEST
                                        });
                                        code++;
                                    }
                                    else
                                    {
                                        // ReSharper disable once PossibleNullReferenceException
                                        data.SFT_TEXT = items.SFT_TEXT ?? " ";
                                        data.SFT_NEMENKL_DC = items.SFT_NEMENKL_DC;
                                        data.SFT_KOL = items.SFT_KOL;
                                        data.SFT_NDS_PERCENT = items.SFT_NDS_PERCENT;
                                        data.OLD_NOM_NAME = items.OLD_NOM_NAME;
                                        data.OLD_NOM_NOMENKL = items.OLD_NOM_NOMENKL;
                                        data.OLD_OVERHEAD_CRS_NAME = items.OLD_OVERHEAD_CRS_NAME;
                                        data.OLD_OVERHEAD_NAME = items.OLD_OVERHEAD_NAME;
                                        data.OLD_UNIT_NAME = items.OLD_UNIT_NAME;
                                        data.SFT_ACCIZ = items.SFT_ACCIZ;
                                        data.SFT_COUNTRY_CODE = items.SFT_COUNTRY_CODE;
                                        data.SFT_DOG_OTGR_DC = items.SFT_DOG_OTGR_DC;
                                        data.SFT_DOG_OTGR_PLAN_CODE = items.SFT_DOG_OTGR_PLAN_CODE;
                                        data.SFT_ED_CENA = items.SFT_ED_CENA;
                                        data.SFT_KOMPLEKT = items.SFT_KOMPLEKT;
                                        data.SFT_UCHET_ED_IZM_DC = items.SFT_UCHET_ED_IZM_DC;
                                        data.SFT_NACENKA_DILERA = items.SFT_NACENKA_DILERA;
                                        data.SFT_NALOG_NA_PROD = items.SFT_NALOG_NA_PROD;
                                        data.SFT_N_GRUZ_DECLAR = items.SFT_N_GRUZ_DECLAR;
                                        data.SFT_PROCENT_ZS_RASHODOV = items.SFT_PROCENT_ZS_RASHODOV;
                                        data.SFT_SHPZ_DC = items.SFT_SHPZ_DC;
                                        data.SFT_STDP_DC = items.SFT_STDP_DC;
                                        data.SFT_STRANA_PROIS = items.SFT_STRANA_PROIS;
                                        data.SFT_SUMMA_K_OPLATE = items.SFT_SUMMA_K_OPLATE;
                                        data.SFT_SUMMA_K_OPLATE_KONTR_CRS = items.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                                        data.SFT_SUMMA_NDS = items.SFT_SUMMA_NDS;
                                        data.SFT_TARA_DC = items.SFT_TARA_DC;
                                        data.SFT_TARA_FLAG = items.SFT_TARA_FLAG;
                                        data.SFT_TARA_MEST = items.SFT_TARA_MEST;
                                    }
                                }
                            }

                            if (doc.DeletedRows.Count > 0)
                                foreach (var i in doc.DeletedRows)
                                {
                                    var deletedItem =
                                        ctx.TD_84.FirstOrDefault(_ => _.CODE == i.Code && _.DOC_CODE == i.DOC_CODE);
                                    if (deletedItem != null)
                                        ctx.TD_84.Remove(deletedItem);
                                }
                        }

                        ctx.SaveChanges();
                        dtx.Commit();
                        foreach (var r in doc.Rows) r.myState = RowStatus.NotEdited;
                        doc.myState = RowStatus.NotEdited;
                    }
                    catch (Exception e)
                    {
                        dtx.Rollback();
                        WindowManager.ShowError(e);
                        return -1;
                    }
                }
            }

            return dc;
        }

        #endregion
    }
}