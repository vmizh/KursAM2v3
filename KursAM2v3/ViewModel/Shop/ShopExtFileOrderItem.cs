using System.Diagnostics;

namespace KursAM2.ViewModel.Shop
{
    [DebuggerDisplay("{OfferId,nq} {Name}")]
    public class ShopExtFileOrderItem
    {
        public string OfferId { set; get; }
        public decimal Price { set; get; }
        public decimal Count { set; get; }
        public string Name { set; get; }
    }
}
