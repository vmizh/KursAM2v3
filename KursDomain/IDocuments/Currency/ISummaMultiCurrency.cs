using System;

namespace Core
{
    public interface ISummaMultiCurrency
    {
        decimal SummaCHF { set; get; }
        decimal SummaEUR { set; get; }
        decimal SummaGBP { set; get; }
        decimal SummaRUB { set; get; }
        decimal SummaSEK { set; get; }
        decimal SummaUSD { set; get; }
        decimal SummaCNY { set; get; }
    }

    public interface INomenklPriceMultiCurrency
    {
        decimal PriceCHF {  get; set; }
        decimal PriceEUR { set; get; }
        decimal PriceGBP { set; get; }
        decimal PriceRUB { set; get; }
        decimal PriceSEK { set; get; }
        decimal PriceUSD { set; get; }
        decimal PriceCNY { set; get; }

        decimal SummaCHF { set; get; }
        decimal SummaEUR { set; get; }
        decimal SummaGBP { set; get; }
        decimal SummaRUB { set; get; }
        decimal SummaSEK { set; get; }
        decimal SummaUSD { set; get; }
        decimal SummaCNY { set; get; }
    }

    public interface INomenklPriceWithNakladMultiCurrency : INomenklPriceMultiCurrency
    {
        decimal PriceWithNakladCHF { set; get; }
        decimal PriceWithNakladEUR { set; get; }
        decimal PriceWithNakladGBP { set; get; }
        decimal PriceWithNakladRUB { set; get; }
        decimal PriceWithNakladSEK { set; get; }
        decimal PriceWithNakladUSD { set; get; }
        decimal PriceWithNakladCNY { set; get; }

        decimal SummaWithNakladCHF { set; get; }
        decimal SummaWithNakladEUR { set; get; }
        decimal SummaWithNakladGBP { set; get; }
        decimal SummaWithNakladRUB { set; get; }
        decimal SummaWithNakladSEK { set; get; }
        decimal SummaWithNakladUSD { set; get; }
        decimal SummaWithNakladCNY { set; get; }
    }
}