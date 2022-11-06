using System;
using Core;
using Data;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Finance
{
    public class SaleTaxNomenkl : TD_24ViewModel
    {
        private decimal myRateCost;
        private decimal myRateSumma;


        public SaleTaxNomenkl(TD_24 entity, decimal costPrice, decimal costSumma, decimal rate, string saleNote)
            : base(entity)
        {
            CostPrice = costPrice;
            CostSumma = costSumma;
            Entity.SaleTaxRate = rate;
            myRateCost = entity.SaleTaxPrice ?? 0;
            myRateSumma = (entity.SaleTaxPrice ?? 0) * entity.DDT_KOL_RASHOD;
            RateResult = Math.Round(SaleSumma - SaleDilerSumma - CostPrice * (Entity.SaleTaxRate ?? 0) * Quantity, 2);
            Entity.SaleTaxNote = saleNote;
        }

        public DateTime Date => SD_24.DD_DATE;
        public int AccountInNum => TD_84.SD_84.SF_IN_NUM;
        public string AccountOutNum => TD_84.SD_84.SF_OUT_NUM;
        public DateTime AccountDate => TD_84.SD_84.SF_DATE;
        public string AccountCreator => TD_84.SD_84.CREATOR;

        // ReSharper disable once PossibleInvalidOperationException
        public string SkladName => MainReferences.Warehouses[(decimal)SD_24.DD_SKLAD_OTPR_DC].Name;
        public string KontragentName => MainReferences.GetKontragent(SD_24.DD_KONTR_POL_DC).Name;
        public string NomenklName => MainReferences.GetNomenkl(DDT_NOMENKL_DC).Name;
        public string NomenklNomNumber => MainReferences.GetNomenkl(DDT_NOMENKL_DC).NomenklNumber;
        public string NomenklCurrencyName => ((IName)MainReferences.GetNomenkl(DDT_NOMENKL_DC).Currency).Name;
        public decimal Quantity => DDT_KOL_RASHOD;
        public string SaleCurrency => MainReferences.Currencies[TD_84.SD_84.SF_CRS_DC].Name;
        public decimal CostPrice { set; get; }
        public decimal CostSumma { set; get; }

        public decimal Rate
        {
            get => Entity.SaleTaxRate ?? 0;
            set
            {
                if (Entity.SaleTaxRate == value) return;
                Entity.SaleTaxRate = value;
                myRateCost = Math.Round(CostPrice * (Entity.SaleTaxRate ?? 0), 2);
                myRateSumma = Math.Round(CostSumma * (Entity.SaleTaxRate ?? 0), 2);
                RateResult = Math.Round(SaleSumma - SaleDilerSumma - CostPrice * (Entity.SaleTaxRate ?? 0) * Quantity,
                    2);
                Update();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RateCost));
                RaisePropertyChanged(nameof(RateSumma));
                RaisePropertyChanged(nameof(RateResult));
                RaisePropertyChanged(nameof(Delta));
                RaisePropertyChanged(nameof(IsResult));
                RaisePropertyChanged(nameof(Result));
            }
        }

        public decimal RateSumma
        {
            get => myRateSumma;
            set
            {
                if (myRateSumma == value) return;
                myRateSumma = value;
                Entity.SaleTaxRate = Math.Round(myRateSumma / CostSumma, 4);
                myRateCost = Math.Round(myRateSumma / Quantity, 2);
                RateResult = Math.Round(SaleSumma - SaleDilerSumma - CostPrice * (Entity.SaleTaxRate ?? 0) * Quantity,
                    2);
                Update();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RateCost));
                RaisePropertyChanged(nameof(Rate));
                RaisePropertyChanged(nameof(RateResult));
                RaisePropertyChanged(nameof(Delta));
                RaisePropertyChanged(nameof(IsResult));
                RaisePropertyChanged(nameof(Result));
                RaisePropertyChanged();
            }
        }

        public decimal RateResult { set; get; }

        public decimal RateCost
        {
            get => myRateCost;
            set
            {
                if (myRateCost == value) return;
                myRateCost = value;
                Entity.SaleTaxRate = Math.Round(myRateCost / CostPrice, 4);
                myRateSumma = Math.Round(myRateCost * Quantity, 2);
                Update();
                RateResult = Math.Round(SaleSumma - SaleDilerSumma - CostPrice * (Entity.SaleTaxRate ?? 0) * Quantity,
                    2);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RateSumma));
                RaisePropertyChanged(nameof(Rate));
                RaisePropertyChanged(nameof(RateResult));
                RaisePropertyChanged(nameof(Delta));
                RaisePropertyChanged(nameof(IsResult));
                RaisePropertyChanged(nameof(Result));
                RaisePropertyChanged();
            }
        }

        public new bool IsSaleTax
        {
            get => Entity.IsSaleTax;
            set
            {
                if (Entity.IsSaleTax == value) return;
                Entity.IsSaleTax = value;
                Update();
                RaisePropertyChanged(nameof(Delta));
                RaisePropertyChanged();
            }
        }

        public string SaleNote
        {
            get => Entity.SaleTaxNote;
            set
            {
                if (Entity.SaleTaxNote == value) return;
                Update();
                Entity.SaleTaxNote = value;
                RaisePropertyChanged();
            }
        }

        public decimal CBRate { set; get; }
        public decimal CBCostSumma { set; get; }
        public decimal Result => SaleSumma - SaleDilerSumma - CBCostSumma;

        public bool? IsResult
        {
            get
            {
                if (Delta > 0)
                    return true;
                if (Delta < 0) return false;
                return null;
            }
        }

        public decimal Delta => IsSaleTax ? RateResult - Result : 0;

        public void UpdatePropertyChanged()
        {
            RaisePropertyChanged(nameof(Rate));
            RaisePropertyChanged(nameof(RateCost));
            RaisePropertyChanged(nameof(RateSumma));
            RaisePropertyChanged(nameof(IsResult));
            RaisePropertyChanged(nameof(RateResult));
            RaisePropertyChanged(nameof(Delta));
            RaisePropertyChanged(nameof(Result));
        }

        public void SystemUpdateRate(decimal rate)
        {
            IsAutoTax = true;
            IsSaleTax = true;
            SaleTaxRate = rate;
            SaleTaxPrice = Math.Round(CostPrice * rate, 2);
            myRateSumma = (SaleTaxPrice ?? 0) * Quantity;
            TaxUpdater = "Система";
        }

        public void SystemUpdatePrice(decimal price)
        {
            IsAutoTax = true;
            IsSaleTax = true;
            SaleTaxPrice = price;
            myRateSumma = price * Quantity;
            SaleTaxRate = Math.Round(price / CostPrice, 4);
            TaxUpdater = "Система";
        }

        private void Update()
        {
            IsAutoTax = false;
            TaxUpdater = string.IsNullOrEmpty(GlobalOptions.UserInfo.FullName)
                ? GlobalOptions.UserInfo.Name
                : GlobalOptions.UserInfo.FullName;
        }

        // ReSharper disable PossibleInvalidOperationException
        public decimal SalePrice => TD_84.SFT_ED_CENA ?? 0;
        public decimal SaleDilerSumma => TD_84.SFT_NACENKA_DILERA ?? 0;
        public decimal SaleSumma => TD_84.SFT_SUMMA_K_OPLATE ?? 0;

        // ReSharper restore PossibleInvalidOperationException
    }
}
