using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.StockHolder;
using Core.Menu;
using Core.ViewModel.Base;


namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHoldersBalancesWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private StockHolderBalanseItem myCurrentStockHolder;
        private readonly List<StockHolderViewModel> stockHoldersAll = new List<StockHolderViewModel>();
        private StockHolderBalanseItem myCurrentPeriod;

        #endregion

        #region Constructors

        public StockHoldersBalancesWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
            RefreshData(null);
        }


        #endregion 
        #region Properties

        public override string WindowName => "Лицевые счета акционеров";
        public override string LayoutName => "StockHoldersBalancesWindowViewModel";

        public ObservableCollection<StockHolderBalanseItem> StockHolders { set; get; } =
            new ObservableCollection<StockHolderBalanseItem>();

        public ObservableCollection<StockHolderBalanseItemPeriod> Periods { set; get; }
            = new ObservableCollection<StockHolderBalanseItemPeriod>();

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
                myCurrentStockHolder = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            stockHoldersAll.Clear();
            StockHolders.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var sh in ctx.StockHolders.ToList())
                {
                    stockHoldersAll.Add(new StockHolderViewModel(sh));
                }

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
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CurrencyDC, s.Summa ?? 0,0,0);
                    }
                    foreach (var s in indata.Where(_ => _.StockHolderId == id))
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CRS_DC, 0,s.SUMM_ORD ?? 0,0);
                    }

                    foreach (var s in outdata.Where(_ => _.StockHolderId == id))
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        SetSumm(newItem, (decimal)s.CRS_DC, 0, 0,s.SUMM_ORD ?? 0);
                    }
                    StockHolders.Add(newItem);
                }

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

                case CurrencyCode.SEK:
                    item.NachSEK += sNach ?? 0;
                    item.InSEK += sIn ?? 0;
                    item.OutSEK += sOut ?? 0;
                    break;
            }
        }

        private void SetCrsVisibleStockHolder()
        {

        }

        #endregion

        #region IDataErrorInfo

        #endregion
    }
}