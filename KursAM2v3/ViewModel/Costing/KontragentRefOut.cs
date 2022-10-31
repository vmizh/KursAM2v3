using System.Collections.ObjectModel;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Invoices.EntityViewModel;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace KursAM2.ViewModel.Costing
{
    public class KontragentRefOut : KONTRAGENT_REF_OUTViewModel
    {
        public KontragentRefOut()
        {
            Requisite = new ObservableCollection<KontragentRefOutRow>();
        }

        public KontragentRefOut(KONTRAGENT_REF_OUT entity)
            : base(entity)
        {
            Requisite = new ObservableCollection<KontragentRefOutRow>();
            if (entity.KONTRAGENT_REF_OUT_REQUISITE != null && entity.KONTRAGENT_REF_OUT_REQUISITE.Count > 0)
                foreach (var row in entity.KONTRAGENT_REF_OUT_REQUISITE)
                    Requisite.Add(new KontragentRefOutRow(row));
        }

        public ObservableCollection<KontragentRefOutRow> Requisite { set; get; }
    }
}
