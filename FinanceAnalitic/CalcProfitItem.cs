using System.Collections.Generic;

namespace FinanceAnalitic
{
    /// <summary>
    ///     Расчет позиции счета прибылей и убытков
    /// </summary>
    public abstract class CalcProfitItem : IProfitCurrencyList
    {
        public TypeProfitItem Type { set; get; }
        public List<IProfitAndLossesBase> OperationList = new List<IProfitAndLossesBase>();

        public abstract void LoadOperations();
        public decimal ProfitRUB { get; set; }
        public decimal LossRUB { get; set; }
        public decimal ResultRUB { get; set; }
        public decimal ProfitUSD { get; set; }
        public decimal LossUSD { get; set; }
        public decimal ResultUSD { get; set; }
        public decimal ProfitEUR { get; set; }
        public decimal LossEUR { get; set; }
        public decimal ResultEUR { get; set; }
        public decimal ProfitSEK { get; set; }
        public decimal LossSEK { get; set; }
        public decimal ResultSEK { get; set; }
        public decimal ProfitGBP { get; set; }
        public decimal LossGBP { get; set; }
        public decimal ResultGBP { get; set; }
        public decimal ProfitCHF { get; set; }
        public decimal LossCHF { get; set; }
        public decimal ResultCHF { get; set; }
        public decimal ProfitOther { get; set; }
        public decimal LossOther { get; set; }
        public decimal ResultOther { get; set; }
        public decimal ProfitCNY { get; set; }
        public decimal LossCNY { get; set; }
        public decimal ResultCNY { get; set; }
    }
}