using Core.EntityViewModel;

namespace Core.ViewModel.Common
{
    public struct Money
    {
        public Currency Currency { set; get; }
        public decimal Summa { set; get; }

        public Money(Currency crs, decimal summa)
        {
            Currency = crs;
            Summa = summa;
        }

        public override string ToString()
        {
            return $"{Summa:n2} {Currency?.Name}";
        }
    }
}