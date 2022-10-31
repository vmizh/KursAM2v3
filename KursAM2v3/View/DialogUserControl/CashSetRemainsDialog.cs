using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursDomain.Documents.Cash;

namespace KursAM2.View.DialogUserControl
{
    public class CashSetRemainsDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly Cash Cash;
        private readonly List<Currency> CashCurrencies = new List<Currency>();
        private CashStartRemains myCurrentRemain;
        private CashSetRemainsUC myDataUserControl;

        public CashSetRemainsDialog(Cash cash)
        {
            Cash = cash;
            LayoutControl = myDataUserControl = new CashSetRemainsUC();
            RefreshData(null);
            WindowName = "Ввод остатков";
        }

        public ObservableCollection<CashStartRemains> CashRemainsCollection { set; get; } =
            new ObservableCollection<CashStartRemains>();

        public ObservableCollection<Currency> CurrencyList { set; get; } =
            new ObservableCollection<Currency>(MainReferences.Currencies.Values);

        public CashStartRemains CurrentRemain
        {
            get => myCurrentRemain;
            set
            {
                if (myCurrentRemain != null && myCurrentRemain.Equals(value)) return;
                myCurrentRemain = value;
                RaisePropertyChanged();
            }
        }

        public CashSetRemainsUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl { get; }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            CashRemainsCollection.Clear();
            CashCurrencies.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var d in ctx.TD_22
                    .Include(_ => _.SD_22)
                    .Where(_ => _.DOC_CODE == Cash.DocCode).AsNoTracking().ToList())
                {
                    CashCurrencies.Add(MainReferences.Currencies[d.CRS_DC]);
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var item = new CashStartRemains(d);
                    CashRemainsCollection.Add(item);
                }
                foreach (var d in CashRemainsCollection)
                {
                    d.CurrencyList = new ObservableCollection<Currency>();
                    foreach (var crs in MainReferences.Currencies.Values)
                        if (!CashCurrencies.Contains(crs))
                            d.CurrencyList.Add(crs);
                    d.CurrencyList.Add(d.Currency);
                }
            }
        }

        public override void ResetLayout(object form)
        {
            myDataUserControl?.LayoutManager.ResetLayout();
        }

        #region Commands

        public ICommand AddNewCrsCommand
        {
            get { return new Command(AddNewCrs, _ => true); }
        }

        private void AddNewCrs(object obj)
        {
            var newItem = new CashStartRemains
            {
                DATE_START = DateTime.Today,
                SUMMA_START = 0,
                DOC_CODE = Cash.DocCode,
                State = RowStatus.NewRow,
                CurrencyList = new ObservableCollection<Currency>()
            };
            //foreach (var d in CashRemainsCollection)
            foreach (var crs in MainReferences.Currencies.Values)
                if (!CashCurrencies.Contains(crs))
                    newItem.CurrencyList.Add(crs);
            CashRemainsCollection.Add(newItem);
        }

        public ICommand RemoveCrsCommand
        {
            get { return new Command(RemoveCrs, _ => CurrentRemain != null); }
        }

        private void RemoveCrs(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (ctx.SD_33.Any(_ => _.CA_DC == Cash.DocCode && _.CRS_DC == CurrentRemain.CRS_DC))
                {
                    ShowMsg();
                    return;
                }
                if (ctx.SD_34.Any(_ => _.CA_DC == Cash.DocCode && _.CRS_DC == CurrentRemain.CRS_DC))
                {
                    ShowMsg();
                    return;
                }
                if (ctx.SD_251.Any(_ => _.CH_CASH_DC == Cash.DocCode && _.CH_CRS_IN_DC == CurrentRemain.CRS_DC))
                {
                    ShowMsg();
                    return;
                }
                if (ctx.SD_251.Any(_ => _.CH_CASH_DC == Cash.DocCode && _.CH_CRS_OUT_DC == CurrentRemain.CRS_DC))
                {
                    ShowMsg();
                    return;
                }
                var old = ctx.TD_22.FirstOrDefault(_ => _.DOC_CODE == Cash.DocCode && _.CRS_DC == CurrentRemain.CRS_DC);
                if (old != null)
                {
                    ctx.TD_22.Remove(old);
                    ctx.SaveChanges();
                    CashRemainsCollection.Remove(CurrentRemain);
                }
            }
        }

        private void ShowMsg()
        {
            WindowManager.ShowMessage("По валюте существуют операции. Удалять не возможно.", "Предупреждение",
                MessageBoxImage.Warning);
        }

        #endregion
    }
}
