using Core.EntityViewModel;
using Data;

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