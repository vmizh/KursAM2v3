using System;
using Data;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Finance
{
    public class PurchaseRow : TD_24ViewModel
    {
        public PurchaseRow(TD_24 entity) : base(entity)
        {
        }

        public decimal AveragePrice { set; get; }
        public decimal CBRate { set; get; }
        public string PostName => ((IName)GlobalOptions.ReferencesCache.GetKontragent(TD_26.SD_26.SF_POST_DC)).Name;

        // ReSharper disable once PossibleInvalidOperationException
        public string CurrencyName =>
            ((IName)GlobalOptions.ReferencesCache.GetCurrency(TD_26.SD_26.SF_CRS_DC.Value)).Name;

        public decimal SummaPurchase
            => Math.Round(((TD_26.SFT_ED_CENA ?? 0) + (TD_26.SFT_SUMMA_NAKLAD ?? 0) / TD_26.SFT_KOL) * DDT_KOL_PRIHOD,
                2);
    }
}
