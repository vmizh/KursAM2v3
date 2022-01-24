using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.StockHolder;

namespace KursAM2.ViewModel.StockHolder
{
    public class StockHolderBalanseItem : IStockHolderMoney
    {
        [Display(AutoGenerateField = true, Name = "Акционер",GroupName = "Наименование")]
        public StockHolderViewModel StockHolder { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "RUR")]
        public decimal NachRUB { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "RUR")]
        public decimal InRUB { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "RUR")]
        public decimal OutRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "RUR")]
        public decimal ResultRUB
        {
            get => OutRUB - NachRUB - InRUB;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "USD")]
        public decimal NachUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "USD")]
        public decimal InUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "USD")]
        public decimal OutUSD { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "USD")]
        public decimal ResultUSD
        {
            get => OutUSD - NachUSD - InUSD;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "EUR")]
        public decimal NachEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "EUR")]
        public decimal InEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "EUR")]
        public decimal OutEUR { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "EUR")]
        public decimal ResultEUR
        {
            get => OutEUR - NachEUR - InEUR;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "GBP")]
        public decimal NachGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "GBP")]
        public decimal InGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "GBP")]
        public decimal OutGBP { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "GBP")]
        public decimal ResultGBP
        {
            get => OutGBP - NachGBP - InGBP;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CHF")]
        public decimal NachCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CHF")]
        public decimal InCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CHF")]
        public decimal OutCHF { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "CHF")]
        public decimal ResultCHF
        {
            get => OutCHF - NachCHF - InCHF;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "SEK")]
        public decimal NachSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "SEK")]
        public decimal InSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "SEK")]
        public decimal OutSEK { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "SEK")]
        public decimal ResultSEK
        {
            get => OutSEK - NachSEK - InSEK;
            set => throw new System.NotImplementedException();
        }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CNY")]
        public decimal NachCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CNY")]
        public decimal InCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CNY")]
        public decimal OutCNY { set; get; }
        [Display(AutoGenerateField = true, Name = "Результат", GroupName = "CNY")]
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