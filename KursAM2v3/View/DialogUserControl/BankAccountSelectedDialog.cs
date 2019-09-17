using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using DevExpress.Mvvm.DataAnnotations;

namespace KursAM2.View.DialogUserControl
{
    public class BankAccountSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private BankAccount myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public BankAccountSelectedDialog()
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
            ItemsCollection = new ObservableCollection<BankAccount>(MainReferences.BankAccounts.Values);
        }

        public BankAccountSelectedDialog(decimal dcOut)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор банковского счета";
            ItemsCollection = new ObservableCollection<BankAccount>();
            foreach (var b in MainReferences.BankAccounts.Values.Where(_ => _.BankDC != dcOut)) ItemsCollection.Add(b);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<BankAccount> ItemsCollection { set; get; }
        public BankAccount CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem != null && myCurrentItem.Equals(value)) return;
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

    public class BankAccountOperationSelectedDialog : RSWindowViewModelBase, IDataUserControl
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
                if (myCurrentItem != null && myCurrentItem.Equals(value)) return;
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
            var dc = (decimal) obj;
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
                            Summa = (decimal) r.VVT_VAL_RASHOD,
                            Note = r.VVT_DOC_NUM,
                            Bank = MainReferences.BankAccounts[r.SD_101.VV_ACC_DC],
                            Date = r.SD_101.VV_STOP_DATE,
                            Currency = MainReferences.Currencies[r.VVT_CRS_DC],
                            SHPZ = r.VVT_SHPZ_DC != null ? MainReferences.SDRSchets[r.VVT_SHPZ_DC.Value] : null
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
            builder.Property(_ => _.Bank).AutoGenerated().ReadOnly().DisplayName("Банк отправитель");
            builder.Property(_ => _.Summa).AutoGenerated().ReadOnly().DisplayName("Сумма");
            builder.Property(_ => _.Date).AutoGenerated().ReadOnly().DisplayName("Дата");
            builder.Property(_ => _.Note).AutoGenerated().ReadOnly().DisplayName("Примечания");
            builder.Property(_ => _.Currency).AutoGenerated().ReadOnly().DisplayName("Валюта");
            builder.Property(_ => _.SHPZ).AutoGenerated().ReadOnly().DisplayName("Счет дох/расх");
        }
    }
}