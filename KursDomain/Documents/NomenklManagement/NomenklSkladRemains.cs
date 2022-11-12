using System;
using KursDomain.References;

namespace KursDomain.Documents.NomenklManagement;

public class NomenklSkladRemains
{
    public Nomenkl Nomenkl { set; get; }
    public string NomenklName { set; get; }
    public string NomenklNumber { set; get; }
    public string NomenklUchetCurrencyName { set; get; }
    public References.Currency NomenklCurrency { set; get; }
    public string NomenklCurrencyNmae { set; get; }
    public References.Warehouse Store { set; get; }
    public string StoreName { set; get; }
    public DateTime LastOperDate { set; get; }
    public decimal QuantityAll { set; get; }
}
