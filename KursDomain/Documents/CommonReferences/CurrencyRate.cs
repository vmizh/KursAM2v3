using System;
using System.Collections.Generic;
using System.Linq;
using Core.ViewModel.Base;

namespace Core.EntityViewModel.CommonReferences
{
    public class CurrencyRate
    {
        public static readonly List<CurrencyRate> CBRates = new List<CurrencyRate>();
        public decimal CrsDC { set; get; }
        public DateTime Date { set; get; }
        public decimal Rate { set; get; }

        /// <summary>
        ///     Получаем значение суммы во второй валюте из первой
        /// </summary>
        public static decimal GetSummaRate(decimal First, decimal Second, DateTime date, decimal Summa)
        {
            if (First == Second)
                return Summa;
            var fRate = CBRates.FirstOrDefault(t => t.CrsDC == First && t.Date == date);
            var sRate = CBRates.FirstOrDefault(t => t.CrsDC == Second && t.Date == date);
            if (sRate == null || fRate == null) return -1;
            if ((First == CurrencyCode.USD || First == CurrencyCode.EUR) && Second == CurrencyCode.RUB)
                return Math.Round(Summa * fRate.Rate, 4);
            if ((Second == CurrencyCode.USD || First == CurrencyCode.EUR) && First == CurrencyCode.RUB)
                return Math.Round(Summa / sRate.Rate, 4);
            return Summa * fRate.Rate / sRate.Rate;
        }

        public static decimal GetCBRate(Currency first, Currency second, DateTime date)
        {
            if (Equals(first, second)) return 1;
            var rates = GetRate(date);
            if (first.Equals(GlobalOptions.SystemProfile.NationalCurrency)) 
                return rates.ContainsKey(second) ? decimal.Round(rates[second], 4) : 1;
            if (second.Equals(GlobalOptions.SystemProfile.NationalCurrency)) 
                return rates.ContainsKey(first) ? decimal.Round(rates[first], 4) : 1;
            return rates.ContainsKey(first) && rates.ContainsKey(second) ?  
                decimal.Round(rates[first] / rates[second], 4) : 1;
        }

        public static decimal GetCBRate(Currency first, DateTime date)
        {
            if (Equals(first, GlobalOptions.SystemProfile.NationalCurrency)) return 1;
            var rates = GetRate(date);
            return rates.ContainsKey(first) ? decimal.Round(rates[first], 4) : 1;
        }

        public static decimal GetCBSummaRate(Currency first, Currency second, Dictionary<Currency, decimal> rates)
        {
            if (first.Equals(second)) return 1;
            if (first.Equals(GlobalOptions.SystemProfile.NationalCurrency))
                return rates.ContainsKey(second) ? decimal.Round(rates[second], 4) : 1;
            if (second.Equals(GlobalOptions.SystemProfile.NationalCurrency))
                return rates.ContainsKey(first) ? decimal.Round(rates[first], 4) : 1;

            if (rates.ContainsKey(first) && rates.ContainsKey(second))
                return decimal.Round(rates[first] / rates[second], 4);
            return 1;
        }

        public static Dictionary<Currency, decimal> GetRate(DateTime date)
        {
            var ret = new Dictionary<Currency, decimal>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var maxd = ctx.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE <= date).Max(_ => _.RATE_DATE);
                    foreach (var r in ctx.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE == maxd))
                        ret.Add(MainReferences.Currencies[r.CRS_DC], decimal.Round(r.RATE, 4));
                }

                ret.Add(GlobalOptions.SystemProfile.NationalCurrency, 1);
            }
            catch (Exception ex)
            {
                //WindowManager.ShowError(ex);
            }

            return ret;
        }

        public static decimal GetRate(decimal First, decimal Second, DateTime date)
        {
            return GetSummaRate(First, Second, date, 1);
        }

        public override string ToString()
        {
            return $"Курс валюты {CrsDC} на {Date} равен {Rate}";
        }

        public static void LoadCBrates(DateTime start, DateTime end)
        {
            CBRates.Clear();
            try
            {
                foreach (
                    var rate in
                    GlobalOptions.GetEntities()
                        .CURRENCY_RATES_CB.Where(t => t.RATE_DATE >= start && t.RATE_DATE <= end))
                {
                    if (!CBRates.Any(r => r.CrsDC == CurrencyCode.RUB && r.Date == rate.RATE_DATE))
                        CBRates.Add(new CurrencyRate {CrsDC = CurrencyCode.RUB, Date = rate.RATE_DATE, Rate = 1});
                    CBRates.Add(new CurrencyRate {CrsDC = rate.CRS_DC, Date = rate.RATE_DATE, Rate = rate.RATE});
                }
            }
            catch (Exception ex)
            {
                //WindowManager.ShowError(null, ex);
            }
        }
    }
}
