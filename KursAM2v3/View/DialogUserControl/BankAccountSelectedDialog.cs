using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.Dialogs;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl
{
    public sealed class BankAccountSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly decimal? excludeAccountDC;
        private readonly Currency currency;
        private BankAccount myCurrentChildItem;
        private BankAccount myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public BankAccountSelectedDialog(Currency crs)
        {
            currency = crs;
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
            ItemsCollection = new ObservableCollection<BankAccount>();
            if (currency == null)
            {
                foreach (var b in GlobalOptions.ReferencesCache.GetBankAccountAll())
                    if (ItemsCollection.All(_ => _.DocCode != ((IDocCode)b).DocCode))
                        ItemsCollection.Add(b as BankAccount);
            }
            else
            {
                foreach (var b in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>()
                             .Where(_ => Equals(_.Currency, crs)))
                    if (ItemsCollection.All(_ => _.DocCode != ((IDocCode)b).DocCode))
                        ItemsCollection.Add(b);
            }
        }

        public BankAccountSelectedDialog(decimal dcOut, bool isMustSurrency)
        {
            excludeAccountDC = dcOut;

            if (isMustSurrency)
            {
                currency = GlobalOptions.ReferencesCache.GetBankAccount(dcOut).Currency as Currency;
            }

            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
            ItemsCollection = new ObservableCollection<BankAccount>();
            foreach (var b in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>()
                         .Where(_ => _.DocCode != dcOut))
                if (ItemsCollection.All(_ => _.DocCode != b.DocCode))
                {
                    ItemsCollection.Add(b);
                }

            CurrentItem = null;
        }


        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<BankAccount> ChildItemsCollection { set; get; }
            = new ObservableCollection<BankAccount>();

        // ReSharper disable once MemberInitializerValueIgnored
        public ObservableCollection<BankAccount> ItemsCollection { set; get; } = new ObservableCollection<BankAccount>();

        public BankAccount CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem, value)) return;
                myCurrentItem = value;
                //ChildItemsCollection.Clear();
                //if (myCurrentItem == null) return;
                //if (excludeAccountDC != null)
                //{
                //    foreach (var acc in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>().Where(_ =>
                //                 ((IDocCode)_.Bank).DocCode == myCurrentItem?.DocCode
                //                 && ((IDocCode)_.Bank).DocCode != excludeAccountDC))
                //        if(currency == null)
                //            ChildItemsCollection.Add(acc);
                //        else
                //        {
                //            if (acc.Currency != null && ((IDocCode)acc.Currency).DocCode == currency?.DocCode)
                //            {
                //                ChildItemsCollection.Add(acc);
                //            }
                //        }
                //}
                //else
                //{
                //    if (currency == null)
                //        foreach (var acc in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>().Where(
                //                     _ =>
                //                         ((IDocCode)_.Bank).DocCode == myCurrentItem.DocCode))
                //            ChildItemsCollection.Add(acc);
                //    else
                //        foreach (var acc in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>().Where(
                //                     _ =>
                //                         ((IDocCode)_.Bank).DocCode == myCurrentItem.DocCode))
                //            if(((IDocCode)acc.Currency).DocCode == currency.DocCode)
                //                ChildItemsCollection.Add(acc);
                //}

                RaisePropertyChanged();
            }
        }

        public BankAccount CurrentChildItem
        {
            get => myCurrentChildItem;
            set
            {
                if (Equals(myCurrentChildItem, value)) return;
                myCurrentChildItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogSelectUC DataUserControl
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

        public override bool IsOkAllow()
        {
            return CurrentChildItem != null;
        }
    }

    public sealed class BankAccountSelectedDialog2 : RSWindowViewModelBase, IDataUserControl
    {
        private StandartDialogs.BankAccountSelect myCurrentChildItem;
        private StandartDialogs.BankSelect myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public BankAccountSelectedDialog2()
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
            ItemsCollection = new ObservableCollection<StandartDialogs.BankSelect>();
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<StandartDialogs.BankSelect> ItemsCollection { set; get; }
        public ObservableCollection<StandartDialogs.BankAccountSelect> ChildItemsCollection => CurrentItem?.AccountList;

        public StandartDialogs.BankSelect CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem, value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogs.BankAccountSelect CurrentChildItem
        {
            get => myCurrentChildItem;
            set
            {
                if (Equals(myCurrentChildItem, value)) return;
                myCurrentChildItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogSelectUC DataUserControl
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

        public override bool IsOkAllow()
        {
            return CurrentChildItem != null;
        }
    }

    public sealed class BankAccountOperationSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private BankOperationForSelectDialog myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public BankAccountOperationSelectedDialog()
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
        }

        public BankAccountOperationSelectedDialog(decimal dcOut)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор операции перевода из банка";
            RefreshData(dcOut);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<BankOperationForSelectDialog> ItemsCollection { set; get; } =
            new ObservableCollection<BankOperationForSelectDialog>();

        public BankOperationForSelectDialog CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem, value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogSelectUC DataUserControl
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
            if (obj == null) return;
            base.RefreshData(obj);
            var dc = (decimal)obj;
            using (var dtx = GlobalOptions.GetEntities())
            {
                var data = dtx.TD_101.Include(_ => _.SD_101).Where(_ => _.BankAccountDC == dc).ToList();
                foreach (var r in data)
                {
                    var s = dtx.TD_101.All(_ => r.CODE != _.BankFromTransactionCode);
                    if (s)
                        ItemsCollection.Add(new BankOperationForSelectDialog
                        {
                            DocCode = r.DOC_CODE,
                            Code = r.CODE,
                            Summa = (decimal)r.VVT_VAL_RASHOD,
                            Note = r.VVT_DOC_NUM,
                            Bank = GlobalOptions.ReferencesCache.GetBankAccount(r.SD_101.VV_ACC_DC) as BankAccount,
                            Date = r.SD_101.VV_STOP_DATE,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(r.VVT_CRS_DC) as Currency,
                            SHPZ = r.VVT_SHPZ_DC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(r.VVT_SHPZ_DC.Value) as SDRSchet
                                : null
                        });
                }
            }
        }
    }

    [MetadataType(typeof(DataAnnotationsBankOperationForSelectDialog))]
    public class BankOperationForSelectDialog
    {
        public decimal DocCode { set; get; }
        public int Code { set; get; }
        public decimal Summa { set; get; }
        public string Note { set; get; }
        public BankAccount Bank { set; get; }
        public DateTime Date { set; get; }
        public Currency Currency { set; get; }
        public SDRSchet SHPZ { set; get; }
    }

    public class DataAnnotationsBankOperationForSelectDialog : DataAnnotationForFluentApiBase,
        IMetadataProvider<BankOperationForSelectDialog>
    {
        void IMetadataProvider<BankOperationForSelectDialog>.BuildMetadata(
            MetadataBuilder<BankOperationForSelectDialog> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Code).NotAutoGenerated();
            builder.Property(_ => _.Bank).AutoGenerated().ReadOnly().DisplayName("Банк отправитель").LocatedAt(1);
            builder.Property(_ => _.Summa).AutoGenerated().ReadOnly().DisplayName("Сумма").LocatedAt(2);
            builder.Property(_ => _.Date).AutoGenerated().ReadOnly().DisplayName("Дата").LocatedAt(0);
            builder.Property(_ => _.Note).AutoGenerated().ReadOnly().DisplayName("Примечания").LocatedAt(4);
            builder.Property(_ => _.Currency).AutoGenerated().ReadOnly().DisplayName("Валюта").LocatedAt(3);
            builder.Property(_ => _.SHPZ).AutoGenerated().ReadOnly().DisplayName("Счет дох/расх").LocatedAt(5);
        }
    }

    public class BankSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private Bank myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public BankSelectedDialog()
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банка";
            ItemsCollection = new ObservableCollection<Bank>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_44.ToList();
                foreach (var d in data)
                {
                    var bank = new Bank();
                    bank.LoadFromEntity(d);
                    ItemsCollection.Add(bank);
                }
            }
        }

        public BankSelectedDialog(decimal dcOut)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банка";
            ItemsCollection = new ObservableCollection<Bank>();
            //foreach (var b in MainReferences.BankAccounts.Values.Where(_ => _.BankDC != dcOut)) ItemsCollection.Add(b);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<Bank> ItemsCollection { set; get; }

        public Bank CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem, value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogSelectUC DataUserControl
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
    }
}
