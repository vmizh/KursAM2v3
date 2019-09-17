using System;
using Core.EntityViewModel;

namespace Core.ViewModel.Common
{
    public class NomenklRemains
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklName { set; get; }
        public string NomenklNumber { set; get; }
        public string NomenklUchetCurrencyName { set; get; }
        public DateTime LastOperDate { set; get; }
        public decimal QuantityAll { set; get; }
    }
}