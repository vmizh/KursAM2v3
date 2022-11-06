using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Data;
using DevExpress.Mvvm;
using Helper;
using KursAM2.ViewModel.Management.DebitorCreditor;
using KursAM2.ViewModel.Splash;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.Calculations
{
    public class DebCredTemp
    {
        public decimal rdr0 { set; get; }
        public decimal rdr1 { set; get; }
        public decimal rdr2 { set; get; }
        public decimal rdr3 { set; get; }
        public decimal rdr4 { set; get; }
        public short rdr5 { set; get; }
        public decimal rdr6 { set; get; }
    }

    public class DebitorCreditorBuilder
    {
        public ISplashScreenService splashService;

        public DebitorCreditorBuilder(ISplashScreenService splashService)
        {
            this.splashService = splashService;
        }

        public List<DebitorCreditorRow> Load(DateTime start, DateTime end)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var kontrs = ent.SD_43.Include(_ => _.SD_301).ToList();
                var rates =
                    ent.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= start && _.RATE_DATE <= end)
                        .ToList();
                var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                var kontrChanged = ent.KONTR_BLS_RECALC;
                var dc_u = new List<decimal>(kontrChanged.Select(_ => _.KONTR_DC).Distinct());
                if (dc_u.Any())
                {
                    var vm = new DebitorCreditorCalcKontrSplashViewModel
                    {
                        MaxProgress = dc_u.Count + 1,
                        Minimum = 0,
                        ExtendExtendedTextVisibility = Visibility.Visible
                    };
                    splashService.ShowSplashScreen();
                    splashService.SetSplashScreenState(vm);
                    foreach (var k in dc_u)
                    {
                        var k1 = k;
                        var dd = kontrChanged.Where(_ => _.KONTR_DC == k1).ToList();
                        if (!dd.Any()) continue;
                        var minDate = dd.Min(_ => _.DATE_CHANGED);
                        vm.ExtendedText = string.Format("Контагент: {0} начиная с {1}",
                            kontrs.Single(_ => _.DOC_CODE == k), minDate);
                        RecalcKontragentBalans.CalcBalans(k, minDate);
                    }

                    splashService.HideSplashScreen();
                }

                rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                var data =
                    ent.Database.SqlQuery<DebCredTemp>(string.Format("SELECT KONTR_DC as rdr0 " +
                                                                     ", cast(sum(BLS_START) AS NUMERIC(18, 2)) as rdr1 " +
                                                                     ", cast(sum(BLS_out) AS NUMERIC(18, 2)) as rdr2 " +
                                                                     ", cast(sum(BLS_in) AS NUMERIC(18, 2)) as rdr3 " +
                                                                     ", cast(sum(BLS_START) - sum(BLS_OUT) + sum(BLS_IN)   AS NUMERIC(18, 2)) as rdr4 " +
                                                                     ", isnull(s.flag_balans,0) as rdr5" +
                                                                     ", s.VALUTA_DC as rdr6 " +
                                                                     "FROM " +
                                                                     "(SELECT KONTR_DC KONTR_DC " +
                                                                     ", sum(CASE WHEN DOC_NAME = ' На начало учета' THEN START_SUMMA ELSE 0 END) BLS_START " +
                                                                     ", sum(CASE WHEN DOC_NAME != ' На начало учета' THEN CRS_KONTR_IN ELSE 0 END) BLS_IN " +
                                                                     ", sum(CASE WHEN DOC_NAME != ' На начало учета' THEN CRS_KONTR_OUT ELSE 0 END) BLS_OUT " +
                                                                     ", 0 BLS_END " +
                                                                     "FROM " +
                                                                     " KONTR_BALANS_OPER_ARC KBOA " +
                                                                     " INNER JOIN SD_43 S43 " +
                                                                     "  ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                     "WHERE " +
                                                                     " DOC_DATE BETWEEN '{0}' AND '{1}' " +
                                                                     "AND DOC_DATE >= S43.START_BALANS " +
                                                                     "GROUP BY " +
                                                                     "kontr_dc " +
                                                                     " UNION ALL " +
                                                                     "SELECT KONTR_DC " +
                                                                     "    , sum(CRS_KONTR_IN) - sum(CRS_KONTR_OUT) " +
                                                                     "   , 0 " +
                                                                     "  , 0 " +
                                                                     " , 0 " +
                                                                     "FROM " +
                                                                     " KONTR_BALANS_OPER_ARC KBOA " +
                                                                     "INNER JOIN SD_43 S43 " +
                                                                     " ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                     "WHERE " +
                                                                     " DOC_DATE < '{0}' " +
                                                                     "AND DOC_DATE >= S43.START_BALANS " +
                                                                     "GROUP BY " +
                                                                     " KONTR_DC " +
                                                                     "UNION ALL " +
                                                                     "SELECT KONTR_DC " +
                                                                     "    , 0 " +
                                                                     "   , 0 " +
                                                                     "  , 0 " +
                                                                     " , sum(CRS_KONTR_IN) - sum(CRS_KONTR_OUT) " +
                                                                     "FROM " +
                                                                     " KONTR_BALANS_OPER_ARC KBOA " +
                                                                     "INNER JOIN SD_43 S43 " +
                                                                     "ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                     "	WHERE " +
                                                                     "	  DOC_DATE <= '{1}' " +
                                                                     "	  AND DOC_DATE >= S43.START_BALANS " +
                                                                     "	GROUP BY " +
                                                                     "	  KONTR_DC) TAB " +
                                                                     "	 inner join sd_43 s on s.doc_code = tab.kontr_dc " +
                                                                     "	GROUP BY " +
                                                                     "	  KONTR_DC, s.flag_balans, s.valuta_dc",
                            CustomFormat.DateToString(start), CustomFormat.DateToString(end)))
                        .ToList();
                return (from d in data.Where(_ => _.rdr4 != 0 || _.rdr2 != 0 || _.rdr3 != 0 || _.rdr1 != 0)
                    let rate1 =
                        GetRate(rates, d.rdr6, GlobalOptions.SystemProfile.MainCurrency.DocCode, start)
                    let rate2 = GetRate(rates, d.rdr6, GlobalOptions.SystemProfile.MainCurrency.DocCode, end)
                    let kontrInfo =
                        new KontragentViewModel(kontrs.SingleOrDefault(_ => _.DOC_CODE == d.rdr0))
                    select new DebitorCreditorRow
                    {
                        Delta = Math.Round(d.rdr4 * rate2 - (d.rdr1 * rate1 + (d.rdr2 * rate2 - d.rdr3 * rate2)), 2),
                        KontrEnd = -d.rdr4,
                        KontrIn = d.rdr2,
                        KontrInfo = kontrInfo,
                        KontrOut = d.rdr3,
                        KontrStart = -d.rdr1,
                        UchetEnd = -Math.Round(d.rdr4 * rate2, 2),
                        UchetIn = Math.Round(d.rdr2 * rate2, 2),
                        UchetOut = Math.Round(d.rdr3 * rate2, 2),
                        UchetStart = -Math.Round(d.rdr1 * rate1, 2),
                        Kontragent = kontrInfo.Name,
                        CurrencyName = ((IName)kontrInfo.BalansCurrency).Name,
                        IsBalans = d.rdr5 == 0
                    }).ToList();
            }
        }

        public static decimal GetRate(List<CURRENCY_RATES_CB> rates, decimal firstDC, decimal secondDC, DateTime date)
        {
            if (firstDC == secondDC) return 1;
            var date1 = rates.Where(_ => _.RATE_DATE <= date).Max(_ => _.RATE_DATE);
            var f = rates.SingleOrDefault(_ => _.CRS_DC == firstDC && _.RATE_DATE == date1);
            var s = rates.SingleOrDefault(_ => _.CRS_DC == secondDC && _.RATE_DATE == date1);
            if (f != null && s != null && s.RATE != 0)
                return f.RATE / f.NOMINAL / (s.RATE / s.NOMINAL);
            return -1;
        }
    }
}
