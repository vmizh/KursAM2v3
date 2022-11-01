using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using KursDomain.Documents.CommonReferences;

namespace KursAM2.ViewModel.Logistiks
{
    public class DocumentRowSearchViewModel : RSViewModelBase
    {
        public string NomName { set; get; }
        public decimal Quantity { set; get; }
        public decimal Price { set; get; }
        public decimal Summa { set; get; }
        public bool IsUsluga { set; get; }
        public CountriesViewModel Country { set; get; }
    }
}
