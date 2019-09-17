using System;
using Core.EntityViewModel;

namespace Core.ViewModel.Common
{
    public class NomenklSkladRemains
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklName { set; get; }
        public string NomenklNumber { set; get; }
        public string NomenklUchetCurrencyName { set; get; }
        public Currency NomenklCurrency { set; get; }
        public string NomenklCurrencyNmae { set; get; }
        public Warehouse Store { set; get; }
        public string StoreName { set; get; }
        public DateTime LastOperDate { set; get; }
        public decimal QuantityAll { set; get; }
    }
}