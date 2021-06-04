using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Invoices.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    public sealed class KontragentGruzoRequisite : SD_43_GRUZOViewModel
    {
        public KontragentGruzoRequisite()
        {
        }

        public KontragentGruzoRequisite(SD_43_GRUZO entity) : base(entity)
        {
        }
    }
}