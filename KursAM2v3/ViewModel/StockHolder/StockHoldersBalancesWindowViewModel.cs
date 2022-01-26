using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.StockHolder;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.View.StockHolder;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHoldersBalancesWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public StockHoldersBalancesWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            stockHoldersAll.Clear();
            StockHolders.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var sh in ctx.StockHolders.ToList()) stockHoldersAll.Add(new StockHolderViewModel(sh));

                var nachdata = ctx.StockHolderAccrualRows.Include(_ => _.StockHolders).ToList();
                var indata = ctx.SD_33.Where(_ => _.StockHolderId != null).ToList();
                var outdata = ctx.SD_34.Where(_ => _.StockHolderId != null).ToList();

                var shIds = nachdata.Select(_ => _.StockHolderId).ToList();
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (Guid id in indata.Select(_ => _.StockHolderId))
                {
                    if (shIds.Contains(id)) continue;
                    shIds.Add(id);
                }

                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (Guid id in outdata.Select(_ => _.StockHolderId))
                {
                    if (shIds.Contains(id)) continue;
                    shIds.Add(id);
                }

                foreach (var id in shIds)
                {
                    var newItem = new StockHolderBalanseItem
                    {
                        StockHolder = stockHoldersAll.FirstOrDefault(_ => _.Id == id)
                    };
                    foreach (var s in nachdata.Where(_ => _.StockHolderId == id))
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CurrencyDC, s.Summa ?? 0, 0, 0);
                    foreach (var s in indata.Where(_ => _.StockHolderId == id))
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CRS_DC, 0, s.SUMM_ORD ?? 0, 0);

                    foreach (var s in outdata.Where(_ => _.StockHolderId == id))
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CRS_DC, 0, 0, s.SUMM_ORD ?? 0);
                    StockHolders.Add(newItem);
                }
            }

            SetCrsVisibleStockHolder();
        }

        #endregion

        #region Fields

        private StockHolderBalanseItem myCurrentStockHolder;
        private readonly List<StockHolderViewModel> stockHoldersAll = new List<StockHolderViewModel>();
        private StockHolderBalanseItem myCurrentPeriod;
        private List<StockHolderMoneyMove> StockHolderMoneyMoves { set; get; } =
            new List<StockHolderMoneyMove>();

        #endregion

        #region Properties

        public override string WindowName => "Лицевые счета акционеров";
        public override string LayoutName => "StockHoldersBalancesWindowViewModel";

        public ObservableCollection<StockHolderBalanseItem> StockHolders { set; get; } =
            new ObservableCollection<StockHolderBalanseItem>();

        public ObservableCollection<StockHolderBalanseItemPeriod> Periods { set; get; }
            = new ObservableCollection<StockHolderBalanseItemPeriod>();

        public ObservableCollection<StockHolderMoneyMove> MoneyMoves { set; get; } =
            new ObservableCollection<StockHolderMoneyMove>();

        public StockHolderBalanseItem CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (myCurrentPeriod == value) return;
                myCurrentPeriod = value;
                RaisePropertyChanged();
            }
        }

        public StockHolderBalanseItem CurrentStockHolder
        {
            get => myCurrentStockHolder;
            set
            {
                if (myCurrentStockHolder == value) return;
                LoadPeriods();
                myCurrentStockHolder = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void SetSumm(StockHolderBalanseItem item, decimal crsDC, decimal? sNach, decimal? sIn, decimal? sOut)
        {
            switch (crsDC)
            {
                case CurrencyCode.RUB:
                    item.NachRUB += sNach ?? 0;
                    item.InRUB += sIn ?? 0;
                    item.OutRUB += sOut ?? 0;
                    break;

                case CurrencyCode.USD:
                    item.NachUSD += sNach ?? 0;
                    item.InUSD += sIn ?? 0;
                    item.OutUSD += sOut ?? 0;
                    break;

                case CurrencyCode.EUR:
                    item.NachEUR += sNach ?? 0;
                    item.InEUR += sIn ?? 0;
                    item.OutEUR += sOut ?? 0;
                    break;

                case CurrencyCode.CHF:
                    item.NachCHF += sNach ?? 0;
                    item.InCHF += sIn ?? 0;
                    item.OutCHF += sOut ?? 0;
                    break;

                case CurrencyCode.CNY:
                    item.NachCNY += sNach ?? 0;
                    item.InCNY += sIn ?? 0;
                    item.OutCNY += sOut ?? 0;
                    break;

                case CurrencyCode.GBP:
                    item.NachGBP += sNach ?? 0;
                    item.InGBP += sIn ?? 0;
                    item.OutGBP += sOut ?? 0;
                    break;

                case CurrencyCode.SEK:
                    item.NachSEK += sNach ?? 0;
                    item.InSEK += sIn ?? 0;
                    item.OutSEK += sOut ?? 0;
                    break;
            }
        }


        private void LoadPeriods()
        {
            LoadMoneyMove();
            Periods.Clear();
            var dates = StockHolderMoneyMoves.Select(_ => _.DocumentDate).Distinct().ToList();
            var dPeriods = DatePeriod.GenerateIerarhy(dates, PeriodIerarhy.YearMonth).ToList();
            foreach (var newDate in dPeriods.Select(d => new StockHolderBalanseItemPeriod
                     {
                         Id = d.Id,
                         ParentId = d.ParentId,
                         DateStart = d.DateStart,
                         DateEnd = d.DateEnd,
                         Name = d.Name

                     }))
            {
                Periods.Add(newDate);
            }
        }

        private void LoadMoneyMove()
        {
            if (CurrentStockHolder == null) return;
            StockHolderMoneyMoves.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dataCashIn  = ctx.SD_33.Where(_ => _.StockHolderId == CurrentStockHolder.StockHolder.Id);
                var dataCashOut = ctx.SD_34.Where(_ => _.StockHolderId == CurrentStockHolder.StockHolder.Id);
                foreach (var d in dataCashOut)
                {
                    var newItem = new StockHolderMoneyMove
                    {
                        DocCode = d.DOC_CODE,
                        DocumentDate = d.DATE_ORD ?? DateTime.Today,
                        Currency = MainReferences.GetCurrency(d.CRS_DC),
                        DocumentName = "Расходный кассовый ордер",
                        DocumentNumber = d.NUM_ORD.ToString(),
                        SummaOut = d.SUMM_ORD ?? 0

                    };
                    StockHolderMoneyMoves.Add(newItem);
                }
            }
        }

        private void SetCrsVisibleStockHolder()
        {
            if (Form is StockHoldersBalancesView frm)
            {
                var bandUSD = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "USD");
                var bandRUB = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "RUB");
                var bandEUR = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "EUR");
                var bandSEK = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "SEK");
                var bandCNY = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "CNY");
                var bandCHF = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "CHF");
                var bandGBP = frm.GridControlStockHolder.Bands.FirstOrDefault(_ => (string)_.Header == "GBP");
                if (bandUSD != null)
                    bandUSD.Visible = !(StockHolders.Sum(_ => _.NachUSD) == 0 && StockHolders.Sum(_ => _.InUSD) == 0 &&
                                        StockHolders.Sum(_ => _.OutUSD) == 0);
                if (bandRUB != null)
                    bandRUB.Visible = !(StockHolders.Sum(_ => _.NachRUB) == 0 && StockHolders.Sum(_ => _.InRUB) == 0 &&
                                        StockHolders.Sum(_ => _.OutRUB) == 0);
                if (bandEUR != null)
                    bandEUR.Visible = !(StockHolders.Sum(_ => _.NachEUR) == 0 && StockHolders.Sum(_ => _.InEUR) == 0
                                                                              && StockHolders.Sum(_ => _.OutEUR) == 0);
                if (bandSEK != null)
                    bandSEK.Visible = !(StockHolders.Sum(_ => _.NachSEK) == 0 && StockHolders.Sum(_ => _.InSEK) == 0
                                                                              && StockHolders.Sum(_ => _.OutSEK) == 0);
                if (bandCNY != null)
                    bandCNY.Visible = !(StockHolders.Sum(_ => _.NachCNY) == 0 && StockHolders.Sum(_ => _.InCNY) == 0
                                                                              && StockHolders.Sum(_ => _.OutCNY) == 0);
                if (bandCHF != null)
                    bandCHF.Visible = !(StockHolders.Sum(_ => _.NachCHF) == 0 && StockHolders.Sum(_ => _.InCHF) == 0
                                                                              && StockHolders.Sum(_ => _.OutCHF) == 0);
                if (bandGBP != null)
                    bandGBP.Visible = !(StockHolders.Sum(_ => _.NachGBP) == 0 && StockHolders.Sum(_ => _.InGBP) == 0
                                                                              && StockHolders.Sum(_ => _.OutGBP) == 0);
            }
        }

        #endregion

        #region IDataErrorInfo

        #endregion
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class StockHolderMoneyMove
    {
        [Display(AutoGenerateField = true, Name = "Тип документ", GroupName = "Документ")]
        [Editable(false)]
        public string DocumentName { set; get; }

        [Display(AutoGenerateField = false)]
        [Editable(false)]
        public decimal? DocCode { set; get; }

        [Display(AutoGenerateField = false)]
        [Editable(false)]
        public int? Code { set; get; }

        [Display(AutoGenerateField = true, Name = "№", GroupName = "Документ")]
        [Editable(false)]
        public string DocumentNumber { set; get; }

        [Display(AutoGenerateField = true, Name = "Дата", GroupName = "Документ")]
        [Editable(false)]
        public DateTime DocumentDate { set; get; }

        [Display(AutoGenerateField = true, Name = "Валюта", GroupName = "Документ")]
        [Editable(false)]
        public Currency Currency { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "Документ")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaNach { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "Документ")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaOut { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "Документ")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal SummaIn { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal NachCNY { set; get; }

        [Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal InCNY { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal OutCNY { set; get; }

        [Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        private decimal ResultCNY { set; get; }
    }
}