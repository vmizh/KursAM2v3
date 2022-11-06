using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.References;

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
