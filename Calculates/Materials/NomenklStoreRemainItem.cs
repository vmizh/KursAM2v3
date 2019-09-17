namespace Calculates.Materials
{
    /// <summary>
    /// остатки товаров на складе. 
    /// </summary>
    public class NomenklStoreRemainItem
    {
        public decimal NomenklDC { set; get; }
        public decimal NomCurrencyDC { set; get; }
        public string NomenklName { set; get; }
        public decimal SkladDC { set; get; }
        public decimal Prihod { set; get; }
        public decimal Rashod { set; get; }
        public decimal Remain { set; get; }
        public decimal PriceWithNaklad { set; get; }
        public decimal Price { set; get; }
        public decimal Summa { set; get; }
        public decimal SummaWithNaklad { set; get; }


    }
}