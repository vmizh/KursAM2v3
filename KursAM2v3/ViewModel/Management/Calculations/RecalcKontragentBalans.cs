using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Core;
using Core.Finance;
using Core.WindowsManager;
using Data;
using Helper;

namespace KursAM2.ViewModel.Management.Calculations
{
    public class KontrBalansRow
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string DOC_NAME { set; get; }
        public string DOC_NUM { set; get; }
        public DateTime DOC_DATE { set; get; }
        public double? CRS_KONTR_IN { set; get; }
        public double? CRS_KONTR_OUT { set; get; }
        public string KONTR_CRS_NAME { set; get; }
        public decimal KONTR_CRS_DC { set; get; }
        public decimal DOC_DC { set; get; }
        public int? DOC_ROW_CODE { set; get; }
        public int? DOC_TYPE_CODE { set; get; }
        public double CRS_OPER_IN { set; get; }
        public double CRS_OPER_OUT { set; get; }
        public string OPER_CRS_NAME { set; get; }
        public decimal OPER_CRS_DC { set; get; }
        public double OPER_CRS_RATE { set; get; }

        public double UCH_CRS_RATE { set; get; }
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    /// <summary>
    ///     расчет для баланса контрагентов
    /// </summary>
    public static class RecalcKontragentBalans
    {
        private static List<KONTR_BALANS_OPER_ARC> GetOperationsOld(decimal kontrDC, DateTime start, DateTime end)
        {
            var ret = new List<KONTR_BALANS_OPER_ARC>();
            using (var ent = GlobalOptions.GetEntities())
            {

                var data = ent.H184000_DVIZH_LIC_SCHET_KONTR_TABLE(kontrDC, start, end).ToList();
                ret.AddRange(
                    data
                        .Select(d => new KONTR_BALANS_OPER_ARC
                        {
                            DOC_NAME = d.DOC_NAME,
                            DOC_NUM = d.DOC_NUM,
                            DOC_DATE = Convert.ToDateTime(d.DOC_DATE),
                            CRS_KONTR_IN = Convert.ToDouble(d.CRS_KONTR_IN),
                            CRS_KONTR_OUT = Convert.ToDouble(d.CRS_KONTR_OUT),
                            DOC_DC = Convert.ToDecimal(d.DOC_DC),
                            DOC_ROW_CODE = Convert.ToInt32(d.DOC_ROW_CODE),
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = Convert.ToInt32(d.DOC_TYPE_CODE),
                            CRS_OPER_IN = Convert.ToDouble(d.CRS_OPER_IN),
                            CRS_OPER_OUT = Convert.ToDouble(d.CRS_OPER_OUT),
                            OPER_CRS_DC = Convert.ToDecimal(d.OPER_CRS_DC),
                            OPER_CRS_RATE = Convert.ToDouble(d.OPER_CRS_RATE),
                            UCH_CRS_RATE = Convert.ToDouble(d.UCH_CRS_RATE),
                            ID = Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty),
                            DOC_EXT_NUM = d.DOC_EXT_NUM
                        }));
            }


            return ret;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static List<KONTR_BALANS_OPER_ARC> GetOperations(decimal kontrDC, DateTime start, DateTime end)
        {
            var operations = new List<KONTR_BALANS_OPER_ARC>();
            using (var ent = GlobalOptions.GetEntities())
            {
                var kontr =
                    ent.SD_43.Include(_ => _.SD_301)
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.DOC_CODE == kontrDC);
                if (kontr == null)
                    throw new Exception($"Контрагент {kontrDC} не найден. RecalcKontragentBalans.CalcBalans");

                // вступительный остаток
                if (kontr.START_BALANS != null && kontr.START_BALANS >= start && kontr.START_BALANS <= end)
                    operations.Add(new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = " На начало учета",
                        DOC_NUM = null,
                        DOC_DATE = (DateTime) kontr.START_BALANS,
                        CRS_KONTR_IN = (double) ((kontr.START_SUMMA ?? 0) <= 0 ? -(kontr.START_SUMMA ?? 0) : 0),
                        CRS_KONTR_OUT = (double) ((kontr.START_SUMMA ?? 0) > 0 ? kontr.START_SUMMA ?? 0 : 0),
                        DOC_DC = 0,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.NotType,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = kontr.SD_301.DOC_CODE,
                        OPER_CRS_RATE = 0,
                        UCH_CRS_RATE = 0,
                        DOC_EXT_NUM = null
                    });

                // банковские операции
                foreach (var op in ent.TD_101.Include(_ => _.SD_101)
                    .Where(
                        _ =>
                            _.VVT_KONTRAGENT == kontrDC && _.SD_101.VV_STOP_DATE >= start &&
                            _.SD_101.VV_STOP_DATE <= end))
                    operations.Add(new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = "Банковская выписка",
                        DOC_NUM = op.VVT_DOC_NUM,
                        DOC_DATE = op.SD_101.VV_STOP_DATE,
                        CRS_KONTR_IN =
                            (double)
                            ((op.VVT_VAL_PRIHOD ?? 0) > (op.VVT_VAL_RASHOD ?? 0)
                                ? (op.VVT_KONTR_CRS_SUMMA ?? 0) < 0
                                    ? -(op.VVT_KONTR_CRS_SUMMA ?? 0)
                                    : op.VVT_KONTR_CRS_SUMMA ?? 0
                                : 0),
                        CRS_KONTR_OUT =
                            (double)
                            ((op.VVT_VAL_PRIHOD ?? 0) < (op.VVT_VAL_RASHOD ?? 0)
                                ? (op.VVT_KONTR_CRS_SUMMA ?? 0) < 0
                                    ? -(op.VVT_KONTR_CRS_SUMMA ?? 0)
                                    : op.VVT_KONTR_CRS_SUMMA ?? 0
                                : 0),
                        DOC_DC = op.DOC_CODE,
                        DOC_ROW_CODE = op.CODE,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.Bank,
                        CRS_OPER_IN = (double) (op.VVT_VAL_PRIHOD ?? 0),
                        CRS_OPER_OUT = (double) (op.VVT_VAL_RASHOD ?? 0),
                        OPER_CRS_DC = op.VVT_CRS_DC,
                        OPER_CRS_RATE = op.VVT_KONTR_CRS_RATE ?? 0,
                        UCH_CRS_RATE = op.VVT_UCHET_VALUTA_RATE ?? 0,
                        DOC_EXT_NUM = op.VVT_DOC_NUM
                    });

                // приходный кассовый ордер
                foreach (var op in ent.SD_33.Where(
                    _ =>
                        _.KONTRAGENT_DC == kontrDC && _.DATE_ORD >= start &&
                        _.DATE_ORD <= end))
                    if (op != null)
                        operations.Add(new KONTR_BALANS_OPER_ARC
                        {
                            DOC_NAME = "Приходный кассовый ордер",
                            DOC_NUM = op.NUM_ORD.ToString(),
                            DOC_DATE = (DateTime) op.DATE_ORD,
                            CRS_KONTR_IN = (double) op.CRS_SUMMA,
                            CRS_KONTR_OUT = 0,
                            DOC_DC = op.DOC_CODE,
                            DOC_ROW_CODE = 0,
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = (int) DocumentTypes.CashIn,
                            CRS_OPER_IN = (double) op.SUMM_ORD,
                            CRS_OPER_OUT = 0,
                            OPER_CRS_DC = (decimal) op.CRS_DC,
                            OPER_CRS_RATE = (double) op.CRS_KOEF,
                            UCH_CRS_RATE = (double) op.UCH_VALUTA_RATE
                        });

                // расходный кассовый ордер
                foreach (var op in ent.SD_34.Where(
                    _ =>
                        _.KONTRAGENT_DC == kontrDC && _.DATE_ORD >= start &&
                        _.DATE_ORD <= end))
                    operations.Add(new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = "Расходный кассовый ордер",
                        DOC_NUM = op.NUM_ORD.ToString(),
                        // ReSharper disable once PossibleInvalidOperationException
                        DOC_DATE = (DateTime) op.DATE_ORD,
                        CRS_KONTR_IN = 0,
                        // ReSharper disable once PossibleInvalidOperationException
                        CRS_KONTR_OUT = (double) op.CRS_SUMMA,
                        DOC_DC = op.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.CashOut,
                        CRS_OPER_IN = 0,
                        // ReSharper disable PossibleInvalidOperationException
                        CRS_OPER_OUT = (double) op.SUMM_ORD,
                        OPER_CRS_DC = (decimal) op.CRS_DC,
                        OPER_CRS_RATE = (double) op.CRS_KOEF,
                        UCH_CRS_RATE = (double) op.UCH_VALUTA_RATE
                    });

                // С/ф от поставщика (ТМЦ)
                var res = from td24 in ent.TD_24
                    // ReSharper disable AccessToDisposedClosure
                    from sd24 in ent.SD_24
                    from td26 in ent.TD_26
                    from sd26 in ent.SD_26
                    from sd83 in ent.SD_83
                    // ReSharper restore AccessToDisposedClosure
                    where td24.DOC_CODE == sd24.DOC_CODE &&
                          td26.DOC_CODE == td24.DDT_SPOST_DC
                          && td26.CODE == td24.DDT_SPOST_ROW_CODE
                          && sd26.DOC_CODE == td26.DOC_CODE
                          && sd83.DOC_CODE == td24.DDT_NOMENKL_DC
                          && sd26.SF_POST_DC == kontrDC
                          && sd24.DD_DATE >= start && sd24.DD_DATE <= end
                    select new
                    {
                        DOC_NAME = "С/ф от поставщика (ТМЦ)",
                        DOC_NUM = sd26.SF_POSTAV_NUM,
                        DOC_DATE = sd24.DD_DATE,
                        CRS_KONTR_IN =
                            (double) (td26.SFT_SUMMA_K_OPLATE_KONTR_CRS * td24.DDT_KOL_PRIHOD / td26.SFT_KOL),
                        CRS_KONTR_OUT = 0,
                        DOC_DC = td26.DOC_CODE,
                        DOC_ROW_CODE = td26.CODE,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountIn,
                        CRS_OPER_IN = (double) sd26.SF_CRS_SUMMA,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = sd26.SF_CRS_DC ?? 0,
                        OPER_CRS_RATE = (double) sd26.SF_KONTR_CRS_RATE,
                        UCH_CRS_RATE = (double) sd26.SF_UCHET_VALUTA_RATE
                    };
                var g = from r in res
                    group r by new {r.DOC_DC, r.DOC_DATE}
                    into grp
                    select new
                    {
                        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                        DOC_NUM = grp.Min(_ => _.DOC_NUM),
                        grp.Key.DOC_DATE,
                        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                        CRS_KONTR_OUT = 0,
                        grp.Key.DOC_DC,
                        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = 26,
                        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                    };
                operations.AddRange(g.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = op.CRS_OPER_IN,
                        CRS_OPER_OUT = op.CRS_OPER_OUT,
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));

                // С/ф от поставщика (услуги)
                var resUslugi = from td26 in ent.TD_26
                    // ReSharper disable AccessToDisposedClosure
                    from sd26 in ent.SD_26
                    from sd83 in ent.SD_83
                    // ReSharper restore AccessToDisposedClosure
                    where (td26.SFT_PEREVOZCHIK_POZITION ?? 0) == 0
                          && (td26.SFT_NAKLAD_KONTR_DC ?? 0) == 0
                          && sd26.DOC_CODE == td26.DOC_CODE
                          && sd83.DOC_CODE == td26.SFT_NEMENKL_DC
                          && sd83.NOM_0MATER_1USLUGA == 1
                          && sd83.NOM_1NAKLRASH_0NO == 0
                          && sd26.SF_POST_DC == kontrDC
                          && (sd26.SF_ACCEPTED ?? 0) == 1
                          && (sd26.SF_TRANZIT ?? 0) == 0
                          && sd26.SF_REGISTR_DATE >= start && sd26.SF_REGISTR_DATE <= end
                    select new
                    {
                        DOC_NAME = "С/ф от поставщика (услуги)",
                        DOC_NUM = sd26.SF_POSTAV_NUM,
                        DOC_DATE = sd26.SF_REGISTR_DATE,
                        CRS_KONTR_IN = (double) td26.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                        CRS_KONTR_OUT = 0,
                        DOC_DC = sd26.DOC_CODE,
                        DOC_ROW_CODE = td26.CODE,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountIn,
                        CRS_OPER_IN = (double) td26.SFT_SUMMA_K_OPLATE,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = sd26.SF_CRS_DC ?? 0,
                        OPER_CRS_RATE = (double) sd26.SF_KONTR_CRS_RATE,
                        UCH_CRS_RATE = (double) sd26.SF_UCHET_VALUTA_RATE
                    };
                var resUslugiWithNaklad = from td26 in ent.TD_26
                    // ReSharper disable AccessToDisposedClosure
                    from sd26 in ent.SD_26
                    from sd83 in ent.SD_83
                    // ReSharper restore AccessToDisposedClosure
                    where (td26.SFT_PEREVOZCHIK_POZITION ?? 0) == 0
                          && (td26.SFT_NAKLAD_KONTR_DC ?? 0) == 0
                          && sd26.DOC_CODE == td26.DOC_CODE
                          && sd83.DOC_CODE == td26.SFT_NEMENKL_DC
                          && sd83.NOM_0MATER_1USLUGA == 1
                          //&& sd83.NOM_1NAKLRASH_0NO == 0
                          && sd26.SF_POST_DC == kontrDC
                          && (sd26.SF_ACCEPTED ?? 0) == 1
                          && (sd26.SF_TRANZIT ?? 0) == 0
                          && sd26.SF_REGISTR_DATE >= start && sd26.SF_REGISTR_DATE <= end
                    select new
                    {
                        DOC_NAME = "С/ф от поставщика (услуги)",
                        DOC_NUM = sd26.SF_POSTAV_NUM,
                        DOC_DATE = sd26.SF_REGISTR_DATE,
                        CRS_KONTR_IN = (double) td26.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                        CRS_KONTR_OUT = 0,
                        DOC_DC = sd26.DOC_CODE,
                        DOC_ROW_CODE = td26.CODE,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountIn,
                        CRS_OPER_IN = (double) td26.SFT_SUMMA_K_OPLATE,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = sd26.SF_CRS_DC ?? 0,
                        OPER_CRS_RATE = (double) sd26.SF_KONTR_CRS_RATE,
                        UCH_CRS_RATE = (double) sd26.SF_UCHET_VALUTA_RATE
                    };
                if (GlobalOptions.SystemProfile.NomenklCalcType == NomenklCalcType.Standart)
                {
                    var gUslugi = from r in resUslugi
                        group r by new {r.DOC_DC, r.DOC_DATE}
                        into grp
                        select new
                        {
                            DOC_NAME = grp.Min(_ => _.DOC_NAME),
                            DOC_NUM = grp.Min(_ => _.DOC_NUM),
                            grp.Key.DOC_DATE,
                            CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                            CRS_KONTR_OUT = 0,
                            grp.Key.DOC_DC,
                            DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = 26,
                            CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                            CRS_OPER_OUT = 0,
                            OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                            OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                            UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                        };
                    operations.AddRange(gUslugi.ToList()
                        .Select(op => new KONTR_BALANS_OPER_ARC
                        {
                            DOC_NAME = op.DOC_NAME,
                            DOC_NUM = op.DOC_NUM,
                            DOC_DATE = (DateTime) op.DOC_DATE,
                            CRS_KONTR_IN = op.CRS_KONTR_IN,
                            CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                            DOC_DC = op.DOC_DC,
                            DOC_ROW_CODE = op.DOC_ROW_CODE,
                            KONTR_DC = op.KONTR_DC,
                            DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                            CRS_OPER_IN = op.CRS_OPER_IN,
                            CRS_OPER_OUT = op.CRS_OPER_OUT,
                            OPER_CRS_DC = op.OPER_CRS_DC,
                            OPER_CRS_RATE = op.OPER_CRS_RATE,
                            UCH_CRS_RATE = op.UCH_CRS_RATE
                        }));
                }
                else
                {
                    var gUslugi = from r in resUslugiWithNaklad
                        group r by new {r.DOC_DC, r.DOC_DATE}
                        into grp
                        select new
                        {
                            DOC_NAME = grp.Min(_ => _.DOC_NAME),
                            DOC_NUM = grp.Min(_ => _.DOC_NUM),
                            grp.Key.DOC_DATE,
                            CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                            CRS_KONTR_OUT = 0,
                            grp.Key.DOC_DC,
                            DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = 26,
                            CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                            CRS_OPER_OUT = 0,
                            OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                            OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                            UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                        };
                    operations.AddRange(gUslugi.ToList()
                        .Select(op => new KONTR_BALANS_OPER_ARC
                        {
                            DOC_NAME = op.DOC_NAME,
                            DOC_NUM = op.DOC_NUM,
                            DOC_DATE = (DateTime) op.DOC_DATE,
                            CRS_KONTR_IN = op.CRS_KONTR_IN,
                            CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                            DOC_DC = op.DOC_DC,
                            DOC_ROW_CODE = op.DOC_ROW_CODE,
                            KONTR_DC = op.KONTR_DC,
                            DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                            CRS_OPER_IN = op.CRS_OPER_IN,
                            CRS_OPER_OUT = op.CRS_OPER_OUT,
                            OPER_CRS_DC = op.OPER_CRS_DC,
                            OPER_CRS_RATE = op.OPER_CRS_RATE,
                            UCH_CRS_RATE = op.UCH_CRS_RATE
                        }));
                }

                if (GlobalOptions.SystemProfile.NomenklCalcType == NomenklCalcType.Standart)
                {
                    // С/ф от поставщика (накладные расходы как услуги)
                    var resNaklUslugi = from td26 in ent.TD_26
                        // ReSharper disable AccessToDisposedClosure
                        from sd26 in ent.SD_26
                        from sd83 in ent.SD_83
                        // ReSharper restore AccessToDisposedClosure
                        where (td26.SFT_PEREVOZCHIK_POZITION ?? 0) == 0
                              &&
                              ((td26.SFT_NAKLAD_KONTR_DC ?? 0) == 0 || (td26.SFT_NAKLAD_KONTR_DC ?? 0) == kontrDC)
                              && sd26.DOC_CODE == td26.DOC_CODE
                              && sd83.DOC_CODE == td26.SFT_NEMENKL_DC
                              && sd83.NOM_1NAKLRASH_0NO == 1
                              && sd26.SF_POST_DC == kontrDC
                              && (sd26.SF_ACCEPTED ?? 0) == 1
                              && (sd26.SF_TRANZIT ?? 0) == 0
                              && sd26.SF_REGISTR_DATE >= start && sd26.SF_REGISTR_DATE <= end
                        select new
                        {
                            DOC_NAME = "С/ф от поставщика (накладные расходы как услуги)",
                            DOC_NUM = sd26.SF_POSTAV_NUM,
                            DOC_DATE = sd26.SF_REGISTR_DATE,
                            CRS_KONTR_IN = (double) td26.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                            CRS_KONTR_OUT = 0,
                            DOC_DC = sd26.DOC_CODE,
                            DOC_ROW_CODE = td26.CODE,
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = (int) DocumentTypes.AccountIn,
                            CRS_OPER_IN = (double) td26.SFT_SUMMA_K_OPLATE,
                            CRS_OPER_OUT = 0,
                            OPER_CRS_DC = sd26.SF_CRS_DC ?? 0,
                            OPER_CRS_RATE = (double) sd26.SF_KONTR_CRS_RATE,
                            UCH_CRS_RATE = (double) sd26.SF_UCHET_VALUTA_RATE
                        };
                    var gNaklUslugi = from r in resNaklUslugi
                        group r by new {r.DOC_DC, r.DOC_DATE}
                        into grp
                        select new
                        {
                            DOC_NAME = grp.Min(_ => _.DOC_NAME),
                            DOC_NUM = grp.Min(_ => _.DOC_NUM),
                            grp.Key.DOC_DATE,
                            CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                            CRS_KONTR_OUT = 0,
                            grp.Key.DOC_DC,
                            DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                            KONTR_DC = kontrDC,
                            DOC_TYPE_CODE = 26,
                            CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                            CRS_OPER_OUT = 0,
                            OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                            OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                            UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                        };
                    operations.AddRange(gNaklUslugi.ToList()
                        .Select(op => new KONTR_BALANS_OPER_ARC
                        {
                            DOC_NAME = op.DOC_NAME,
                            DOC_NUM = op.DOC_NUM,
                            DOC_DATE = (DateTime) op.DOC_DATE,
                            CRS_KONTR_IN = op.CRS_KONTR_IN,
                            CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                            DOC_DC = op.DOC_DC,
                            DOC_ROW_CODE = op.DOC_ROW_CODE,
                            KONTR_DC = op.KONTR_DC,
                            DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                            CRS_OPER_IN = op.CRS_OPER_IN,
                            CRS_OPER_OUT = op.CRS_OPER_OUT,
                            OPER_CRS_DC = op.OPER_CRS_DC,
                            OPER_CRS_RATE = op.OPER_CRS_RATE,
                            UCH_CRS_RATE = op.UCH_CRS_RATE
                        }));
                }

                var sql = string.Format("SELECT 'Доход дилера по с/ф' AS DOC_NAME, " +
                                        "CONVERT(varchar, SD_84.SF_IN_NUM) + ISNULL(' / ' + SD_84.SF_OUT_NUM, '') AS DOC_NUM," +
                                        " ISNULL(SD_24.DD_DATE, SD_84.SF_DATE) AS DOC_DATE, " +
                                        "SUM(TD_84.SFT_NACENKA_DILERA * " +
                                        "CASE ISNULL(SD_83.NOM_0MATER_1USLUGA,0) " +
                                        "WHEN 0 THEN ISNULL(TD_24.DDT_KOL_RASHOD,0) " +
                                        "ELSE TD_84.SFT_KOL " +
                                        "END * CASE SD_84.SF_CRS_DC WHEN 3010000002 THEN 1 / SD_84.SF_UCHET_VALUTA_RATE ELSE SD_84.SF_UCHET_VALUTA_RATE END) AS CRS_KONTR_IN, " +
                                        "cast(0 as float) as CRS_KONTR_OUT," +
                                        "SD_84.DOC_CODE AS DOC_DC, " +
                                        "0 as DOC_ROW_CODE, " +
                                        "84 as DOC_TYPE_CODE, " +
                                        "cast(0 as float) as CRS_OPER_IN, " +
                                        "cast(SF_CRS_SUMMA_K_OPLATE as float) as CRS_OPER_OUT, " +
                                        "SD_84.SF_CRS_DC as OPER_CRS_DC,  " +
                                        "SD_84.SF_KONTR_CRS_RATE as OPER_CRS_RATE, " +
                                        "SD_84.SF_UCHET_VALUTA_RATE as UCH_CRS_RATE " +
                                        "FROM SD_84 (NOLOCK) " +
                                        "INNER JOIN SD_301 (NOLOCK) ON SD_84.SF_DILER_CRS_DC = SD_301.DOC_CODE " +
                                        "INNER JOIN SD_301 OP (NOLOCK) ON OP.DOC_CODE = SD_84.SF_CRS_DC " +
                                        "INNER JOIN TD_84 (NOLOCK) ON SD_84.DOC_CODE = TD_84.DOC_CODE " +
                                        "INNER JOIN SD_83 (NOLOCK) ON SD_83.DOC_CODE = TD_84.SFT_NEMENKL_DC " +
                                        "LEFT OUTER JOIN TD_24 (NOLOCK) ON TD_24.DDT_SFACT_DC = TD_84.DOC_CODE " +
                                        "AND TD_24.DDT_SFACT_ROW_CODE = TD_84.Code " +
                                        "AND TD_24.DDT_KOL_RASHOD > 0 " +
                                        "LEFT OUTER JOIN SD_24 (NOLOCK) ON SD_24.DOC_CODE = TD_24.DOC_CODE " +
                                        "WHERE ((ISNULL(SD_24.DD_DATE, '1900-01-01' ) BETWEEN '{1}' AND '{2}' AND ISNULL(SD_83.NOM_0MATER_1USLUGA,0) = 0) " +
                                        "OR (SD_84.SF_DATE BETWEEN '{1}' AND '{2}' AND ISNULL(SD_83.NOM_0MATER_1USLUGA,0) = 1)) " +
                                        "AND SF_DILER_DC = {0} " +
                                        "AND SF_ACCEPTED = 1 " +
                                        "GROUP BY SD_84.SF_IN_NUM, SD_84.SF_OUT_NUM, SD_84.SF_DATE, SD_24.DD_DATE, SD_301.CRS_SHORTNAME, SF_DILER_CRS_DC, " +
                                        "SD_84.DOC_CODE, SF_CRS_SUMMA_K_OPLATE, OP.CRS_SHORTNAME, SD_84.SF_CRS_DC, SD_84.SF_KONTR_CRS_RATE, SD_84.SF_UCHET_VALUTA_RATE",
                    kontrDC, CustomFormat.DateToString(start), CustomFormat.DateToString(end));
                var resDiler =
                    ent.Database.SqlQuery<KontrBalansRow>(sql);
                var gDiler = from r in resDiler
                    group r by new {r.DOC_DC, r.DOC_DATE}
                    into grp
                    select new
                    {
                        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                        DOC_NUM = grp.Min(_ => _.DOC_NUM),
                        grp.Key.DOC_DATE,
                        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                        CRS_KONTR_OUT = 0,
                        grp.Key.DOC_DC,
                        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = 26,
                        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                    };
                operations.AddRange(gDiler.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = (double) op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = (int) op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = op.CRS_OPER_IN,
                        CRS_OPER_OUT = op.CRS_OPER_OUT,
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));
                var vozvratTovara = from td24 in ent.TD_24
                    // ReSharper disable AccessToDisposedClosure
                    from sd24 in ent.SD_24
                    from sd43 in ent.SD_43
                    // ReSharper restore AccessToDisposedClosure
                    where td24.DOC_CODE == sd24.DOC_CODE && td24.DDT_SPOST_DC == null
                                                         && sd24.DD_DATE >= start && sd24.DD_DATE <= end
                                                         && sd43.DOC_CODE == sd24.DD_KONTR_OTPR_DC &&
                                                         sd43.DOC_CODE == kontrDC
                                                         && sd24.DD_VOZVRAT == 1
                    select new
                    {
                        DOC_NAME = "Возврат товара",
                        DOC_NUM =
                            sd24.DD_IN_NUM + (string.IsNullOrEmpty(sd24.DD_EXT_NUM) ? " / " + sd24.DD_EXT_NUM : null),
                        DOC_DATE = sd24.DD_DATE,
                        CRS_KONTR_IN = (double) (td24.DDT_KONTR_CRS_SUMMA ?? 0),
                        CRS_KONTR_OUT = (double) 0,
                        DOC_DC = sd24.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountOut,
                        CRS_OPER_IN = (double) ((td24.DDT_TAX_CENA ?? 0) * td24.DDT_KOL_PRIHOD),
                        CRS_OPER_OUT = (double) 0,
                        OPER_CRS_DC = sd43.VALUTA_DC,
                        OPER_CRS_RATE = sd24.DD_KONTR_CRS_RATE ?? 0,
                        UCH_CRS_RATE = sd24.DD_UCHET_VALUTA_RATE ?? 0
                    };
                operations.AddRange(vozvratTovara.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = op.CRS_OPER_IN,
                        CRS_OPER_OUT = op.CRS_OPER_OUT,
                        OPER_CRS_DC = (decimal) op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));

                // С/ф клиенту (ТМЦ)
                var resTovarClient = from sd84 in ent.SD_84
                    // ReSharper disable AccessToDisposedClosure
                    from td84 in ent.TD_84
                    from td24 in ent.TD_24
                    from sd24 in ent.SD_24
                    from sd83 in ent.SD_83
                    // ReSharper restore AccessToDisposedClosure
                    where td84.DOC_CODE == sd84.DOC_CODE
                          && td84.SFT_KOL > 0
                          && td24.DDT_SFACT_DC == td84.DOC_CODE && td24.DDT_SFACT_ROW_CODE == td84.CODE &&
                          td24.DDT_KOL_RASHOD > 0
                          && td24.DOC_CODE == sd24.DOC_CODE
                          && sd83.DOC_CODE == td84.SFT_NEMENKL_DC && sd83.NOM_0MATER_1USLUGA == 0 &&
                          (sd84.SF_TRANZIT ?? 0) == 0
                          && sd84.SF_CLIENT_DC == kontrDC
                          && sd24.DD_DATE >= start && sd24.DD_DATE <= end
                    select new
                    {
                        DOC_NAME = "С/ф клиенту (ТМЦ)",
                        DOC_NUM =
                            sd84.SF_IN_NUM + (string.IsNullOrEmpty(sd84.SF_OUT_NUM) ? " / " + sd84.SF_OUT_NUM : null),
                        DOC_DATE = sd24.DD_DATE,
                        CRS_KONTR_IN = 0,
                        CRS_KONTR_OUT = (double) (td84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / td84.SFT_KOL *
                                        (double) td24.DDT_KOL_RASHOD,
                        DOC_DC = sd84.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountOut,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = sd84.SF_CRS_SUMMA_K_OPLATE,
                        OPER_CRS_DC = sd84.SF_CRS_DC,
                        OPER_CRS_RATE = sd84.SF_KONTR_CRS_RATE ?? 0,
                        UCH_CRS_RATE = (double) sd84.SF_UCHET_VALUTA_RATE
                    };
                var gTovarClient = from r in resTovarClient
                    group r by new {r.DOC_DC, r.DOC_DATE}
                    into grp
                    select new
                    {
                        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                        DOC_NUM = grp.Min(_ => _.DOC_NUM),
                        grp.Key.DOC_DATE,
                        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                        CRS_KONTR_OUT = grp.Sum(_ => _.CRS_KONTR_OUT),
                        grp.Key.DOC_DC,
                        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = 84,
                        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                        CRS_OPER_OUT = grp.Sum(_ => _.CRS_OPER_OUT),
                        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                    };
                operations.AddRange(gTovarClient.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = (double) op.CRS_OPER_IN,
                        CRS_OPER_OUT = (double) op.CRS_OPER_OUT,
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));
                // С/ф клиенту (услуги)
                var resClientUslugi = from sd84 in ent.SD_84
                    // ReSharper disable AccessToDisposedClosure
                    from td84 in ent.TD_84
                    from sd83 in ent.SD_83
                    // ReSharper restore AccessToDisposedClosure
                    where sd84.SF_CLIENT_DC == kontrDC
                          && sd84.SF_DATE >= start && sd84.SF_DATE <= end
                          && sd84.DOC_CODE == td84.DOC_CODE
                          && sd84.SF_ACCEPTED == 1
                          && (sd83.NOM_0MATER_1USLUGA == 1 || sd83.NOM_1NAKLRASH_0NO == 1)
                          && (sd84.SF_TRANZIT ?? 0) == 0
                          && sd83.DOC_CODE == td84.SFT_NEMENKL_DC
                    select new
                    {
                        DOC_NAME = "С/ф клиенту (услуги)",
                        DOC_NUM =
                            sd84.SF_IN_NUM + (string.IsNullOrEmpty(sd84.SF_OUT_NUM) ? " / " + sd84.SF_OUT_NUM : null),
                        DOC_DATE = sd84.SF_DATE,
                        CRS_KONTR_IN = 0,
                        CRS_KONTR_OUT = td84.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                        DOC_DC = sd84.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountOut,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = sd84.SF_CRS_SUMMA_K_OPLATE,
                        OPER_CRS_DC = sd84.SF_CRS_DC,
                        OPER_CRS_RATE = sd84.SF_KONTR_CRS_RATE ?? 0,
                        UCH_CRS_RATE = (double) sd84.SF_UCHET_VALUTA_RATE
                    };
                var gClientUslugi = from r in resClientUslugi
                    group r by new {r.DOC_DC, r.DOC_DATE}
                    into grp
                    select new
                    {
                        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                        DOC_NUM = grp.Min(_ => _.DOC_NUM),
                        grp.Key.DOC_DATE,
                        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                        CRS_KONTR_OUT = grp.Sum(_ => _.CRS_KONTR_OUT),
                        grp.Key.DOC_DC,
                        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = 84,
                        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                        CRS_OPER_OUT = grp.Sum(_ => _.CRS_OPER_OUT),
                        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                    };
                operations.AddRange(gClientUslugi.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = (double) op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = (double) op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = op.CRS_OPER_IN,
                        CRS_OPER_OUT = (double) op.CRS_OPER_OUT,
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));

                // Продажа за наличный расчет
                var resProdNal = from sd259 in ent.SD_259
                    select new
                    {
                        DOC_NAME = "Продажа за наличный расчет",
                        DOC_NUM = sd259.PZN_NUM,
                        DOC_DATE = sd259.PZN_DATE,
                        CRS_KONTR_IN = 0,
                        CRS_KONTR_OUT = sd259.PZN_KONTR_CRS_SUMMA,
                        DOC_DC = sd259.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.SaleForCash,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = sd259.PZNT_SUMMA_K_OPLATE,
                        OPER_CRS_DC = sd259.PZN_PRLIST_CRS_DC,
                        OPER_CRS_RATE = sd259.PZN_KONTR_CRS_RATE,
                        UCH_CRS_RATE = sd259.PZN_UCH_CRS_RATE
                    };
                var gProdNal = from r in resProdNal
                    group r by new {r.DOC_DC, r.DOC_DATE}
                    into grp
                    select new
                    {
                        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                        DOC_NUM = grp.Min(_ => _.DOC_NUM).ToString(),
                        grp.Key.DOC_DATE,
                        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                        CRS_KONTR_OUT = grp.Sum(_ => _.CRS_KONTR_OUT),
                        grp.Key.DOC_DC,
                        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = 259,
                        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                        CRS_OPER_OUT = grp.Sum(_ => _.CRS_OPER_OUT),
                        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                    };
                operations.AddRange(gProdNal.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM,
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = (double) op.CRS_KONTR_IN,
                        CRS_KONTR_OUT = (double) op.CRS_KONTR_OUT,
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = op.CRS_OPER_IN,
                        CRS_OPER_OUT = (double) op.CRS_OPER_OUT,
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = op.UCH_CRS_RATE
                    }));

                // Акт в/з 
                var resActZachet = from sd110 in ent.SD_110
                    // ReSharper disable AccessToDisposedClosure
                    from td110 in ent.TD_110
                    // ReSharper restore AccessToDisposedClosure
                    where sd110.DOC_CODE == td110.DOC_CODE
                          && td110.VZT_KONTR_DC == kontrDC
                          && td110.VZT_DOC_DATE >= start && td110.VZT_DOC_DATE <= end
                    select new
                    {
                        DOC_NAME = "Акт в/з ",
                        DOC_NUM = sd110.VZ_NUM,
                        DOC_DATE = td110.VZT_DOC_DATE,
                        CRS_KONTR_IN = td110.VZT_KONTR_CRS_SUMMA > 0 ? 0 : -td110.VZT_KONTR_CRS_SUMMA,
                        CRS_KONTR_OUT = td110.VZT_KONTR_CRS_SUMMA < 0 ? 0 : td110.VZT_KONTR_CRS_SUMMA,
                        DOC_DC = sd110.DOC_CODE,
                        DOC_ROW_CODE = td110.CODE,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.ActZachet,
                        CRS_OPER_IN = td110.VZT_KONTR_CRS_SUMMA > 0 ? 0 : -td110.VZT_KONTR_CRS_SUMMA,
                        CRS_OPER_OUT = td110.VZT_KONTR_CRS_SUMMA < 0 ? 0 : td110.VZT_KONTR_CRS_SUMMA,
                        OPER_CRS_DC = td110.VZT_CRS_DC,
                        OPER_CRS_RATE = (double) td110.VZT_KONTR_CRS_RATE,
                        UCH_CRS_RATE = td110.VZT_UCH_CRS_RATE
                    };
                //var gActZachet = from r in resActZachet
                //    group r by new {r.DOC_DC, r.DOC_DATE}
                //    into grp
                //    select new
                //    {
                //        DOC_NAME = grp.Min(_ => _.DOC_NAME),
                //        DOC_NUM = grp.Min(_ => _.DOC_NUM).ToString(),
                //        grp.Key.DOC_DATE,
                //        CRS_KONTR_IN = grp.Sum(_ => _.CRS_KONTR_IN),
                //        CRS_KONTR_OUT = grp.Sum(_ => _.CRS_KONTR_OUT),
                //        grp.Key.DOC_DC,
                //        DOC_ROW_CODE = grp.Min(_ => _.DOC_ROW_CODE),
                //        KONTR_DC = kontrDC,
                //        DOC_TYPE_CODE = 110,
                //        CRS_OPER_IN = grp.Sum(_ => _.CRS_OPER_IN),
                //        CRS_OPER_OUT = grp.Sum(_ => _.CRS_OPER_OUT),
                //        OPER_CRS_DC = grp.Min(_ => _.OPER_CRS_DC),
                //        OPER_CRS_RATE = grp.Min(_ => _.OPER_CRS_RATE),
                //        UCH_CRS_RATE = grp.Min(_ => _.UCH_CRS_RATE)
                //    };
                operations.AddRange(resActZachet.ToList()
                    .Select(op => new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = op.DOC_NAME,
                        DOC_NUM = op.DOC_NUM.ToString(),
                        DOC_DATE = op.DOC_DATE,
                        CRS_KONTR_IN = (double) (op.CRS_KONTR_IN ?? 0),
                        CRS_KONTR_OUT = (double) (op.CRS_KONTR_OUT ?? 0),
                        DOC_DC = op.DOC_DC,
                        DOC_ROW_CODE = op.DOC_ROW_CODE,
                        KONTR_DC = op.KONTR_DC,
                        DOC_TYPE_CODE = op.DOC_TYPE_CODE,
                        CRS_OPER_IN = (double) (op.CRS_OPER_IN ?? 0),
                        CRS_OPER_OUT = (double) (op.CRS_OPER_OUT ?? 0),
                        OPER_CRS_DC = op.OPER_CRS_DC,
                        OPER_CRS_RATE = op.OPER_CRS_RATE,
                        UCH_CRS_RATE = (double) op.UCH_CRS_RATE
                    }));

                // Акт сверки
                foreach (
                    var op in
                    ent.SD_430.Where(
                        _ => _.ASV_DATE >= start && _.ASV_DATE <= end && _.ASV_KONTR_DC == kontrDC))
                    operations.Add(new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = "Акт сверки",
                        DOC_NUM = op.ASV_NUM.ToString(),
                        DOC_DATE = op.ASV_FIKS_DATE,
                        CRS_KONTR_IN = 0,
                        CRS_KONTR_OUT = 0,
                        DOC_DC = op.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.ActReconciliation,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = kontr.SD_301.DOC_CODE,
                        OPER_CRS_RATE = 1,
                        UCH_CRS_RATE = 0
                    });

                // С/ф от поставщика (накладные расходы внешнего исполнителя)
                var curNakladSF = (from sd26 in ent.SD_26
                        // ReSharper disable AccessToDisposedClosure
                        from td26 in ent.TD_26
                        // ReSharper disable once InconsistentNaming
                        from td26_2 in ent.TD_26
                        from td24 in ent.TD_24
                        from sd24 in ent.SD_24
                        // ReSharper restore AccessToDisposedClosure
                        where sd26.DOC_CODE == td26.DOC_CODE
                              && td26.SFT_IS_NAKLAD == 1
                              && sd26.DOC_CODE == td26_2.DOC_CODE
                              && td24.DDT_SPOST_DC == td26_2.DOC_CODE
                              && td24.DDT_SPOST_ROW_CODE == td26_2.CODE
                              && (td24.DDT_TAX_IN_SFACT ?? 0) == 1
                              && td24.DOC_CODE == sd24.DOC_CODE
                              && sd24.DD_DATE >= start && sd24.DD_DATE <= end
                              && (sd26.SF_TRANZIT ?? 0) == 0
                              && (td26.SFT_PEREVOZCHIK_POZITION ?? 0) == 0
                              && (td26.SFT_NAKLAD_KONTR_DC ?? sd26.SF_POST_DC) == kontrDC
                        select new
                        {
                            nNakladSFDC = sd26.DOC_CODE,
                            dtOtgrDate = sd24.DD_DATE
                        }).ToList()
                    .Distinct();
                operations.AddRange(from r in curNakladSF
                    let nNakladSumKontr = (from sd26 in ent.SD_26
                            from td26 in ent.TD_26
                            from td24 in ent.TD_24
                            from sd24 in ent.SD_24
                            where
                                sd26.DOC_CODE == td26.DOC_CODE && td24.DDT_SPOST_DC == td26.DOC_CODE &&
                                td24.DDT_SPOST_ROW_CODE == td26.CODE && td24.DDT_TAX_EXECUTED == 1 &&
                                td24.DOC_CODE == sd24.DOC_CODE && sd24.DD_DATE == r.dtOtgrDate &&
                                sd26.DOC_CODE == r.nNakladSFDC
                            select new
                            {
                                summa = (td26.SFT_SUMMA_NAKLAD ?? 0) * td24.DDT_KOL_PRIHOD / td26.SFT_KOL
                            }).ToList()
                        .Sum(_ => _.summa)
                    let nNakladSumAll = (from td26 in ent.TD_26
                            join sd26 in ent.SD_26 on td26.DOC_CODE equals sd26.DOC_CODE
                            join sd83 in ent.SD_83 on td26.SFT_NEMENKL_DC equals sd83.DOC_CODE
                            where td26.DOC_CODE == r.nNakladSFDC && td26.SFT_IS_NAKLAD == 1
                            select new {nNakladSumAll = td26.SFT_KOL * (td26.SFT_POST_ED_CENA ?? 0)}).ToList()
                        .Sum(_ => _.nNakladSumAll)
                    where nNakladSumAll != 0
                    let nNakladSumToOplAll = (from td26 in ent.TD_26
                            from sd26 in ent.SD_26
                            where
                                sd26.DOC_CODE == td26.DOC_CODE && sd26.DOC_CODE == r.nNakladSFDC &&
                                (td26.SFT_NAKLAD_KONTR_DC ?? sd26.SF_POST_DC) == kontrDC && td26.SFT_IS_NAKLAD == 1
                            select new
                            {
                                nNakladSumToOplAll =
                                    ((kontr.VALUTA_DC == GlobalOptions.SystemProfile.MainCurrency.DocCode
                                        ? td26.SFT_SUMMA_V_UCHET_VALUTE
                                        : td26.SFT_SUMMA_K_OPLATE_KONTR_CRS) ?? 0) * nNakladSumKontr / nNakladSumAll
                            }).ToList()
                        .Sum(_ => _.nNakladSumToOplAll)
                    where nNakladSumToOplAll > 0
                    let op = ent.SD_26.First(_ => _.DOC_CODE == r.nNakladSFDC)
                    select new KONTR_BALANS_OPER_ARC
                    {
                        DOC_NAME = "С/ф от поставщика (накладные расходы внешнего исполнителя)",
                        DOC_NUM = op.SF_POSTAV_NUM,
                        DOC_DATE = r.dtOtgrDate,
                        CRS_KONTR_IN = (double) nNakladSumToOplAll,
                        CRS_KONTR_OUT = 0,
                        DOC_DC = op.DOC_CODE,
                        DOC_ROW_CODE = 0,
                        KONTR_DC = kontrDC,
                        DOC_TYPE_CODE = (int) DocumentTypes.AccountIn,
                        CRS_OPER_IN = 0,
                        CRS_OPER_OUT = 0,
                        OPER_CRS_DC = (decimal) op.SF_CRS_DC,
                        OPER_CRS_RATE = (double) op.SF_UCHET_VALUTA_RATE,
                        UCH_CRS_RATE = (double) op.SF_UCHET_VALUTA_RATE
                    });
                foreach (var op in operations)
                    op.ID = Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty);
            }

            // =========================
            return operations;
        }

        public static void CalcBalans(decimal kontrDC, DateTime start)
        {
            //var kontr = MainReferences.GetKontragent(kontrDC);
            using (var ent = GlobalOptions.GetEntities())
            {
                using (var tnx = ent.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var operations = GetOperationsOld(kontrDC, new DateTime(2000, 1, 1), DateTime.Today);
                    try
                    {
                        if (operations == null || operations.Count == 0)
                        {
                            ent.Database.ExecuteSqlCommand(
                                $"DELeTE from KONTR_BLS_RECALC where kontr_dc = {CustomFormat.DecimalToSqlDecimal(kontrDC)} ");
                            tnx.Commit();
                            return;
                        }

                        ent.Database.ExecuteSqlCommand(
                            $"DELeTE from KONTR_BALANS_OPER_ARC where kontr_dc = {CustomFormat.DecimalToSqlDecimal(kontrDC)}");
                        var sqlQuery = new StringBuilder();
                        if (operations.Any())
                        {
                            foreach (
                                var query in
                                operations.Where(_ => _.OPER_CRS_DC > 0)
                                    .Select(o =>
                                        "INSERT INTO dbo.KONTR_BALANS_OPER_ARC " +
                                        "(DOC_NAME ,DOC_NUM ,DOC_DATE ,CRS_KONTR_IN ,CRS_KONTR_OUT, DOC_DC, DOC_ROW_CODE, DOC_TYPE_CODE, CRS_OPER_IN " +
                                        ",CRS_OPER_OUT, OPER_CRS_DC, OPER_CRS_RATE, UCH_CRS_RATE, KONTR_DC, ID, NEW_CALC, DOC_EXT_NUM) " +
                                        "VALUES( " + $"'{o.DOC_NAME}', " + $"'{removeChars(o.DOC_NUM)}' " +
                                        $",'{CustomFormat.DateToString(o.DOC_DATE)}' " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.CRS_KONTR_IN)} " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.CRS_KONTR_OUT)} " +
                                        $",{o.DOC_DC}  " +
                                        $",{o.DOC_ROW_CODE} " + $",{o.DOC_TYPE_CODE} " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.CRS_OPER_IN)} " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.CRS_OPER_OUT)} " +
                                        $",{o.OPER_CRS_DC} " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.OPER_CRS_RATE)}  " +
                                        $",{CustomFormat.DecimalToSqlDecimal((decimal) o.UCH_CRS_RATE)} " +
                                        $",{o.KONTR_DC} " +
                                        $",'{o.ID}',1 " +
                                        $",'{o.DOC_EXT_NUM}' )"))
                                sqlQuery.Append(query + "; " + "\n ");
                            ent.Database.ExecuteSqlCommand(sqlQuery.ToString());
                        }

                        ent.Database.ExecuteSqlCommand($"DELeTE from KONTR_BLS_RECALC where kontr_dc = {kontrDC} ");
                        tnx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(null, ex);
                    }
                }
            }
        }

        private static string removeChars(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) return s;
            return s.Replace("\'","");
        }

        //public static void CalcBalans(decimal kontrDC)
        //{
        //    var currencies = GlobalOptions.GetEntities().SD_301.ToList();

        //    var bankOpers = GlobalOptions.GetEntities().TD_101.Include(_ => _.SD_101)
        //        .Where(_ => _.VVT_KONTRAGENT == kontrDC);
        //}

        //public static void CalcBalans(IEnumerable<decimal> kontrDClist)
        //{
        //}
    }
}