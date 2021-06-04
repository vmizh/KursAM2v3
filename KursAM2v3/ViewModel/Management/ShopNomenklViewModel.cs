using System.Collections.ObjectModel;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Management
{
    public class ShopNomenklViewModel : RSViewModelBase
    {
        public ShopNomenklViewModel()
        {
            Documents = new ObservableCollection<ShopDocumentViewModel>();
        }

        public Nomenkl Nomenkl { set; get; }
        public string NomenklNumber { set; get; }
        public decimal Quantity { set; get; }
        public decimal Summa { set; get; }
        public decimal Price { set; get; }
        public decimal CostOne { set; get; }
        public decimal Cost { set; get; }
        public decimal Result { set; get; }
        public ObservableCollection<ShopDocumentViewModel> Documents { set; get; }
    }
}