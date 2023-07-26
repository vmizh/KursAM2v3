using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.StockHolder;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.StockHolder;
using KursDomain.Menu;
using KursDomain.References;

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

        public override bool IsDocumentOpenAllow => CurrentMoneyDoc != null;

        public override void DocumentOpen(object obj)
        {
            switch (CurrentMoneyDoc.DocumentName)
            {
                case "Приходный кассовый ордер":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.CashIn, (decimal)CurrentMoneyDoc.DocCode);
                    break;
                case "Расходный кассовый ордер":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.CashOut, (decimal)CurrentMoneyDoc.DocCode);
                    break;
                case "Начисление акционеру":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.StockHolderAccrual, 0, (Guid)CurrentMoneyDoc.DocId);
                    break;
            }
        }

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

                var shIds = nachdata.Select(_ => _.StockHolderId).Distinct().ToList();
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
            if (StockHolders.Count > 0) CurrentStockHolder = StockHolders.First();
        }

        #endregion

        #region Fields

        private StockHolderBalanseItem myCurrentStockHolder;
        private readonly List<StockHolderViewModel> stockHoldersAll = new List<StockHolderViewModel>();
        private StockHolderBalanseItemPeriod myCurrentPeriod;
        private StockHolderMoneyMove myCurrentMoneyDoc;

        private List<StockHolderMoneyMove> StockHolderMoneyMoves { get; } =
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

        public StockHolderMoneyMove CurrentMoneyDoc
        {
            get => myCurrentMoneyDoc;
            set
            {
                if (myCurrentMoneyDoc == value) return;
                myCurrentMoneyDoc = value;
                RaisePropertyChanged();
            }
        }

        public StockHolderBalanseItemPeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (myCurrentPeriod == value) return;
                myCurrentPeriod = value;
                LoadDocumentsForPeriod();
                SetCrsVisibleStockHolderMoneyMove();
                RaisePropertyChanged();
            }
        }

        public StockHolderBalanseItem CurrentStockHolder
        {
            get => myCurrentStockHolder;
            set
            {
                if (myCurrentStockHolder == value) return;
                myCurrentStockHolder = value;
                LoadPeriods();
                SetCrsVisiblePeriod();
                CalcPeriodNakopit();
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void LoadDocumentsForPeriod()
        {
            if (CurrentPeriod == null) return;
            MoneyMoves.Clear();
            foreach (var m in StockHolderMoneyMoves
                         .Where(_ => _.DocumentDate >= CurrentPeriod.DateStart &&
                                     _.DocumentDate <= CurrentPeriod.DateEnd))
                MoneyMoves.Add(m);
        }

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
                         Name = d.Name,
                         PeriodType = d.PeriodType
                     }))
            {
                SetPeriodSum(newDate);
                Periods.Add(newDate);
            }
        }

        private void SetPeriodSum(StockHolderBalanseItemPeriod periodItem)
        {
            foreach (var m in StockHolderMoneyMoves.Where(_ =>
                         _.DocumentDate >= periodItem.DateStart && _.DocumentDate <= periodItem.DateEnd))
                switch (m.Currency.DocCode)
                {
                    case CurrencyCode.RUB:
                        // periodItem.InRUB += m.SummaIn;
                        periodItem.OutRUB += m.SummaOut;
                        periodItem.NachRUB += m.SummaNach;
                        break;
                    case CurrencyCode.USD:
                        // periodItem.InUSD += m.SummaIn;
                        periodItem.OutUSD += m.SummaOut;
                        periodItem.NachUSD += m.SummaNach;
                        break;
                    case CurrencyCode.EUR:
                        // periodItem.InEUR += m.SummaIn;
                        periodItem.OutEUR += m.SummaOut;
                        periodItem.NachEUR += m.SummaNach;
                        break;
                    case CurrencyCode.GBP:
                        // periodItem.InGBP += m.SummaIn;
                        periodItem.OutGBP += m.SummaOut;
                        periodItem.NachGBP += m.SummaNach;
                        break;
                    case CurrencyCode.CHF:
                        // periodItem.InCHF += m.SummaIn;
                        periodItem.OutCHF += m.SummaOut;
                        periodItem.NachCHF += m.SummaNach;
                        break;
                    case CurrencyCode.CNY:
                        // periodItem.InCNY += m.SummaIn;
                        periodItem.OutCNY += m.SummaOut;
                        periodItem.NachCNY += m.SummaNach;
                        break;
                    case CurrencyCode.SEK:
                        // periodItem.InSEK += m.SummaIn;
                        periodItem.OutSEK += m.SummaOut;
                        periodItem.NachSEK += m.SummaNach;
                        break;
                }
        }

        private void SetPeriodSum(StockHolderMoneyMove item)
        {
            switch (item.Currency.DocCode)
            {
                case CurrencyCode.RUB:
                    // periodItem.InRUB += m.SummaIn;
                    item.OutRUB = item.SummaOut;
                    item.NachRUB = item.SummaNach;
                    break;
                case CurrencyCode.USD:
                    // item.InUSD = m.SummaIn;
                    item.OutUSD = item.SummaOut;
                    item.NachUSD = item.SummaNach;
                    break;
                case CurrencyCode.EUR:
                    // item.InEUR = m.SummaIn;
                    item.OutEUR = item.SummaOut;
                    item.NachEUR = item.SummaNach;
                    break;
                case CurrencyCode.GBP:
                    // item.InGBP = m.SummaIn;
                    item.OutGBP = item.SummaOut;
                    item.NachGBP = item.SummaNach;
                    break;
                case CurrencyCode.CHF:
                    // item.InCHF = m.SummaIn;
                    item.OutCHF = item.SummaOut;
                    item.NachCHF = item.SummaNach;
                    break;
                case CurrencyCode.CNY:
                    // item.InCNY = m.SummaIn;
                    item.OutCNY = item.SummaOut;
                    item.NachCNY = item.SummaNach;
                    break;
                case CurrencyCode.SEK:
                    // item.InSEK = m.SummaIn;
                    item.OutSEK = item.SummaOut;
                    item.NachSEK = item.SummaNach;
                    break;
            }
        }

        private void LoadMoneyMove()
        {
            if (CurrentStockHolder == null) return;
            StockHolderMoneyMoves.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dataCashIn = ctx.SD_33.Where(_ => _.StockHolderId == CurrentStockHolder.StockHolder.Id);
                var dataCashOut = ctx.SD_34.Where(_ => _.StockHolderId == CurrentStockHolder.StockHolder.Id);
                var dataNach = ctx.StockHolderAccrualRows.Include(_ => _.StockHolderAccrual)
                    .Where(_ => _.StockHolderId == CurrentStockHolder.StockHolder.Id);
                //foreach (var d in dataCashIn)
                //{
                //    var newItem = new StockHolderMoneyMove
                //    {
                //        DocCode = d.DOC_CODE,
                //        DocumentDate = d.DATE_ORD ?? DateTime.Today,
                //        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                //        DocumentName = "Приходный кассовый ордер",
                //        DocumentNumber = d.NUM_ORD.ToString(),
                //        SummaIn = d.SUMM_ORD ?? 0
                //    };
                //    StockHolderMoneyMoves.Add(newItem);
                //}

                foreach (var d in dataCashOut)
                {
                    var newItem = new StockHolderMoneyMove
                    {
                        DocCode = d.DOC_CODE,
                        DocumentDate = d.DATE_ORD ?? DateTime.Today,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                        DocumentName = "Расходный кассовый ордер",
                        DocumentNumber = d.NUM_ORD.ToString(),
                        SummaOut = d.SUMM_ORD ?? 0
                    };
                    SetPeriodSum(newItem);
                    StockHolderMoneyMoves.Add(newItem);
                }

                foreach (var d in dataNach)
                {
                    var newItem = new StockHolderMoneyMove
                    {
                        DocId = d.DocId,
                        DocumentDate = d.StockHolderAccrual.Date,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC) as Currency,
                        DocumentName = "Начисление акционеру",
                        DocumentNumber = d.StockHolderAccrual.Num.ToString(),
                        SummaNach = d.Summa ?? 0
                    };
                    SetPeriodSum(newItem);
                    StockHolderMoneyMoves.Add(newItem);
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void CalcPeriodNakopit()
        {
            foreach (var p in Periods.OrderByDescending(_ => _.DateEnd))
            {
                var d = Periods.Where(_ => _.DateEnd <= p.DateEnd && _.PeriodType == PeriodType.Day);
                // ReSharper disable once PossibleMultipleEnumeration
                if (!d.Any()) continue;

                p.ResultRUB = d.Sum(_ => _.OutRUB) - d.Sum(_ => _.InRUB) - d.Sum(_ => _.NachRUB);
                p.ResultUSD = d.Sum(_ => _.OutUSD) - d.Sum(_ => _.InUSD) - d.Sum(_ => _.NachUSD);
                p.ResultEUR = d.Sum(_ => _.OutEUR) - d.Sum(_ => _.InEUR) - d.Sum(_ => _.NachEUR);
                p.ResultGBP = d.Sum(_ => _.OutGBP) - d.Sum(_ => _.InGBP) - d.Sum(_ => _.NachGBP);
                p.ResultCHF = d.Sum(_ => _.OutCHF) - d.Sum(_ => _.InCHF) - d.Sum(_ => _.NachCHF);
                p.ResultSEK = d.Sum(_ => _.OutSEK) - d.Sum(_ => _.InSEK) - d.Sum(_ => _.NachSEK);
                p.ResultCNY = d.Sum(_ => _.OutCNY) - d.Sum(_ => _.InCNY) - d.Sum(_ => _.NachCNY);
            }
        }

        private void SetCrsVisiblePeriod()
        {
            if (Form is StockHoldersBalancesView frm)
            {
                var bandUSD = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "USD");
                var bandRUB = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "RUB");
                var bandEUR = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "EUR");
                var bandSEK = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "SEK");
                var bandCNY = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "CNY");
                var bandCHF = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "CHF");
                var bandGBP = frm.treePeriods.Bands.FirstOrDefault(_ => (string)_.Header == "GBP");
                if (bandUSD != null)
                    bandUSD.Visible = !(StockHolderMoneyMoves.Sum(_ => _.NachUSD) == 0 ||
                                        StockHolderMoneyMoves.Sum(_ => _.OutUSD) == 0);
                if (bandRUB != null)
                    bandRUB.Visible = !(StockHolders.Sum(_ => _.NachRUB) == 0 ||
                                        StockHolders.Sum(_ => _.OutRUB) == 0);
                if (bandEUR != null)
                    bandEUR.Visible = !(StockHolders.Sum(_ => _.NachEUR) == 0 ||
                                        StockHolders.Sum(_ => _.OutEUR) == 0);
                if (bandSEK != null)
                    bandSEK.Visible = !(StockHolders.Sum(_ => _.NachSEK) == 0 ||
                                        StockHolders.Sum(_ => _.OutSEK) == 0);
                if (bandCNY != null)
                    bandCNY.Visible = !(StockHolders.Sum(_ => _.NachCNY) == 0 ||
                                        StockHolders.Sum(_ => _.OutCNY) == 0);
                if (bandCHF != null)
                    bandCHF.Visible = !(StockHolders.Sum(_ => _.NachCHF) == 0 ||
                                        StockHolders.Sum(_ => _.OutCHF) == 0);
                if (bandGBP != null)
                    bandGBP.Visible = !(StockHolders.Sum(_ => _.NachGBP) == 0 ||
                                        StockHolders.Sum(_ => _.OutGBP) == 0);
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

        private void SetCrsVisibleStockHolderMoneyMove()
        {
            if (Form is StockHoldersBalancesView frm)
            {
                var bandUSD = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "USD");
                var bandRUB = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "RUB");
                var bandEUR = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "EUR");
                var bandSEK = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "SEK");
                var bandCNY = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "CNY");
                var bandCHF = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "CHF");
                var bandGBP = frm.GridControlStockHolderMove.Bands.FirstOrDefault(_ => (string)_.Header == "GBP");
                if (bandUSD != null)
                    bandUSD.Visible = !(MoneyMoves.Sum(_ => _.NachUSD) == 0 &&
                                        MoneyMoves.Sum(_ => _.OutUSD) == 0);
                if (bandRUB != null)
                    bandRUB.Visible = !(MoneyMoves.Sum(_ => _.NachRUB) == 0 &&
                                        MoneyMoves.Sum(_ => _.OutRUB) == 0);
                if (bandEUR != null)
                    bandEUR.Visible = !(MoneyMoves.Sum(_ => _.NachEUR) == 0 &&
                                        MoneyMoves.Sum(_ => _.OutEUR) == 0);
                if (bandSEK != null)
                    bandSEK.Visible = !(MoneyMoves.Sum(_ => _.NachSEK) == 0
                                        && MoneyMoves.Sum(_ => _.OutSEK) == 0);
                if (bandCNY != null)
                    bandCNY.Visible = !(MoneyMoves.Sum(_ => _.NachCNY) == 0
                                        && MoneyMoves.Sum(_ => _.OutCNY) == 0);
                if (bandCHF != null)
                    bandCHF.Visible = !(MoneyMoves.Sum(_ => _.NachCHF) == 0
                                        && MoneyMoves.Sum(_ => _.OutCHF) == 0);
                if (bandGBP != null)
                    bandGBP.Visible = !(MoneyMoves.Sum(_ => _.NachGBP) == 0
                                        && MoneyMoves.Sum(_ => _.OutGBP) == 0);
            }
        }

        #endregion

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is StockHoldersBalancesView frm)
            {
                frm.TableViewStockHolderMove.ShowTotalSummary = true;
            }
        }
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
        public Guid? DocId { set; get; }


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

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "Документ")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal SummaIn { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachRUB { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "RUB")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "RUB")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutRUB { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "RUB")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultRUB { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachUSD { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "USD")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "USD")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutUSD { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "USD")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultUSD { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachEUR { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "EUR")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "EUR")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutEUR { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "EUR")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultEUR { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachGBP { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "GBP")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "GBP")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutGBP { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "GBP")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultGBP { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachCHF { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CHF")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CHF")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutCHF { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "CHF")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultCHF { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachSEK { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "SEK")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "SEK")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutSEK { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "SEK")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultSEK { set; get; }

        [Display(AutoGenerateField = true, Name = "Начислено", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal NachCNY { set; get; }

        //[Display(AutoGenerateField = true, Name = "Поступило", GroupName = "CNY")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal InCNY { set; get; }

        [Display(AutoGenerateField = true, Name = "Выплачено", GroupName = "CNY")]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal OutCNY { set; get; }

        //[Display(AutoGenerateField = true, Name = "Накопительно", GroupName = "CNY")]
        //[Editable(false)]
        //[DisplayFormat(DataFormatString = "n2")]
        //public decimal ResultCNY { set; get; }
    }
}
