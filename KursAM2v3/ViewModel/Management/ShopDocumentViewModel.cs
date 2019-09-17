using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Management
{
    public class ShopDocumentViewModel : KursViewModelBase
    {
        public string NomenklNumber { set; get; }
        public decimal Quantity { set; get; }
        public decimal Summa { set; get; }
        public decimal Price { set; get; }
        public decimal Cost { set; get; }
        public decimal Result { set; get; }
    }
}