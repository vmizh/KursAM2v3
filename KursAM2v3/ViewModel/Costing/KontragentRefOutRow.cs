using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Invoices.EntityViewModel;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace KursAM2.ViewModel.Costing
{
    public class KontragentRefOutRow : KONTRAGENT_REF_OUT_REQUISITEViewModel
    {
        public KontragentRefOutRow()
        {
        }

        public KontragentRefOutRow(KONTRAGENT_REF_OUT_REQUISITE entity)
            : base(entity)
        {
        }
    }
}
