using System;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace Core
{
    public class CurrencyRates
    {
        private List<CURRENCY_RATES_CB> myRates = new List<CURRENCY_RATES_CB>();

        public CurrencyRates()
        {
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
            LoadRates();
        }

        public CurrencyRates(DateTime start, DateTime end)
        {
            StartDate = start;
            EndDate = end;
            LoadRates();
        }

        private DateTime StartDate { get; }
        private DateTime EndDate { get; }

        private void LoadRates()
        {
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    myRates =
                        ent.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= StartDate && _.RATE_DATE <= EndDate)
                            .ToList();
                }
                var dt = myRates.Select(_ => _.RATE_DATE).Distinct().ToList();
                myRates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public decimal GetRate(decimal firstDC, decimal secondDC, DateTime date)
        {
            if (firstDC == secondDC) return 1;
            var date1 = myRates.Where(_ => _.RATE_DATE <= date).Max(_ => _.RATE_DATE);
            var f = myRates.SingleOrDefault(_ => _.CRS_DC == firstDC && _.RATE_DATE == date1);
            var s = myRates.SingleOrDefault(_ => _.CRS_DC == secondDC && _.RATE_DATE == date1);
            if (f != null && s != null && s.RATE != 0)
                return f.RATE / f.NOMINAL / (s.RATE / s.NOMINAL);
            return -1;
        }

        public decimal GetRate(decimal? firstDC, decimal? secondDC, DateTime date)
        {
            if (firstDC == null || secondDC == null) return 0;
            return GetRate(firstDC.Value, secondDC.Value, date);
        }
    }
}