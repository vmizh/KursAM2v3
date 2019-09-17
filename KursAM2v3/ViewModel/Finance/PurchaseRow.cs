using System;
using Core;
using Core.EntityViewModel;
using Data;

namespace KursAM2.ViewModel.Finance
{
    public class PurchaseRow : TD_24ViewModel
    {
        public PurchaseRow()
        {
        }

        public PurchaseRow(TD_24 entity) : base(entity)
        {
        }

        public decimal AveragePrice { set; get; }
        public decimal CBRate { set; get; }
        public string PostName => MainReferences.GetKontragent(TD_26.SD_26.SF_POST_DC).Name;
        public string CurrencyName => MainReferences.Currencies[TD_26.SD_26.SF_CRS_DC.Value].Name;

        public decimal SummaPurchase
            => Math.Round(((TD_26.SFT_ED_CENA ?? 0) + (TD_26.SFT_SUMMA_NAKLAD ?? 0) / TD_26.SFT_KOL) * DDT_KOL_PRIHOD,
                2);
    }
}