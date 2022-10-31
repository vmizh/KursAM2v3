using System;
using Core.Helper;
using KursDomain.References;
using Currency = Core.EntityViewModel.CommonReferences.Currency;
using Kontragent = KursDomain.Documents.CommonReferences.Kontragent.Kontragent;
using Nomenkl = Core.EntityViewModel.NomenklManagement.Nomenkl;

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
