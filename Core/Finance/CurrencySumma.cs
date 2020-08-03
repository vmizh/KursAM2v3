using Core.EntityViewModel;
using Data;

namespace Core.Finance
{
    public class CurrencySumma
    {
        public Currency Currency { set; get; }
        public decimal Summa { set; get; }

        public CURRENCY_RATES_CB Rate { set; get; }
    }
}