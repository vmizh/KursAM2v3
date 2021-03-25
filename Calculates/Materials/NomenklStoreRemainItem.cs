using System;
using System.Collections;
using System.Globalization;
using Core.EntityViewModel;
using DevExpress.Mvvm.DataAnnotations;

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
        public decimal StoreDC { set; get; }
        public string StoreName { set; get; }
        public decimal Prihod { set; get; }
        public decimal Rashod { set; get; }
        public decimal Remain { set; get; }
        public decimal PriceWithNaklad { set; get; }
        public decimal Price { set; get; }
        public decimal Summa { set; get; }
        public decimal SummaWithNaklad { set; get; }
    }

    public class NomenklCalcMove
    {
        public decimal NomDC { set; get; }
        public decimal StoreDC { set; get; }
        public string StoreName { set; get; }
        public string NomNomenkl { set; get; }
        public string NomName { set; get; }
        public DateTime Date { set; get; }
        public decimal Start { set; get; }
        public decimal Prihod { set; get; }
        public decimal Rashod { set; get; }
        public decimal Ostatok { set; get; }
        public decimal Price { set; get; }
        public decimal PriceWithNaklad { set; get; }
        public decimal MoneyPrihod { set; get; }
        public decimal MoneyPrihodWithNaklad { set; get; }
        public decimal MoneyRashod { set; get; }
        public decimal MoneyRashodWithNaklad { set; get; }
        public decimal StartPrice { set; get; }
        public decimal StartPriceWithNaklad { set; get; }
    }
}