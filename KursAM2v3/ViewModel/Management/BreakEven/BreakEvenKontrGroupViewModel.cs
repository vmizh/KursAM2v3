using Core.EntityViewModel.CommonReferences.Kontragent;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace KursAM2.ViewModel.Management.BreakEven
{
    public class BreakEvenKontrGroupViewModel : CommonRow
    {
        public decimal KontrBalans { set; get; }
        public decimal KontrCurrentBalans { set; get; }
        public string KontrCrs { set; get; }
        public Kontragent Kontragent { set; get; }
    }
}
