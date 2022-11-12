using Data;
using KursDomain;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Finance
{
    public class KontragentOperationInfo : KONTR_BALANS_OPER_ARCViewModel
    {
        public KontragentOperationInfo(KONTR_BALANS_OPER_ARC entity)
            : base(entity)
        {
        }

        public string KontragentCrsName => ((IName)GlobalOptions.ReferencesCache.GetKontragent(KONTR_DC).Currency).Name;

        public string OperationCrsName
            =>
                ((IName)GlobalOptions.ReferencesCache.GetCurrency(OPER_CRS_DC))?.Name ?? "не укзана";
    }
}
