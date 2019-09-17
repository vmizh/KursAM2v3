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
    }
}