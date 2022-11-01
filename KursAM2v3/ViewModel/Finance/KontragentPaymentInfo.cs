using Core;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace KursAM2.ViewModel.Finance
{
    public class KontragentOperationInfo : KONTR_BALANS_OPER_ARCViewModel
    {
        public KontragentOperationInfo(KONTR_BALANS_OPER_ARC entity)
            : base(entity)
        {
        }

        public string KontragentCrsName => MainReferences.GetKontragent(KONTR_DC).BalansCurrency.Name;

        public string OperationCrsName
            =>
                MainReferences.Currencies.ContainsKey(OPER_CRS_DC)
                    ? MainReferences.Currencies[OPER_CRS_DC].Name
                    : "не укзана";
    }
}
