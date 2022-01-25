using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.StockHolder;

namespace KursAM2.ViewModel.StockHolder
{
    public class StockHolderBalanseItem : IStockHolderMoney
    {
        [Display(AutoGenerateField = true, Name = "Акционер",GroupName = "Наименование")]
        public StockHolderViewModel StockHolder { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "RUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachRUB { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "RUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal InRUB { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "RUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "RUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultRUB
        {
            get => OutRUB - NachRUB - InRUB;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "USD"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "USD"), DisplayFormat(DataFormatString = "n2")]
        public decimal InUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "USD"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "USD"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultUSD
        {
            get => OutUSD - NachUSD - InUSD;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "EUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "EUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal InEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "EUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "EUR"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultEUR
        {
            get => OutEUR - NachEUR - InEUR;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "GBP"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "GBP"), DisplayFormat(DataFormatString = "n2")]
        public decimal InGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "GBP"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "GBP"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultGBP
        {
            get => OutGBP - NachGBP - InGBP;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CHF"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CHF"), DisplayFormat(DataFormatString = "n2")]
        public decimal InCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CHF"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "CHF"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultCHF
        {
            get => OutCHF - NachCHF - InCHF;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "SEK"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "SEK"), DisplayFormat(DataFormatString = "n2")]
        public decimal InSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "SEK"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "SEK"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultSEK
        {
            get => OutSEK - NachSEK - InSEK;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CNY"), DisplayFormat(DataFormatString = "n2")]
        public decimal NachCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CNY"), DisplayFormat(DataFormatString = "n2")]
        public decimal InCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CNY"), DisplayFormat(DataFormatString = "n2")]
        public decimal OutCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "CNY"), DisplayFormat(DataFormatString = "n2")]
        public decimal ResultCNY
        {
            get => OutCNY - NachCNY - InCNY;
            set => throw new System.NotImplementedException();
        }
    }

    public class StockHolderBalanseItemPeriod : DatePeriod, IStockHolderMoney
    {
        public decimal NachRUB { get; set; }
        public decimal InRUB { get; set; }
        public decimal OutRUB { get; set; }
        public decimal ResultRUB { get; set; }
        public decimal NachUSD { get; set; }
        public decimal InUSD { get; set; }
        public decimal OutUSD { get; set; }
        public decimal ResultUSD { get; set; }
        public decimal NachEUR { get; set; }
        public decimal InEUR { get; set; }
        public decimal OutEUR { get; set; }
        public decimal ResultEUR { get; set; }
        public decimal NachGBP { get; set; }
        public decimal InGBP { get; set; }
        public decimal OutGBP { get; set; }
        public decimal ResultGBP { get; set; }
        public decimal NachCHF { get; set; }
        public decimal InCHF { get; set; }
        public decimal OutCHF { get; set; }
        public decimal ResultCHF { get; set; }
        public decimal NachSEK { get; set; }
        public decimal InSEK { get; set; }
        public decimal OutSEK { get; set; }
        public decimal ResultSEK { get; set; }
        public decimal NachCNY { get; set; }
        public decimal InCNY { get; set; }
        public decimal OutCNY { get; set; }
        public decimal ResultCNY { get; set; }
    }

    public interface IStockHolderMoney
    {
         decimal NachRUB { set; get; }
         decimal InRUB { set; get; }
         decimal OutRUB { set; get; }
         decimal ResultRUB { set; get; }

         decimal NachUSD { set; get; }
         decimal InUSD { set; get; }
         decimal OutUSD { set; get; }
         decimal ResultUSD { set; get; }

         decimal NachEUR { set; get; }
         decimal InEUR { set; get; }
         decimal OutEUR { set; get; }
         decimal ResultEUR  { set; get; }

         decimal NachGBP { set; get; }
         decimal InGBP { set; get; }
         decimal OutGBP { set; get; }
         decimal ResultGBP { set; get; }

         decimal NachCHF { set; get; }
         decimal InCHF { set; get; }
         decimal OutCHF { set; get; }
         decimal ResultCHF { set; get; }

         decimal NachSEK { set; get; }
         decimal InSEK { set; get; }
         decimal OutSEK { set; get; }
         decimal ResultSEK { set; get; }

         decimal NachCNY { set; get; }
         decimal InCNY { set; get; }
         decimal OutCNY { set; get; }
         decimal ResultCNY { set; get; }
    }
}