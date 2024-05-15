using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.Documents.Currency;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public class NomenklRemainsOnSkladWithPrice
    {
        [Display(Name = "Номенклатура")]
        [ReadOnly(true)]
        public Nomenkl Nomenkl { set; get; }

        [Display(Name = "Ном.№")]
        [ReadOnly(true)]
        public string NomNomenkl => Nomenkl?.NomenklNumber;

        [Display(Name = "Валюта")]
        [ReadOnly(true)]
        public Currency Currency => (Currency)Nomenkl?.Currency;

        [Display(Name = "Кол-во на складе")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n4")]
        public decimal Quantity { set; get; }

        [Display(AutoGenerateField = false)] public Prices Prices { set; get; }

        [Display(Name = "Цена")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Price => Prices.Price;

        [Display(Name = "Сумма")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Summa => Prices.Price * Quantity;

        [Display(Name = "Цена с накл.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal PriceWithNaklad => Prices.PriceWithNaklad;

        [Display(Name = "Сумма с накл.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaWithPrices => Prices.PriceWithNaklad * Quantity;

        [Display(Name = "Макс. отгруз.")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal MaxOtgruz { set; get; }


        [Display(Name = "Отгрузить")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal FactOtgruz { set; get; }
    }
}
