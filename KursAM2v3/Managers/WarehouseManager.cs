using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Nomenkl;
using KursAM2.ViewModel.Logistiks.Warehouse;

namespace KursAM2.Managers
{
    public class WarehouseManager
    {
        private readonly ErrorMessageBase myErrorMessager;

        public WarehouseManager(ErrorMessageBase errMessager)
        {
            myErrorMessager = errMessager;
        }

        #region Справочник складов

        public static List<Warehouse> GetWarehouses(List<Warehouse> excludeList = null, string searchText = null)
        {
            var result = new List<Warehouse>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = searchText == null
                        ? ctx.SD_27.ToList()
                        : ctx.SD_27.Where(_ => _.SKL_NAME.ToUpper().Contains(searchText.ToUpper())).ToList();
                    if (excludeList == null || excludeList.Count == 0)
                        foreach (var d in data)
                            result.Add(new Warehouse(d));
                    else
                        foreach (var d in data)
                            if (excludeList.All(_ => _.DocCode != d.DOC_CODE))
                                result.Add(new Warehouse(d));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        #endregion

        #region Приходный складской ордер

        /// <summary>
        ///     Загрузить приходный складской ордер
        /// </summary>
        /// <param name="dc">Код документа</param>
        /// <returns></returns>
        public WarehouseOrderIn GetOrderIn(decimal dc)
        {
            WarehouseOrderIn result;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_26.SD_26")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_122")
                        .Include("TD_24.SD_170")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_1751")
                        .Include("TD_24.SD_2")
                        .Include("TD_24.SD_254")
                        .Include("TD_24.SD_27")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_3011")
                        .Include("TD_24.SD_3012")
                        .Include("TD_24.SD_303")
                        .Include("TD_24.SD_384")
                        .Include("TD_24.SD_43")
                        .Include("TD_24.SD_83")
                        .Include("TD_24.SD_831")
                        .Include("TD_24.SD_832")
                        .Include("TD_24.SD_84")
                        .Include("TD_24.TD_73")
                        .Include("TD_24.TD_9")
                        .Include("TD_24.TD_84")
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_241")
                        .Include("TD_24.TD_242")
                        .Include("TD_24.TD_242.SD_24")
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    result = new WarehouseOrderIn(data);
                    foreach (var r in result.Rows) r.myState = RowStatus.NotEdited;
                    result.myState = RowStatus.NotEdited;
                }
            }
            catch (Exception e)
            {
                myErrorMessager.WriteErrorMessage(e, "Ошибка", "$WarehouseManager.GetOrderIn {dc}");
                return null;
            }

            return result;
        }

        public WarehouseOrderIn NewOrderIn()
        {
            return new WarehouseOrderIn
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
        }

        public WarehouseOrderIn NewOrderInCopy(WarehouseOrderIn doc)
        {
            var res = new WarehouseOrderIn(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            foreach (var r in res.Rows)
            {
                r.DocCode = -1;
                r.State = RowStatus.NewRow;
            }

            return res;
        }

        public WarehouseOrderIn NewOrderInCopy(decimal dc)
        {
            var doc = GetOrderIn(dc);
            var res = new WarehouseOrderIn(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            foreach (var r in res.Rows)
            {
                r.DocCode = -1;
                r.State = RowStatus.NewRow;
            }

            return res;
        }

        public WarehouseOrderIn NewOrderInRecuisite(WarehouseOrderIn doc)
        {
            var ret = new WarehouseOrderIn(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            ret.Rows.Clear();
            return ret;
        }

        public WarehouseOrderIn NewOrderInRecuisite(decimal dc)
        {
            var ret = new WarehouseOrderIn(GetOrderIn(dc).Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            ret.Rows.Clear();
            return ret;
        }

        public decimal SaveOrderIn(WarehouseOrderIn doc, WarehouseOrderInSearchViewModel inSearchWindow = null)
        {
            var newDC = doc.DocCode;
            using (var ctx = GlobalOptions.GetEntities())
            {
                ctx.Database.CommandTimeout = 120;
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var d in doc.DeletedRows)
                        {
                            var oldrow = ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == d.DocCode && _.CODE == d.Code);
                            if (oldrow == null) continue;
                            ctx.TD_24.Remove(oldrow);
                        }

                        if (doc.DocCode == -1)
                        {
                            var guidId = Guid.NewGuid();
                            var inNum = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DD_IN_NUM) + 1 : 1;
                            newDC = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                            ctx.SD_24.Add(new SD_24
                            {
                                DOC_CODE = newDC,
                                DD_TYPE_DC = doc.Entity.DD_TYPE_DC,
                                DD_DATE = doc.Date,
                                DD_IN_NUM = inNum,
                                DD_EXT_NUM = doc.DD_EXT_NUM,
                                DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC,
                                DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC,
                                DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC,
                                DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC,
                                DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN,
                                DD_EXECUTED = doc.DD_EXECUTED,
                                DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN,
                                DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME,
                                DD_POLUCH_NAME = doc.DD_POLUCH_NAME,
                                DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN,
                                DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO,
                                DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO,
                                DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL,
                                DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL,
                                DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC,
                                DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC,
                                DD_SHABLON = doc.DD_SHABLON,
                                DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC,
                                DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC,
                                DD_TREB_NUM = doc.DD_TREB_NUM,
                                DD_TREB_DATE = doc.DD_TREB_DATE,
                                DD_TREB_DC = doc.DD_TREB_DC,
                                CREATOR = doc.CREATOR,
                                DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN,
                                DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC,
                                DD_SCHET = doc.DD_SCHET,
                                DD_DOVERENNOST = doc.DD_DOVERENNOST,
                                DD_NOSZATR_ID = doc.DD_NOSZATR_ID,
                                DD_NOSZATR_DC = doc.DD_NOSZATR_DC,
                                DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC,
                                DD_NOTES = doc.Note,
                                DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC,
                                DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE,
                                DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC,
                                DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE,
                                DD_SPOST_DC = doc.Entity.DD_SPOST_DC,
                                DD_SFACT_DC = doc.Entity.DD_SFACT_DC,
                                DD_VOZVRAT = doc.Entity.DD_VOZVRAT,
                                DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE,
                                DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE,
                                DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV,
                                DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG,
                                DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE,
                                DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR,
                                DD_VIEZD_DATE = doc.DD_VIEZD_DATE,
                                DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC,
                                DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE,
                                DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC,
                                TSTAMP = doc.TSTAMP,
                                OWNER_ID = doc.OWNER_ID,
                                OWNER_TEXT = doc.OWNER_TEXT,
                                CONSIGNEE_ID = doc.CONSIGNEE_ID,
                                CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT,
                                BUYER_ID = doc.BUYER_ID,
                                BUYER_TEXT = doc.BUYER_TEXT,
                                SHIPMENT_ID = doc.SHIPMENT_ID,
                                SHIPMENT_TEXT = doc.SHIPMENT_TEXT,
                                SUPPLIER_ID = doc.SUPPLIER_ID,
                                SUPPLIER_TEXT = doc.SUPPLIER_TEXT,
                                GRUZO_INFO_ID = doc.GRUZO_INFO_ID,
                                GROZO_REQUISITE = doc.GROZO_REQUISITE,
                                Id = guidId
                            });
                            if (doc.Rows.Count > 0)
                            {
                                var code = 1;
                                foreach (var item in doc.Rows)
                                {
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = item.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = item.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = item.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = item.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) item.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) item.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = item.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = item.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = item.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = item.DDT_CRS_DC,
                                        DDT_SFACT_DC = item.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = item.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) item.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) item.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = item.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = item.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = item.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = item.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = item.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = item.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = item.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = item.DDT_TREB_DC,
                                        DDT_TREB_CODE = item.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = item.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = item.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = item.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) item.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = item.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = item.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = item.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = item.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = item.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = item.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = item.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = item.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = item.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = item.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = item.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) item.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = item.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = item.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = item.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = item.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) item.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = item.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = item.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) item.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = item.DDT_OS_DC,
                                        DDT_GARANT_DC = item.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = item.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) item.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = item.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) item.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = item.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = item.DDT_DILER_DC,
                                        DDT_DILER_SUM = item.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = item.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = item.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = item.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = item.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = item.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = item.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = item.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = item.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = item.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = item.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = item.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = item.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = item.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = item.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) item.DDT_MEST_TARA,
                                        DDT_TARA_DC = item.DDT_TARA_DC,
                                        DDT_TARA_FLAG = item.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = item.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = item.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = item.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = item.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = item.DDT_CALC_UCHET_TAX_PRICE,
                                        TSTAMP = item.TSTAMP,
                                        Id = Guid.NewGuid(),
                                        DocId = guidId
                                    });
                                    code++;
                                }
                            }
                        }
                        else
                        {
                            var old = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            if (old == null) return doc.DocCode;
                            old.DD_TYPE_DC = doc.Entity.DD_TYPE_DC;
                            old.DD_DATE = doc.Date;
                            old.DD_IN_NUM = doc.DD_IN_NUM;
                            old.DD_EXT_NUM = doc.DD_EXT_NUM;
                            old.DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC;
                            old.DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC;
                            old.DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC;
                            old.DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC;
                            old.DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN;
                            old.DD_EXECUTED = doc.DD_EXECUTED;
                            old.DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN;
                            old.DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME;
                            old.DD_POLUCH_NAME = doc.DD_POLUCH_NAME;
                            old.DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN;
                            old.DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO;
                            old.DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO;
                            old.DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL;
                            old.DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL;
                            old.DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC;
                            old.DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC;
                            old.DD_SHABLON = doc.DD_SHABLON;
                            old.DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC;
                            old.DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC;
                            old.DD_TREB_NUM = doc.DD_TREB_NUM;
                            old.DD_TREB_DATE = doc.DD_TREB_DATE;
                            old.DD_TREB_DC = doc.DD_TREB_DC;
                            old.CREATOR = doc.CREATOR;
                            old.DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN;
                            old.DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC;
                            old.DD_SCHET = doc.DD_SCHET;
                            old.DD_DOVERENNOST = doc.DD_DOVERENNOST;
                            old.DD_NOSZATR_ID = doc.DD_NOSZATR_ID;
                            old.DD_NOSZATR_DC = doc.DD_NOSZATR_DC;
                            old.DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC;
                            old.DD_NOTES = doc.Note;
                            old.DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC;
                            old.DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE;
                            old.DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC;
                            old.DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE;
                            old.DD_SPOST_DC = doc.Entity.DD_SPOST_DC;
                            old.DD_SFACT_DC = doc.Entity.DD_SFACT_DC;
                            old.DD_VOZVRAT = doc.Entity.DD_VOZVRAT;
                            old.DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE;
                            old.DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE;
                            old.DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV;
                            old.DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG;
                            old.DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE;
                            old.DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR;
                            old.DD_VIEZD_DATE = doc.DD_VIEZD_DATE;
                            old.DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC;
                            old.DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE;
                            old.DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC;
                            old.TSTAMP = doc.TSTAMP;
                            old.OWNER_ID = doc.OWNER_ID;
                            old.OWNER_TEXT = doc.OWNER_TEXT;
                            old.CONSIGNEE_ID = doc.CONSIGNEE_ID;
                            old.CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT;
                            old.BUYER_ID = doc.BUYER_ID;
                            old.BUYER_TEXT = doc.BUYER_TEXT;
                            old.SHIPMENT_ID = doc.SHIPMENT_ID;
                            old.SHIPMENT_TEXT = doc.SHIPMENT_TEXT;
                            old.SUPPLIER_ID = doc.SUPPLIER_ID;
                            old.SUPPLIER_TEXT = doc.SUPPLIER_TEXT;
                            old.GRUZO_INFO_ID = doc.GRUZO_INFO_ID;
                            old.GROZO_REQUISITE = doc.GROZO_REQUISITE;
                            var codes = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                            var code = codes.Count > 0 ? codes.Max(_ => _.CODE) : 0;
                            foreach (var r in doc.Rows)
                            {
                                var oldrow = ctx.TD_24.FirstOrDefault(_ =>
                                    _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (oldrow == null)
                                {
                                    code++;
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = r.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = r.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = r.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = r.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = r.DDT_CRS_DC,
                                        DDT_SFACT_DC = r.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = r.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = r.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = r.DDT_TREB_DC,
                                        DDT_TREB_CODE = r.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = r.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = r.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = r.DDT_OS_DC,
                                        DDT_GARANT_DC = r.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = r.DDT_DILER_DC,
                                        DDT_DILER_SUM = r.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = r.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) r.DDT_MEST_TARA,
                                        DDT_TARA_DC = r.DDT_TARA_DC,
                                        DDT_TARA_FLAG = r.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = r.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE,
                                        Id = Guid.NewGuid(),
                                        DocId = old.Id
                                    });
                                }
                                else
                                {
                                    oldrow.DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD;
                                    oldrow.DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO;
                                    oldrow.DDT_KOL_RASHOD = r.DDT_KOL_RASHOD;
                                    oldrow.DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO;
                                    oldrow.DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD;
                                    oldrow.DDT_ED_IZM_DC = r.DDT_ED_IZM_DC;
                                    oldrow.DDT_SPOST_DC = r.DDT_SPOST_DC;
                                    oldrow.DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE;
                                    oldrow.DDT_CRS_DC = r.DDT_CRS_DC;
                                    oldrow.DDT_SFACT_DC = r.DDT_SFACT_DC;
                                    oldrow.DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE;
                                    oldrow.DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR;
                                    oldrow.DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV;
                                    oldrow.DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA;
                                    oldrow.DDT_TAX_CENA = r.DDT_TAX_CENA;
                                    oldrow.DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED;
                                    oldrow.DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT;
                                    oldrow.DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA;
                                    oldrow.DDT_FACT_CENA = r.DDT_FACT_CENA;
                                    oldrow.DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED;
                                    oldrow.DDT_TREB_DC = r.DDT_TREB_DC;
                                    oldrow.DDT_TREB_CODE = r.DDT_TREB_CODE;
                                    oldrow.DDT_NOSZATR_DC = r.DDT_NOSZATR_DC;
                                    oldrow.DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE;
                                    oldrow.DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC;
                                    oldrow.DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD;
                                    oldrow.DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA;
                                    oldrow.DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA;
                                    oldrow.DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA;
                                    oldrow.DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC;
                                    oldrow.DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE;
                                    oldrow.DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE;
                                    oldrow.DDT_SHPZ_DC = r.DDT_SHPZ_DC;
                                    oldrow.DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA;
                                    oldrow.DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE;
                                    oldrow.DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE;
                                    oldrow.DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC;
                                    oldrow.DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE;
                                    oldrow.DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC;
                                    oldrow.DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC;
                                    oldrow.DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE;
                                    oldrow.DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC;
                                    oldrow.DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE;
                                    oldrow.DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA;
                                    oldrow.DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN;
                                    oldrow.DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE;
                                    oldrow.DDT_OS_DC = r.DDT_OS_DC;
                                    oldrow.DDT_GARANT_DC = r.DDT_GARANT_DC;
                                    oldrow.DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE;
                                    oldrow.DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM;
                                    oldrow.DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE;
                                    oldrow.DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE;
                                    oldrow.DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE;
                                    oldrow.DDT_DILER_DC = r.DDT_DILER_DC;
                                    oldrow.DDT_DILER_SUM = r.DDT_DILER_SUM;
                                    oldrow.DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE;
                                    oldrow.DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE;
                                    oldrow.DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER;
                                    oldrow.DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC;
                                    oldrow.DDT_RASHOD_DATE = r.DDT_RASHOD_DATE;
                                    oldrow.DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR;
                                    oldrow.DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE;
                                    oldrow.DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC;
                                    oldrow.DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC;
                                    oldrow.DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE;
                                    oldrow.DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE;
                                    oldrow.DDT_MEST_TARA = (double?) r.DDT_MEST_TARA;
                                    oldrow.DDT_TARA_DC = r.DDT_TARA_DC;
                                    oldrow.DDT_TARA_FLAG = r.DDT_TARA_FLAG;
                                    oldrow.DDT_PART_NUMBER = r.DDT_PART_NUMBER;
                                    oldrow.DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE;
                                    oldrow.DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE;
                                    oldrow.DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE;
                                    oldrow.DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE;
                                }
                            }
                        }

                        ctx.SaveChanges();
                        var wManager = new WindowManager();
                        foreach (var r in doc.Rows.Where(_ => _.DDT_SPOST_DC != null))
                        {
                            var schets = ctx.TD_26.First(_ => _.DOC_CODE == r.DDT_SPOST_DC
                                                              && _.CODE == r.DDT_SPOST_ROW_CODE);
                            if (r.DDT_KOL_PRIHOD > schets.SFT_KOL)
                            {
                                var res = wManager.ShowWinUIMessageBox($"По товару {r.NomNomenkl} {r.Nomenkl} " +
                                                                       "Приход превысил кол-во по счету ",
                                    "Превышение кол-ва прихода над кол-вом в счете", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning);
                                switch (res)
                                {
                                    case MessageBoxResult.Yes:
                                        continue;
                                    case MessageBoxResult.No:
                                        transaction.Rollback();
                                        return -1;
                                }
                            }
                        }

                        var calc = new NomenklCostMediumSliding(ctx);
                        foreach (var n in doc.Rows.Select(_ => _.Nomenkl.DocCode))
                        {
                            var ops = calc.GetOperations(n);
                            if (ops != null && ops.Count > 0)
                                calc.Save(ops);
                            var c = NomenklCalculationManager.NomenklRemain(ctx, DateTime.Today, n,
                                // ReSharper disable once PossibleInvalidOperationException
                                (decimal) doc.Entity.DD_SKLAD_POL_DC);
                            if (c < 0)
                            {
                                var nom = MainReferences.GetNomenkl(n);
                                var res = wManager.ShowWinUIMessageBox($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                                       $"склад {MainReferences.Warehouses[(decimal) doc.Entity.DD_SKLAD_POL_DC]} в кол-ве {c}." +
                                                                       "Все равно сохранить?",
                                    "Отрицательные остатки", MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning);
                                switch (res)
                                {
                                    case MessageBoxResult.Yes:
                                        continue;
                                    case MessageBoxResult.No:
                                        transaction.Rollback();
                                        return -1;
                                }
                            }
                        }

                        transaction.Commit();
                        doc.myState = RowStatus.NotEdited;
                        foreach (var r in doc.Rows) r.myState = RowStatus.NotEdited;
                        doc.DeletedRows.Clear();
                        return newDC;
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
        }

        public void DeleteOrderIn(WarehouseOrderIn doc, WarehouseOrderInSearchViewModel inSearchWindow = null)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var rows = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                        if (rows.Count > 0)
                            foreach (var r in rows)
                            {
                                var oldrow =
                                    ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == r.DOC_CODE && _.CODE == r.CODE);
                                if (oldrow != null)
                                    ctx.TD_24.Remove(oldrow);
                            }

                        var olddoc = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                        if (olddoc != null)
                            ctx.SD_24.Remove(olddoc);
                        ctx.SaveChanges();
                        var calc = new NomenklCostMediumSliding(ctx);
                        foreach (var n in doc.Rows.Select(_ => _.Nomenkl.DocCode))
                        {
                            var ops = calc.GetOperations(n);
                            if (ops != null && ops.Count > 0)
                                calc.Save(ops);
                            var c = NomenklCalculationManager.GetNomenklStoreRemain(ctx, DateTime.Today, n,
                                // ReSharper disable once PossibleInvalidOperationException
                                (decimal) doc.Entity.DD_SKLAD_POL_DC);
                            if (c < 0)
                            {
                                transaction.Rollback();
                                var nom = MainReferences.GetNomenkl(n);
                                WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                          $"склад {MainReferences.Warehouses[(decimal) doc.Entity.DD_SKLAD_POL_DC]} в кол-ве {c} ",
                                    "Отрицательные остатки", MessageBoxImage.Error);
                                return;
                            }
                        }

                        transaction.Commit();
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

        public static List<WarehouseOrderIn> GetOrdersIn(string searchText = null)
        {
            return new List<WarehouseOrderIn>();
        }

        public static List<WarehouseOrderIn> GetOrdersIn(DateTime dateStart, DateTime dateEnd, string searchText = null)
        {
            return new List<WarehouseOrderIn>();
        }

        #endregion

        #region Расходный складской ордер

        /// <summary>
        ///     Загрузить приходный складской ордер
        /// </summary>
        /// <param name="dc">Код документа</param>
        /// <returns></returns>
        public WarehouseOrderOut GetOrderOut(decimal dc)
        {
            WarehouseOrderOut result;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_26.SD_26")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_122")
                        .Include("TD_24.SD_170")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_1751")
                        .Include("TD_24.SD_2")
                        .Include("TD_24.SD_254")
                        .Include("TD_24.SD_27")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_3011")
                        .Include("TD_24.SD_3012")
                        .Include("TD_24.SD_303")
                        .Include("TD_24.SD_384")
                        .Include("TD_24.SD_43")
                        .Include("TD_24.SD_83")
                        .Include("TD_24.SD_831")
                        .Include("TD_24.SD_832")
                        .Include("TD_24.SD_84")
                        .Include("TD_24.TD_73")
                        .Include("TD_24.TD_9")
                        .Include("TD_24.TD_84")
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_241")
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    result = new WarehouseOrderOut(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            return result;
        }

        public WarehouseOrderOut NewOrderOut()
        {
            return new WarehouseOrderOut();
        }

        public WarehouseOrderOut NewOrderOutCopy(WarehouseOrderOut doc)
        {
            return NewOrderOutCopy(doc.DocCode);
        }

        public WarehouseOrderOut NewOrderOutCopy(decimal dc)
        {
            var old = GetOrderOut(dc);
            old.DocCode = -1;
            old.DD_IN_NUM = -1;
            old.DD_EXT_NUM = null;
            old.Date = DateTime.Today;
            old.CREATOR = GlobalOptions.UserInfo.Name;
            foreach (var r in old.Rows)
            {
                r.DOC_CODE = -1;
                r.State = RowStatus.NewRow;
            }

            var doc = new WarehouseOrderOut(old.Entity) {State = RowStatus.NewRow};
            return doc;
        }

        public WarehouseOrderOut NewOrderOutRecuisite(WarehouseOrderOut doc)
        {
            return NewOrderOutRecuisite(doc.DocCode);
        }

        public WarehouseOrderOut NewOrderOutRecuisite(decimal dc)
        {
            var old = GetOrderOut(dc);
            old.DocCode = -1;
            old.DD_IN_NUM = -1;
            old.DD_EXT_NUM = null;
            old.Date = DateTime.Today;
            old.CREATOR = GlobalOptions.UserInfo.Name;
            old.Rows.Clear();
            old.Entity.TD_24.Clear();
            var doc = new WarehouseOrderOut(old.Entity) {State = RowStatus.NewRow};
            return doc;
        }

        public decimal SaveOrderOut(WarehouseOrderOut doc, WarehouseOrderInSearchViewModel inSearchWindow = null)
        {
            var newDC = doc.DocCode;
            using (var ctx = GlobalOptions.GetEntities())
            {
                ctx.Database.CommandTimeout = 120;
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var d in doc.DeletedRows)
                        {
                            var oldrow = ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == d.DocCode && _.CODE == d.Code);
                            if (oldrow == null) continue;
                            ctx.TD_24.Remove(oldrow);
                        }

                        if (doc.DocCode == -1)
                        {
                            var guidId = Guid.NewGuid();
                            var inNum = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DD_IN_NUM) + 1 : 1;
                            newDC = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                            ctx.SD_24.Add(new SD_24
                            {
                                DOC_CODE = newDC,
                                DD_TYPE_DC = doc.Entity.DD_TYPE_DC,
                                DD_DATE = doc.Date,
                                DD_IN_NUM = inNum,
                                DD_EXT_NUM = doc.DD_EXT_NUM,
                                DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC,
                                DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC,
                                DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC,
                                DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC,
                                DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN,
                                DD_EXECUTED = doc.DD_EXECUTED,
                                DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN,
                                DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME,
                                DD_POLUCH_NAME = doc.DD_POLUCH_NAME,
                                DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN,
                                DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO,
                                DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO,
                                DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL,
                                DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL,
                                DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC,
                                DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC,
                                DD_SHABLON = doc.DD_SHABLON,
                                DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC,
                                DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC,
                                DD_TREB_NUM = doc.DD_TREB_NUM,
                                DD_TREB_DATE = doc.DD_TREB_DATE,
                                DD_TREB_DC = doc.DD_TREB_DC,
                                CREATOR = doc.CREATOR,
                                DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN,
                                DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC,
                                DD_SCHET = doc.DD_SCHET,
                                DD_DOVERENNOST = doc.DD_DOVERENNOST,
                                DD_NOSZATR_ID = doc.DD_NOSZATR_ID,
                                DD_NOSZATR_DC = doc.DD_NOSZATR_DC,
                                DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC,
                                DD_NOTES = doc.Note,
                                DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC,
                                DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE,
                                DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC,
                                DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE,
                                DD_SPOST_DC = doc.Entity.DD_SPOST_DC,
                                DD_SFACT_DC = doc.Entity.DD_SFACT_DC,
                                DD_VOZVRAT = doc.Entity.DD_VOZVRAT,
                                DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE,
                                DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE,
                                DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV,
                                DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG,
                                DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE,
                                DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR,
                                DD_VIEZD_DATE = doc.DD_VIEZD_DATE,
                                DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC,
                                DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE,
                                DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC,
                                TSTAMP = doc.TSTAMP,
                                OWNER_ID = doc.OWNER_ID,
                                OWNER_TEXT = doc.OWNER_TEXT,
                                CONSIGNEE_ID = doc.CONSIGNEE_ID,
                                CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT,
                                BUYER_ID = doc.BUYER_ID,
                                BUYER_TEXT = doc.BUYER_TEXT,
                                SHIPMENT_ID = doc.SHIPMENT_ID,
                                SHIPMENT_TEXT = doc.SHIPMENT_TEXT,
                                SUPPLIER_ID = doc.SUPPLIER_ID,
                                SUPPLIER_TEXT = doc.SUPPLIER_TEXT,
                                GRUZO_INFO_ID = doc.GRUZO_INFO_ID,
                                GROZO_REQUISITE = doc.GROZO_REQUISITE,
                                Id = guidId
                            });
                            if (doc.Rows.Count > 0)
                            {
                                var code = 1;
                                foreach (var item in doc.Rows)
                                {
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = item.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = item.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = item.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = item.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) item.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) item.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = item.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = item.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = item.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = item.DDT_CRS_DC,
                                        DDT_SFACT_DC = item.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = item.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) item.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) item.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = item.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = item.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = item.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = item.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = item.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = item.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = item.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = item.DDT_TREB_DC,
                                        DDT_TREB_CODE = item.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = item.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = item.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = item.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) item.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = item.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = item.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = item.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = item.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = item.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = item.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = item.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = item.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = item.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = item.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = item.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) item.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = item.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = item.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = item.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = item.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) item.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = item.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = item.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) item.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = item.DDT_OS_DC,
                                        DDT_GARANT_DC = item.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = item.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) item.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = item.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) item.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = item.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = item.DDT_DILER_DC,
                                        DDT_DILER_SUM = item.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = item.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = item.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = item.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = item.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = item.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = item.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = item.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = item.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = item.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = item.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = item.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = item.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = item.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = item.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) item.DDT_MEST_TARA,
                                        DDT_TARA_DC = item.DDT_TARA_DC,
                                        DDT_TARA_FLAG = item.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = item.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = item.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = item.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = item.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = item.DDT_CALC_UCHET_TAX_PRICE,
                                        TSTAMP = item.TSTAMP,
                                        Id = Guid.NewGuid(),
                                        DocId = guidId
                                    });
                                    code++;
                                }
                            }
                        }
                        else
                        {
                            var old = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            if (old == null) return doc.DocCode;
                            old.DD_TYPE_DC = doc.Entity.DD_TYPE_DC;
                            old.DD_DATE = doc.Date;
                            old.DD_IN_NUM = doc.DD_IN_NUM;
                            old.DD_EXT_NUM = doc.DD_EXT_NUM;
                            old.DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC;
                            old.DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC;
                            old.DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC;
                            old.DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC;
                            old.DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN;
                            old.DD_EXECUTED = doc.DD_EXECUTED;
                            old.DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN;
                            old.DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME;
                            old.DD_POLUCH_NAME = doc.DD_POLUCH_NAME;
                            old.DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN;
                            old.DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO;
                            old.DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO;
                            old.DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL;
                            old.DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL;
                            old.DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC;
                            old.DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC;
                            old.DD_SHABLON = doc.DD_SHABLON;
                            old.DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC;
                            old.DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC;
                            old.DD_TREB_NUM = doc.DD_TREB_NUM;
                            old.DD_TREB_DATE = doc.DD_TREB_DATE;
                            old.DD_TREB_DC = doc.DD_TREB_DC;
                            old.CREATOR = doc.CREATOR;
                            old.DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN;
                            old.DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC;
                            old.DD_SCHET = doc.DD_SCHET;
                            old.DD_DOVERENNOST = doc.DD_DOVERENNOST;
                            old.DD_NOSZATR_ID = doc.DD_NOSZATR_ID;
                            old.DD_NOSZATR_DC = doc.DD_NOSZATR_DC;
                            old.DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC;
                            old.DD_NOTES = doc.Note;
                            old.DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC;
                            old.DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE;
                            old.DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC;
                            old.DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE;
                            old.DD_SPOST_DC = doc.Entity.DD_SPOST_DC;
                            old.DD_SFACT_DC = doc.Entity.DD_SFACT_DC;
                            old.DD_VOZVRAT = doc.Entity.DD_VOZVRAT;
                            old.DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE;
                            old.DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE;
                            old.DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV;
                            old.DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG;
                            old.DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE;
                            old.DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR;
                            old.DD_VIEZD_DATE = doc.DD_VIEZD_DATE;
                            old.DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC;
                            old.DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE;
                            old.DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC;
                            old.TSTAMP = doc.TSTAMP;
                            old.OWNER_ID = doc.OWNER_ID;
                            old.OWNER_TEXT = doc.OWNER_TEXT;
                            old.CONSIGNEE_ID = doc.CONSIGNEE_ID;
                            old.CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT;
                            old.BUYER_ID = doc.BUYER_ID;
                            old.BUYER_TEXT = doc.BUYER_TEXT;
                            old.SHIPMENT_ID = doc.SHIPMENT_ID;
                            old.SHIPMENT_TEXT = doc.SHIPMENT_TEXT;
                            old.SUPPLIER_ID = doc.SUPPLIER_ID;
                            old.SUPPLIER_TEXT = doc.SUPPLIER_TEXT;
                            old.GRUZO_INFO_ID = doc.GRUZO_INFO_ID;
                            old.GROZO_REQUISITE = doc.GROZO_REQUISITE;
                            var codes = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                            var code = codes.Count > 0 ? codes.Max(_ => _.CODE) : 0;
                            foreach (var r in doc.Rows)
                            {
                                var oldrow = ctx.TD_24.FirstOrDefault(_ =>
                                    _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (oldrow == null)
                                {
                                    code++;
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = r.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = r.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = r.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = r.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = r.DDT_CRS_DC,
                                        DDT_SFACT_DC = r.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = r.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = r.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = r.DDT_TREB_DC,
                                        DDT_TREB_CODE = r.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = r.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = r.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = r.DDT_OS_DC,
                                        DDT_GARANT_DC = r.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = r.DDT_DILER_DC,
                                        DDT_DILER_SUM = r.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = r.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) r.DDT_MEST_TARA,
                                        DDT_TARA_DC = r.DDT_TARA_DC,
                                        DDT_TARA_FLAG = r.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = r.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE,
                                        Id = Guid.NewGuid(),
                                        DocId = old.Id
                                    });
                                }
                                else
                                {
                                    oldrow.DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD;
                                    oldrow.DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO;
                                    oldrow.DDT_KOL_RASHOD = r.DDT_KOL_RASHOD;
                                    oldrow.DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO;
                                    oldrow.DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD;
                                    oldrow.DDT_ED_IZM_DC = r.DDT_ED_IZM_DC;
                                    oldrow.DDT_SPOST_DC = r.DDT_SPOST_DC;
                                    oldrow.DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE;
                                    oldrow.DDT_CRS_DC = r.DDT_CRS_DC;
                                    oldrow.DDT_SFACT_DC = r.DDT_SFACT_DC;
                                    oldrow.DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE;
                                    oldrow.DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR;
                                    oldrow.DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV;
                                    oldrow.DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA;
                                    oldrow.DDT_TAX_CENA = r.DDT_TAX_CENA;
                                    oldrow.DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED;
                                    oldrow.DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT;
                                    oldrow.DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA;
                                    oldrow.DDT_FACT_CENA = r.DDT_FACT_CENA;
                                    oldrow.DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED;
                                    oldrow.DDT_TREB_DC = r.DDT_TREB_DC;
                                    oldrow.DDT_TREB_CODE = r.DDT_TREB_CODE;
                                    oldrow.DDT_NOSZATR_DC = r.DDT_NOSZATR_DC;
                                    oldrow.DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE;
                                    oldrow.DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC;
                                    oldrow.DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD;
                                    oldrow.DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA;
                                    oldrow.DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA;
                                    oldrow.DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA;
                                    oldrow.DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC;
                                    oldrow.DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE;
                                    oldrow.DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE;
                                    oldrow.DDT_SHPZ_DC = r.DDT_SHPZ_DC;
                                    oldrow.DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA;
                                    oldrow.DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE;
                                    oldrow.DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE;
                                    oldrow.DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC;
                                    oldrow.DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE;
                                    oldrow.DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC;
                                    oldrow.DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC;
                                    oldrow.DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE;
                                    oldrow.DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC;
                                    oldrow.DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE;
                                    oldrow.DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA;
                                    oldrow.DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN;
                                    oldrow.DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE;
                                    oldrow.DDT_OS_DC = r.DDT_OS_DC;
                                    oldrow.DDT_GARANT_DC = r.DDT_GARANT_DC;
                                    oldrow.DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE;
                                    oldrow.DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM;
                                    oldrow.DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE;
                                    oldrow.DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE;
                                    oldrow.DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE;
                                    oldrow.DDT_DILER_DC = r.DDT_DILER_DC;
                                    oldrow.DDT_DILER_SUM = r.DDT_DILER_SUM;
                                    oldrow.DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE;
                                    oldrow.DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE;
                                    oldrow.DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER;
                                    oldrow.DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC;
                                    oldrow.DDT_RASHOD_DATE = r.DDT_RASHOD_DATE;
                                    oldrow.DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR;
                                    oldrow.DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE;
                                    oldrow.DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC;
                                    oldrow.DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC;
                                    oldrow.DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE;
                                    oldrow.DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE;
                                    oldrow.DDT_MEST_TARA = (double?) r.DDT_MEST_TARA;
                                    oldrow.DDT_TARA_DC = r.DDT_TARA_DC;
                                    oldrow.DDT_TARA_FLAG = r.DDT_TARA_FLAG;
                                    oldrow.DDT_PART_NUMBER = r.DDT_PART_NUMBER;
                                    oldrow.DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE;
                                    oldrow.DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE;
                                    oldrow.DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE;
                                    oldrow.DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE;
                                }
                            }
                        }

                        ctx.SaveChanges();
                        var calc = new NomenklCostMediumSliding(ctx);
                        foreach (var n in doc.Rows.Select(_ => _.Nomenkl.DocCode))
                        {
                            var ops = calc.GetOperations(n);
                            if (ops != null && ops.Count > 0)
                                calc.Save(ops);
                            var c = NomenklCalculationManager.NomenklRemain(ctx, doc.Date, n,
                                // ReSharper disable once PossibleInvalidOperationException
                                (decimal) doc.Entity.DD_SKLAD_OTPR_DC);
                            if (c < 0)
                            {
                                transaction.Rollback();
                                var nom = MainReferences.GetNomenkl(n);
                                WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                          // ReSharper disable once PossibleInvalidOperationException
                                                          $"склад {MainReferences.Warehouses[(decimal) doc.Entity.DD_SKLAD_OTPR_DC]} в кол-ве {c} ",
                                    "Отрицательные остатки", MessageBoxImage.Error);
                                return -1;
                            }
                        }

                        transaction.Commit();
                        doc.myState = RowStatus.NotEdited;
                        foreach (var r in doc.Rows) r.myState = RowStatus.NotEdited;
                        doc.DeletedRows.Clear();
                        return newDC;
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
        }

        public void DeleteOrderOut(WarehouseOrderOut doc, WarehouseOrderInSearchViewModel inSearchWindow = null)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var rows = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                        if (rows.Count > 0)
                            foreach (var r in rows)
                            {
                                var oldrow =
                                    ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == r.DOC_CODE && _.CODE == r.CODE);
                                if (oldrow != null)
                                    ctx.TD_24.Remove(oldrow);
                            }

                        var olddoc = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                        if (olddoc != null)
                            ctx.SD_24.Remove(olddoc);
                        ctx.SaveChanges();
                        transaction.Commit();
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

        public List<WarehouseOrderOut> GetOrdersOut(string searchText = null)
        {
            return new List<WarehouseOrderOut>();
        }

        public List<WarehouseOrderOut> GetOrdersOut(DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            return new List<WarehouseOrderOut>();
        }

        #endregion

        #region Расходная накладная

        /// <summary>
        ///     Загрузить расходную накладную
        /// </summary>
        /// <param name="dc">Код документа</param>
        /// <returns></returns>
        public Waybill GetWaybill(decimal dc)
        {
            Waybill result;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Include(_ => _.SD_84)
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_26.SD_26")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_122")
                        .Include("TD_24.SD_170")
                        .Include("TD_24.SD_175")
                        .Include("TD_24.SD_1751")
                        .Include("TD_24.SD_2")
                        .Include("TD_24.SD_254")
                        .Include("TD_24.SD_27")
                        .Include("TD_24.SD_301")
                        .Include("TD_24.SD_3011")
                        .Include("TD_24.SD_3012")
                        .Include("TD_24.SD_303")
                        .Include("TD_24.SD_384")
                        .Include("TD_24.SD_43")
                        .Include("TD_24.SD_83")
                        .Include("TD_24.SD_831")
                        .Include("TD_24.SD_832")
                        .Include("TD_24.SD_84")
                        .Include("TD_24.TD_73")
                        .Include("TD_24.TD_9")
                        .Include("TD_24.TD_84")
                        .Include("TD_24.TD_26")
                        .Include("TD_24.TD_241")
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    result = new Waybill(data) {myState = RowStatus.NotEdited};
                    foreach (var r in result.Rows)
                        r.myState = RowStatus.NotEdited;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            return result;
        }

        public Waybill NewWaybill()
        {
            return new Waybill
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
        }

        public Waybill NewWaybillCopy(Waybill doc)
        {
            var res = new Waybill(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            foreach (var r in res.Rows)
            {
                r.DocCode = -1;
                r.State = RowStatus.NewRow;
            }

            return res;
        }

        public Waybill NewWaybillCopy(decimal dc)
        {
            var doc = GetWaybill(dc);
            var res = new Waybill(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            foreach (var r in res.Rows)
            {
                r.DocCode = -1;
                r.State = RowStatus.NewRow;
            }

            return res;
        }

        public Waybill NewWaybillRecuisite(Waybill doc)
        {
            var ret = new Waybill(doc.Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            ret.Rows.Clear();
            return ret;
        }

        public Waybill NewWaybillRecuisite(decimal dc)
        {
            var ret = new Waybill(GetWaybill(dc).Entity)
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                Date = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name
            };
            ret.Rows.Clear();
            return ret;
        }

        public decimal SaveWaybill(Waybill doc, WaybillSearchViewModel inSearchWindow = null)
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
                            var oldrow = ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == d.DocCode && _.CODE == d.Code);
                            if (oldrow == null) continue;
                            ctx.TD_24.Remove(oldrow);
                        }

                        if (doc.DocCode == -1)
                        {
                            var guidId = Guid.NewGuid();
                            var inNum = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DD_IN_NUM) + 1 : 1;
                            newDC = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                            ctx.SD_24.Add(new SD_24
                            {
                                DOC_CODE = newDC,
                                DD_TYPE_DC = doc.Entity.DD_TYPE_DC,
                                DD_DATE = doc.Date,
                                DD_IN_NUM = inNum,
                                DD_EXT_NUM = doc.DD_EXT_NUM,
                                DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC,
                                DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC,
                                DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC,
                                DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC,
                                DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN,
                                DD_EXECUTED = doc.DD_EXECUTED,
                                DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN,
                                DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME,
                                DD_POLUCH_NAME = doc.DD_POLUCH_NAME,
                                DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN,
                                DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO,
                                DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO,
                                DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL,
                                DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL,
                                DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC,
                                DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC,
                                DD_SHABLON = doc.DD_SHABLON,
                                DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC,
                                DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC,
                                DD_TREB_NUM = doc.DD_TREB_NUM,
                                DD_TREB_DATE = doc.DD_TREB_DATE,
                                DD_TREB_DC = doc.DD_TREB_DC,
                                CREATOR = doc.CREATOR,
                                DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN,
                                DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC,
                                DD_SCHET = doc.DD_SCHET,
                                DD_DOVERENNOST = doc.DD_DOVERENNOST,
                                DD_NOSZATR_ID = doc.DD_NOSZATR_ID,
                                DD_NOSZATR_DC = doc.DD_NOSZATR_DC,
                                DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC,
                                DD_NOTES = doc.Note,
                                DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC,
                                DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE,
                                DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC,
                                DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE,
                                DD_SPOST_DC = doc.Entity.DD_SPOST_DC,
                                DD_SFACT_DC = doc.Entity.DD_SFACT_DC,
                                DD_VOZVRAT = doc.Entity.DD_VOZVRAT,
                                DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE,
                                DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE,
                                DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV,
                                DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG,
                                DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE,
                                DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR,
                                DD_VIEZD_DATE = doc.DD_VIEZD_DATE,
                                DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC,
                                DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE,
                                DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC,
                                TSTAMP = doc.TSTAMP,
                                OWNER_ID = doc.OWNER_ID,
                                OWNER_TEXT = doc.OWNER_TEXT,
                                CONSIGNEE_ID = doc.CONSIGNEE_ID,
                                CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT,
                                BUYER_ID = doc.BUYER_ID,
                                BUYER_TEXT = doc.BUYER_TEXT,
                                SHIPMENT_ID = doc.SHIPMENT_ID,
                                SHIPMENT_TEXT = doc.SHIPMENT_TEXT,
                                SUPPLIER_ID = doc.SUPPLIER_ID,
                                SUPPLIER_TEXT = doc.SUPPLIER_TEXT,
                                GRUZO_INFO_ID = doc.GRUZO_INFO_ID,
                                GROZO_REQUISITE = doc.GROZO_REQUISITE,
                                Id = guidId
                            });
                            if (doc.Rows.Count > 0)
                            {
                                var code = 1;
                                foreach (var item in doc.Rows)
                                {
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = item.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = item.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = item.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = item.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) item.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) item.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = item.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = item.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = item.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = item.DDT_CRS_DC,
                                        DDT_SFACT_DC = item.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = item.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) item.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) item.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = item.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = item.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = item.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = item.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = item.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = item.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = item.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = item.DDT_TREB_DC,
                                        DDT_TREB_CODE = item.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = item.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = item.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = item.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) item.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = item.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = item.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = item.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = item.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = item.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = item.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = item.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = item.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = item.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = item.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = item.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) item.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = item.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = item.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = item.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = item.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) item.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = item.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = item.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) item.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = item.DDT_OS_DC,
                                        DDT_GARANT_DC = item.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = item.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) item.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = item.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) item.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = item.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = item.DDT_DILER_DC,
                                        DDT_DILER_SUM = item.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = item.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = item.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = item.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = item.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = item.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = item.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = item.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = item.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = item.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = item.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = item.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = item.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = item.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = item.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) item.DDT_MEST_TARA,
                                        DDT_TARA_DC = item.DDT_TARA_DC,
                                        DDT_TARA_FLAG = item.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = item.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = item.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = item.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = item.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = item.DDT_CALC_UCHET_TAX_PRICE,
                                        TSTAMP = item.TSTAMP,
                                        Id = Guid.NewGuid(),
                                        DocId = guidId
                                    });
                                    code++;
                                }
                            }
                        }
                        else
                        {
                            var old = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            if (old == null) return doc.DocCode;
                            old.DD_TYPE_DC = doc.Entity.DD_TYPE_DC;
                            old.DD_DATE = doc.Date;
                            old.DD_IN_NUM = doc.DD_IN_NUM;
                            old.DD_EXT_NUM = doc.DD_EXT_NUM;
                            old.DD_SKLAD_OTPR_DC = doc.Entity.DD_SKLAD_OTPR_DC;
                            old.DD_SKLAD_POL_DC = doc.Entity.DD_SKLAD_POL_DC;
                            old.DD_KONTR_OTPR_DC = doc.Entity.DD_KONTR_OTPR_DC;
                            old.DD_KONTR_POL_DC = doc.Entity.DD_KONTR_POL_DC;
                            old.DD_KLADOV_TN = doc.Entity.DD_KLADOV_TN;
                            old.DD_EXECUTED = doc.DD_EXECUTED;
                            old.DD_POLUCHATEL_TN = doc.DD_POLUCHATEL_TN;
                            old.DD_OTRPAV_NAME = doc.DD_OTRPAV_NAME;
                            old.DD_POLUCH_NAME = doc.DD_POLUCH_NAME;
                            old.DD_KTO_SDAL_TN = doc.DD_KTO_SDAL_TN;
                            old.DD_KOMU_PEREDANO = doc.DD_KOMU_PEREDANO;
                            old.DD_OT_KOGO_POLUCHENO = doc.DD_OT_KOGO_POLUCHENO;
                            old.DD_GRUZOOTPRAVITEL = doc.DD_GRUZOOTPRAVITEL;
                            old.DD_GRUZOPOLUCHATEL = doc.DD_GRUZOPOLUCHATEL;
                            old.DD_OTPUSK_NA_SKLAD_DC = doc.Entity.DD_OTPUSK_NA_SKLAD_DC;
                            old.DD_PRIHOD_SO_SKLADA_DC = doc.Entity.DD_PRIHOD_SO_SKLADA_DC;
                            old.DD_SHABLON = doc.DD_SHABLON;
                            old.DD_VED_VIDACH_DC = doc.DD_VED_VIDACH_DC;
                            old.DD_PERIOD_DC = doc.Entity.DD_PERIOD_DC;
                            old.DD_TREB_NUM = doc.DD_TREB_NUM;
                            old.DD_TREB_DATE = doc.DD_TREB_DATE;
                            old.DD_TREB_DC = doc.DD_TREB_DC;
                            old.CREATOR = doc.CREATOR;
                            old.DD_PODTVERZHDEN = doc.DD_PODTVERZHDEN;
                            old.DD_OSN_OTGR_DC = doc.DD_OSN_OTGR_DC;
                            old.DD_SCHET = doc.DD_SCHET;
                            old.DD_DOVERENNOST = doc.DD_DOVERENNOST;
                            old.DD_NOSZATR_ID = doc.DD_NOSZATR_ID;
                            old.DD_NOSZATR_DC = doc.DD_NOSZATR_DC;
                            old.DD_DOGOVOR_POKUPKI_DC = doc.DD_DOGOVOR_POKUPKI_DC;
                            old.DD_NOTES = doc.Note;
                            old.DD_KONTR_CRS_DC = doc.DD_KONTR_CRS_DC;
                            old.DD_KONTR_CRS_RATE = doc.DD_KONTR_CRS_RATE;
                            old.DD_UCHET_VALUTA_DC = doc.DD_UCHET_VALUTA_DC;
                            old.DD_UCHET_VALUTA_RATE = doc.DD_UCHET_VALUTA_RATE;
                            old.DD_SPOST_DC = doc.Entity.DD_SPOST_DC;
                            old.DD_SFACT_DC = doc.Entity.DD_SFACT_DC;
                            old.DD_VOZVRAT = doc.Entity.DD_VOZVRAT;
                            old.DD_OTPRAV_TYPE = doc.DD_OTPRAV_TYPE;
                            old.DD_POLUCH_TYPE = doc.DD_POLUCH_TYPE;
                            old.DD_LISTOV_SERVIFICATOV = doc.DD_LISTOV_SERVIFICATOV;
                            old.DD_VIEZD_FLAG = doc.DD_VIEZD_FLAG;
                            old.DD_VIEZD_MASHINE = doc.DD_VIEZD_MASHINE;
                            old.DD_VIEZD_CREATOR = doc.DD_VIEZD_CREATOR;
                            old.DD_VIEZD_DATE = doc.DD_VIEZD_DATE;
                            old.DD_KONTR_POL_FILIAL_DC = doc.DD_KONTR_POL_FILIAL_DC;
                            old.DD_KONTR_POL_FILIAL_CODE = doc.DD_KONTR_POL_FILIAL_CODE;
                            old.DD_PROZV_PROCESS_DC = doc.DD_PROZV_PROCESS_DC;
                            old.TSTAMP = doc.TSTAMP;
                            old.OWNER_ID = doc.OWNER_ID;
                            old.OWNER_TEXT = doc.OWNER_TEXT;
                            old.CONSIGNEE_ID = doc.CONSIGNEE_ID;
                            old.CONSIGNEE_TEXT = doc.CONSIGNEE_TEXT;
                            old.BUYER_ID = doc.BUYER_ID;
                            old.BUYER_TEXT = doc.BUYER_TEXT;
                            old.SHIPMENT_ID = doc.SHIPMENT_ID;
                            old.SHIPMENT_TEXT = doc.SHIPMENT_TEXT;
                            old.SUPPLIER_ID = doc.SUPPLIER_ID;
                            old.SUPPLIER_TEXT = doc.SUPPLIER_TEXT;
                            old.GRUZO_INFO_ID = doc.GRUZO_INFO_ID;
                            old.GROZO_REQUISITE = doc.GROZO_REQUISITE;
                            var codes = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                            var code = codes.Count > 0 ? codes.Max(_ => _.CODE) : 0;
                            foreach (var r in doc.Rows)
                            {
                                var oldrow = ctx.TD_24.FirstOrDefault(_ =>
                                    _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (oldrow == null)
                                {
                                    code++;
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = newDC,
                                        CODE = code,
                                        DDT_NOMENKL_DC = r.DDT_NOMENKL_DC,
                                        DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD,
                                        DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO,
                                        DDT_KOL_RASHOD = r.DDT_KOL_RASHOD,
                                        DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO,
                                        DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD,
                                        DDT_ED_IZM_DC = r.DDT_ED_IZM_DC,
                                        DDT_SPOST_DC = r.DDT_SPOST_DC,
                                        DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE,
                                        DDT_CRS_DC = r.DDT_CRS_DC,
                                        DDT_SFACT_DC = r.DDT_SFACT_DC,
                                        DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE,
                                        DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR,
                                        DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV,
                                        DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA,
                                        DDT_TAX_CENA = r.DDT_TAX_CENA,
                                        DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED,
                                        DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT,
                                        DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA,
                                        DDT_FACT_CENA = r.DDT_FACT_CENA,
                                        DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED,
                                        DDT_TREB_DC = r.DDT_TREB_DC,
                                        DDT_TREB_CODE = r.DDT_TREB_CODE,
                                        DDT_NOSZATR_DC = r.DDT_NOSZATR_DC,
                                        DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE,
                                        DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC,
                                        DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD,
                                        DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA,
                                        DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA,
                                        DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA,
                                        DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC,
                                        DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE,
                                        DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE,
                                        DDT_SHPZ_DC = r.DDT_SHPZ_DC,
                                        DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA,
                                        DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE,
                                        DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE,
                                        DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC,
                                        DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE,
                                        DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC,
                                        DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC,
                                        DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE,
                                        DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC,
                                        DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE,
                                        DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA,
                                        DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN,
                                        DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE,
                                        DDT_OS_DC = r.DDT_OS_DC,
                                        DDT_GARANT_DC = r.DDT_GARANT_DC,
                                        DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE,
                                        DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM,
                                        DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE,
                                        DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE,
                                        DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE,
                                        DDT_DILER_DC = r.DDT_DILER_DC,
                                        DDT_DILER_SUM = r.DDT_DILER_SUM,
                                        DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE,
                                        DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE,
                                        DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER,
                                        DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC,
                                        DDT_RASHOD_DATE = r.DDT_RASHOD_DATE,
                                        DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR,
                                        DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC,
                                        DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC,
                                        DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM,
                                        DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE,
                                        DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC,
                                        DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC,
                                        DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE,
                                        DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE,
                                        DDT_MEST_TARA = (double?) r.DDT_MEST_TARA,
                                        DDT_TARA_DC = r.DDT_TARA_DC,
                                        DDT_TARA_FLAG = r.DDT_TARA_FLAG,
                                        DDT_PART_NUMBER = r.DDT_PART_NUMBER,
                                        DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE,
                                        DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE,
                                        DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE,
                                        DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE,
                                        Id = Guid.NewGuid(),
                                        DocId = old.Id
                                    });
                                }
                                else
                                {
                                    oldrow.DDT_KOL_PRIHOD = r.DDT_KOL_PRIHOD;
                                    oldrow.DDT_KOL_ZATREBOVANO = r.DDT_KOL_ZATREBOVANO;
                                    oldrow.DDT_KOL_RASHOD = r.DDT_KOL_RASHOD;
                                    oldrow.DDT_KOL_PODTVERZHDENO = (double?) r.DDT_KOL_PODTVERZHDENO;
                                    oldrow.DDT_KOL_SHAB_PRIHOD = (double?) r.DDT_KOL_SHAB_PRIHOD;
                                    oldrow.DDT_ED_IZM_DC = r.DDT_ED_IZM_DC;
                                    oldrow.DDT_SPOST_DC = r.DDT_SPOST_DC;
                                    oldrow.DDT_SPOST_ROW_CODE = r.DDT_SPOST_ROW_CODE;
                                    oldrow.DDT_CRS_DC = r.DDT_CRS_DC;
                                    oldrow.DDT_SFACT_DC = r.DDT_SFACT_DC;
                                    oldrow.DDT_SFACT_ROW_CODE = r.DDT_SFACT_ROW_CODE;
                                    oldrow.DDT_OSTAT_STAR = (double?) r.DDT_OSTAT_STAR;
                                    oldrow.DDT_OSTAT_NOV = (double?) r.DDT_OSTAT_NOV;
                                    oldrow.DDT_TAX_CRS_CENA = r.DDT_TAX_CRS_CENA;
                                    oldrow.DDT_TAX_CENA = r.DDT_TAX_CENA;
                                    oldrow.DDT_TAX_EXECUTED = r.DDT_TAX_EXECUTED;
                                    oldrow.DDT_TAX_IN_SFACT = r.DDT_TAX_IN_SFACT;
                                    oldrow.DDT_FACT_CRS_CENA = r.DDT_FACT_CRS_CENA;
                                    oldrow.DDT_FACT_CENA = r.DDT_FACT_CENA;
                                    oldrow.DDT_FACT_EXECUTED = r.DDT_FACT_EXECUTED;
                                    oldrow.DDT_TREB_DC = r.DDT_TREB_DC;
                                    oldrow.DDT_TREB_CODE = r.DDT_TREB_CODE;
                                    oldrow.DDT_NOSZATR_DC = r.DDT_NOSZATR_DC;
                                    oldrow.DDT_NOSZATR_ROW_CODE = r.DDT_NOSZATR_ROW_CODE;
                                    oldrow.DDT_POST_ED_IZM_DC = r.DDT_POST_ED_IZM_DC;
                                    oldrow.DDT_KOL_POST_PRIHOD = (double?) r.DDT_KOL_POST_PRIHOD;
                                    oldrow.DDT_PRICHINA_SPISANIA = r.DDT_PRICHINA_SPISANIA;
                                    oldrow.DDT_VOZVRAT_TREBOVINIA = r.DDT_VOZVRAT_TREBOVINIA;
                                    oldrow.DDT_VOZVRAT_PRICHINA = r.DDT_VOZVRAT_PRICHINA;
                                    oldrow.DDT_TOV_CHECK_DC = r.DDT_TOV_CHECK_DC;
                                    oldrow.DDT_TOV_CHECK_CODE = r.DDT_TOV_CHECK_CODE;
                                    oldrow.DDT_ACT_GP_PROD_CODE = r.DDT_ACT_GP_PROD_CODE;
                                    oldrow.DDT_SHPZ_DC = r.DDT_SHPZ_DC;
                                    oldrow.DDT_KONTR_CRS_SUMMA = r.DDT_KONTR_CRS_SUMMA;
                                    oldrow.DDT_SUMMA_V_UCHET_VALUTE = r.DDT_SUMMA_V_UCHET_VALUTE;
                                    oldrow.DDT_CENA_V_UCHET_VALUTE = r.DDT_CENA_V_UCHET_VALUTE;
                                    oldrow.DDT_SKLAD_OTPR_DC = r.DDT_SKLAD_OTPR_DC;
                                    oldrow.DDT_NOM_CRS_RATE = (double?) r.DDT_NOM_CRS_RATE;
                                    oldrow.DDT_PROIZV_PLAN_DC = r.DDT_PROIZV_PLAN_DC;
                                    oldrow.DDT_RASH_ORD_DC = r.DDT_RASH_ORD_DC;
                                    oldrow.DDT_RASH_ORD_CODE = r.DDT_RASH_ORD_CODE;
                                    oldrow.DDT_VOZVR_OTGR_CSR_DC = r.DDT_VOZVR_OTGR_CSR_DC;
                                    oldrow.DDT_VOZVR_UCH_CRS_RATE = (double?) r.DDT_VOZVR_UCH_CRS_RATE;
                                    oldrow.DDT_VOZVR_OTGR_CRS_TAX_CENA = r.DDT_VOZVR_OTGR_CRS_TAX_CENA;
                                    oldrow.DDT_SBORSCHIK_TN = r.DDT_SBORSCHIK_TN;
                                    oldrow.DDT_KOL_IN_ONE = (double?) r.DDT_KOL_IN_ONE;
                                    oldrow.DDT_OS_DC = r.DDT_OS_DC;
                                    oldrow.DDT_GARANT_DC = r.DDT_GARANT_DC;
                                    oldrow.DDT_GARANT_ROW_CODE = r.DDT_GARANT_ROW_CODE;
                                    oldrow.DDT_ACT_RAZ_PROC_STOIM = (double?) r.DDT_ACT_RAZ_PROC_STOIM;
                                    oldrow.DDT_PROIZV_PLAN_ROW_CODE = r.DDT_PROIZV_PLAN_ROW_CODE;
                                    oldrow.DDT_APGP_TO_EXECUTE = (double?) r.DDT_APGP_TO_EXECUTE;
                                    oldrow.DDT_APGP_NOT_EXECUTE = r.DDT_APGP_NOT_EXECUTE;
                                    oldrow.DDT_DILER_DC = r.DDT_DILER_DC;
                                    oldrow.DDT_DILER_SUM = r.DDT_DILER_SUM;
                                    oldrow.DDT_VHOD_KONTR_EXECUTE = r.DDT_VHOD_KONTR_EXECUTE;
                                    oldrow.DDT_VHOD_KONTR_NOTE = r.DDT_VHOD_KONTR_NOTE;
                                    oldrow.DDT_VHOD_KONTR_USER = r.DDT_VHOD_KONTR_USER;
                                    oldrow.DDT_ZAIAVKA_DC = r.DDT_ZAIAVKA_DC;
                                    oldrow.DDT_RASHOD_DATE = r.DDT_RASHOD_DATE;
                                    oldrow.DDT_VOZVRAT_TREB_CREATOR = r.DDT_VOZVRAT_TREB_CREATOR;
                                    oldrow.DDT_VOZVRAT_SFACT_OPLAT_DC = r.DDT_VOZVRAT_SFACT_OPLAT_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_DC = r.DDT_VOZVRAT_SFACT_CRS_DC;
                                    oldrow.DDT_VOZVRAT_SFACT_SUM = r.DDT_VOZVRAT_SFACT_SUM;
                                    oldrow.DDT_VOZVRAT_SFACT_CRS_RATE = r.DDT_VOZVRAT_SFACT_CRS_RATE;
                                    oldrow.DDT_PROIZV_PROC_NOMENKL_DC = r.DDT_PROIZV_PROC_NOMENKL_DC;
                                    oldrow.DDT_PROIZV_PARTIA_DC = r.DDT_PROIZV_PARTIA_DC;
                                    oldrow.DDT_PROIZV_PARTIA_CODE = r.DDT_PROIZV_PARTIA_CODE;
                                    oldrow.DDT_DAVAL_SIRIE = r.DDT_DAVAL_SIRIE;
                                    oldrow.DDT_MEST_TARA = (double?) r.DDT_MEST_TARA;
                                    oldrow.DDT_TARA_DC = r.DDT_TARA_DC;
                                    oldrow.DDT_TARA_FLAG = r.DDT_TARA_FLAG;
                                    oldrow.DDT_PART_NUMBER = r.DDT_PART_NUMBER;
                                    oldrow.DDT_VNPER_UD_POINT_UCHVAL_PRICE = r.DDT_VNPER_UD_POINT_UCHVAL_PRICE;
                                    oldrow.DDT_SKIDKA_CREDIT_NOTE = r.DDT_SKIDKA_CREDIT_NOTE;
                                    oldrow.DDT_CALC_NOM_TAX_PRICE = r.DDT_CALC_NOM_TAX_PRICE;
                                    oldrow.DDT_CALC_UCHET_TAX_PRICE = r.DDT_CALC_UCHET_TAX_PRICE;
                                }
                            }
                        }

                        ctx.SaveChanges();
                        var nomDCList = new List<decimal>();
                        foreach (var n in doc.Rows.Select(_ => _.Nomenkl.DocCode)) nomDCList.Add(n);
                        NomenklManager.RecalcPrice(nomDCList, ctx);
                        transaction.Commit();
                        doc.myState = RowStatus.NotEdited;
                        foreach (var r in doc.Rows) r.myState = RowStatus.NotEdited;
                        doc.DeletedRows.Clear();
                        return newDC;
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
        }

        public void DeleteWaybill(Waybill doc, WaybillSearchViewModel inSearchWindow = null)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var rows = ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                        if (rows.Count > 0)
                            foreach (var r in rows)
                            {
                                var oldrow =
                                    ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == r.DOC_CODE && _.CODE == r.CODE);
                                if (oldrow != null)
                                    ctx.TD_24.Remove(oldrow);
                            }

                        var olddoc = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                        if (olddoc != null)
                            ctx.SD_24.Remove(olddoc);
                        ctx.SaveChanges();
                        transaction.Commit();
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

        public List<Waybill> GetWaybills(string searchText = null)
        {
            return new List<Waybill>();
        }

        public List<Waybill> GetWaybills(DateTime dateStart, DateTime dateEnd,
            string searchText = null)
        {
            var res = new List<Waybill>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_24
                    .Include(_ => _.TD_24)
                    .Include("TD_24.TD_26")
                    .Include("TD_24.TD_26.SD_26")
                    .Include("TD_24.SD_175")
                    .Include("TD_24.SD_301")
                    .Include("TD_24.SD_122")
                    .Include("TD_24.SD_170")
                    .Include("TD_24.SD_175")
                    .Include("TD_24.SD_1751")
                    .Include("TD_24.SD_2")
                    .Include("TD_24.SD_254")
                    .Include("TD_24.SD_27")
                    .Include("TD_24.SD_301")
                    .Include("TD_24.SD_3011")
                    .Include("TD_24.SD_3012")
                    .Include("TD_24.SD_303")
                    .Include("TD_24.SD_384")
                    .Include("TD_24.SD_43")
                    .Include("TD_24.SD_83")
                    .Include("TD_24.SD_831")
                    .Include("TD_24.SD_832")
                    .Include("TD_24.SD_84")
                    .Include("TD_24.TD_73")
                    .Include("TD_24.TD_9")
                    .Include("TD_24.TD_84")
                    .Include("TD_24.TD_26")
                    .Include("TD_24.TD_241")
                    .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd && _.DD_TYPE_DC == 2010000012).ToList();
                foreach (var d in data) res.Add(new Waybill(d));
            }

            return res;
        }

        #endregion`
    }
}