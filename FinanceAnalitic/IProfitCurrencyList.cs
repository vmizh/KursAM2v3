namespace FinanceAnalitic
{
    public interface IProfitCurrencyList
    {
        decimal ProfitRUB { set; get; }
        decimal LossRUB { set; get; }
        decimal ResultRUB { set; get; }

        decimal ProfitUSD { set; get; }
        decimal LossUSD { set; get; }
        decimal ResultUSD { set; get; }

        decimal ProfitEUR { set; get; }
        decimal LossEUR { set; get; }
        decimal ResultEUR { set; get; }

        decimal ProfitSEK { set; get; }
        decimal LossSEK { set; get; }
        decimal ResultSEK { set; get; }

        decimal ProfitGBP { set; get; }
        decimal LossGBP { set; get; }
        decimal ResultGBP { set; get; }

        decimal ProfitCHF { set; get; }
        decimal LossCHF { set; get; }
        decimal ResultCHF { set; get; }

        decimal ProfitOther { set; get; }
        decimal LossOther { set; get; }
        decimal ResultOther { set; get; }

        decimal ProfitCNY { set; get; }
        decimal LossCNY { set; get; }
        decimal ResultCNY { set; get; }
    }
}