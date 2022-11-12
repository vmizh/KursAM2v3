using System;
using KursDomain.References;

namespace Calculates.Materials
{
    public class NomenklCosts
    {
        public Nomenkl Nomenkl { set; get; }
        public Warehouse Warehouse { set; get; }
        public Currency Currency { set; get; }
        public decimal Quantity { set; get; }
        public decimal Price { set; get; }
        public DateTime Date { set; get; }
    }
}
