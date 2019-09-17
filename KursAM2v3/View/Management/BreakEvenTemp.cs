using System;

namespace KursAM2.View.Management
{
    public class BreakEvenTemp
    {
        public int IsUsluga { set; get; }
        public DateTime DATE { set; get; }
        public decimal NomenklDC { set; get; }
        public decimal CO { set; get; }
        public decimal KontragentDC { set; get; }
        public decimal Currency { set; get; }
        public decimal Manager { set; get; }
        public decimal Quantity { set; get; }
        public decimal Price { set; get; }
        public decimal SummaKontrCrs { set; get; }
        public decimal NomSumm { set; get; }
        public DateTime SF_DATE { set; get; }
        public string SF_NUM { set; get; }
        public string SF_NOTES { set; get; }
        public string NAKL_NUM { set; get; }
        public string NAKL_NOTES { set; get; }
        public decimal Diler { set; get; }
        public decimal DilerSumma { set; get; }
        public decimal NomenklSumWOReval { set; get; }
        public decimal KontrCrsDC { set; get; }
        public decimal NomenklCrsDC { set; get; }
        public string TypeProdName { set; get; }
        public decimal? SaleTaxPrice { set; get; }
        public decimal? SaleTaxRate { set; get; }
        public bool IsSaleTax { set; get; }
        public decimal DocDC { set; get; }
        public decimal TypeDC { set; get; }
    }
}