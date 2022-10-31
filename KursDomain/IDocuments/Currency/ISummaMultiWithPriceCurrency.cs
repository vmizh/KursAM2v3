namespace Core
{
    public interface ISummaMultiWithPriceCurrency : ISummaMultiCurrency
    {
        decimal PriceCHF { set; get; }
        decimal PriceEUR { set; get; }
        decimal PriceGBP { set; get; }
        decimal PriceRUB { set; get; }
        decimal PriceSEK { set; get; }
        decimal PriceUSD { set; get; }
        decimal PriceCNY { set; get; }
    }
}