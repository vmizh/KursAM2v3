using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.ViewModel.Common
{
    [MetadataType(typeof(DataAnnotationsCrossCurrencyRate))]
    public class CrossCurrencyRate : RSViewModelBase
    {
        public readonly ObservableCollection<CrossCurrencyRate> CurrencyList =
            new ObservableCollection<CrossCurrencyRate>();

        private Currency myCurrency;
        private decimal myCurrencyEUR;
        private decimal myCurrencyGBP;
        private decimal myCurrencyRUB;
        private decimal myCurrencyUSD;

        /// <summary>
        ///     При указании курсов по отношению к рублю,
        ///     все зависимые курсы пересчитываются через рубли, иначе не зависимо
        /// </summary>
        private bool myIsCrossRateAuto;

        [Display(AutoGenerateField = false)]
        public bool IsCrossRateAuto
        {
            get => myIsCrossRateAuto;
            set
            {
                if (myIsCrossRateAuto == value) return;
                myIsCrossRateAuto = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                RaisePropertyChanged();
            }
        }
        [Display(Name = "USD")]
        [DisplayFormat(DataFormatString = "n4")]
        public decimal CurrencyUSD
        {
            get => myCurrencyUSD;
            set
            {
                if (myCurrencyUSD == value) return;
                myCurrencyUSD = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
        public decimal CurrencyRUB
        {
            get => myCurrencyRUB;
            set
            {
                if (myCurrencyRUB == value) return;
                myCurrencyRUB = value;
                RaisePropertyChanged();
            }
        }
        [Display(Name = "EUR")]
        [DisplayFormat(DataFormatString = "n4")]
        public decimal CurrencyEUR
        {
            get => myCurrencyEUR;
            set
            {
                if (myCurrencyEUR == value) return;
                myCurrencyEUR = value;
                RaisePropertyChanged();
            }
        }
        [Display(Name = "GBP")]
        [DisplayFormat(DataFormatString = "n4")]
        public decimal CurrencyGBP
        {
            get => myCurrencyGBP;
            set
            {
                if (myCurrencyGBP == value) return;
                myCurrencyGBP = value;
                RaisePropertyChanged();
            }
        }

        private void RecalcRate(string fieldName, decimal rate)
        {
            if (Equals(Currency, GlobalOptions.SystemProfile.NationalCurrency))
                if (IsCrossRateAuto)
                {
                    CrossCurrencyRate row;
                    switch (fieldName)
                    {
                        case "CurrencyRUB":
                            myCurrencyRUB = 1;
                            return;
                        case "CurrencyUSD":
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.USD);
                            row.CurrencyUSD = 1 / rate;
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.EUR);
                            row.CurrencyEUR = row.CurrencyUSD / CurrencyEUR;
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.GBP);
                            row.CurrencyGBP = row.CurrencyUSD / CurrencyGBP;
                            return;
                        case "CurrencyEUR":
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.EUR);
                            row.CurrencyEUR = 1 / rate;
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.USD);
                            row.CurrencyUSD = row.CurrencyEUR / CurrencyUSD;
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.GBP);
                            row.CurrencyGBP = row.CurrencyEUR / CurrencyGBP;
                            return;
                        case "CurrencyGBP":
                            row = CurrencyList.First(_ => _.Currency.DocCode == CurrencyCode.GBP);
                            row.CurrencyRUB = 1 / rate;
                            return;
                    }
                }
        }

        public decimal GetCurrencyRate(CrossCurrencyRate row, Currency crs)
        {
            return GetCurrencyRate(row, crs.Name);
        }

        public decimal GetCurrencyRate(CrossCurrencyRate row, string crsName)
        {
            switch (crsName)
            {
                case CurrencyCode.RUBName:
                case CurrencyCode.RURName:
                    return row.CurrencyRUB;
                case CurrencyCode.USDName:
                    return row.CurrencyUSD;
                case CurrencyCode.EURName:
                    return row.CurrencyEUR;
                case CurrencyCode.GBPName:
                    return row.CurrencyGBP;
            }
            return -1;
        }

        public decimal GetCurrencyRate(Currency crs)
        {
            return GetCurrencyRate(crs.DocCode);
        }

        public decimal GetCurrencyRate(decimal crsCode)
        {
            switch (crsCode)
            {
                case CurrencyCode.RUB:
                    return CurrencyRUB;
                case CurrencyCode.USD:
                    return CurrencyUSD;
                case CurrencyCode.EUR:
                    return CurrencyEUR;
                case CurrencyCode.GBP:
                    return CurrencyGBP;
            }
            return -1;
        }

        public decimal GetRate(Currency baseCrs, Currency inCrs, Currency outCrs)
        {
            var row = CurrencyList.FirstOrDefault(_ => Equals(_.Currency, baseCrs));
            return GetCurrencyRate(row, inCrs) / GetCurrencyRate(row, outCrs) == 0 ? -1 : GetCurrencyRate(row, outCrs);
        }

        public decimal GetRate(CrossCurrencyRate row, Currency inCrs, Currency outCrs)
        {
            return GetCurrencyRate(row, inCrs) / GetCurrencyRate(row, outCrs) == 0 ? -1 : GetCurrencyRate(row, outCrs);
        }

        public decimal GetRate(Currency inCrs, Currency outCrs)
        {
            return GetCurrencyRate(inCrs) / (GetCurrencyRate(outCrs) == 0 ? -1 : GetCurrencyRate(outCrs));
        }

        public void SetRates(DateTime date)
        {
            CurrencyList.Clear();
            var rates = CurrencyRate.GetRate(date);
            foreach (var c in MainReferences.Currencies.Values.Where(_ => _.DocCode == CurrencyCode.RUB))
                CurrencyList.Add(new CrossCurrencyRate
                {
                    Currency = c,
                    CurrencyRUB = 1,
                    CurrencyEUR = rates[MainReferences.Currencies[CurrencyCode.EUR]],
                    CurrencyGBP = rates[MainReferences.Currencies[CurrencyCode.GBP]],
                    CurrencyUSD = rates[MainReferences.Currencies[CurrencyCode.USD]]
                });
        }
    }

    public static class DataAnnotationsCrossCurrencyRate
    {
        public static void BuildMetadata(MetadataBuilder<CrossCurrencyRate> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.Note).NotAutoGenerated();
        }
    }
}