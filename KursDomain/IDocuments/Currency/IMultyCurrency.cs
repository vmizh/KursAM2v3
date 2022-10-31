namespace Core
{
    public interface IMultyCurrency
    {
        decimal ProfitCHF { set; get; }
        decimal ProfitEUR { set; get; }
        decimal ProfitGBP { set; get; }
        decimal ProfitRUB { set; get; }
        decimal ProfitSEK { set; get; }
        decimal ProfitUSD { set; get; }
        decimal LossCHF { set; get; }
        decimal LossEUR { set; get; }
        decimal LossGBP { set; get; }
        decimal LossRUB { set; get; }
        decimal LossSEK { set; get; }
        decimal LossUSD { set; get; }
        decimal ResultCHF { get; }
        decimal ResultEUR { get; }
        decimal ResultGBP { get; }
        decimal ResultRUB { get; }
        decimal ResultSEK { get; }
        decimal ResultUSD { get; }
    }

    public interface IMultyWithDilerCurrency : IMultyCurrency
    {
        decimal DilerCHF { get; set; }
        decimal DilerEUR { get; set; }
        decimal DilerGBP { get; set; }
        decimal DilerRUB { get; set; }
        decimal DilerSEK { get; set; }
        decimal DilerUSD { get; set; }
    }
}