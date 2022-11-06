using System;
using KursDomain.Documents.Management;
using KursDomain.References;

namespace FinanceAnalitic
{
    public interface IProfitAndLossesBase : IProfitCurrencyList
    {
        TypeProfitAndLossCalc CalcType { set; get; }
        Guid GroupId { set; get; }
        decimal Quantity { set; get; }
        decimal Price { set; get; }
        string CurrencyName { set; get; }
        string Kontragent { set; get; }

        Currency Currency { set; get; }

        Nomenkl Nomenkl { set; get; }
        Kontragent KontragentBase { set; get; }
        DateTime Docdate { set; get; }
        DateTime Date { set; get; }
        DateTime RowDate { set; get; }
        string DocNum { set; get; }
    }
}
