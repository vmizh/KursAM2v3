using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.View.DialogUserControl;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class AddPaymentBankClientInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentBankClientInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentBankClientInvoice");
            this.kontrDC = kontrDC;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            //base.RefreshData(obj);
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.TD_101
                    .Include(_ => _.SD_101)
                    .Include(_ => _.SD_114)
                    .Include(_ => _.SD_114.SD_44)
                    .Where(_ => _.VVT_KONTRAGENT == kontrDC
                                && _.BankAccountDC == null &&
                                _.VVT_SFACT_POSTAV_DC == null &&
                                _.VVT_SFACT_CLIENT_DC == null
                                && _.BankFromTransactionCode == null &&
                                _.IsCurrencyChange != true
                                && _.VVT_KASS_PRIH_ORDER_DC == null &&
                                _.VVT_RASH_KASS_ORDER_DC == null
                                && (_.VVT_VAL_RASHOD ?? 0) == 0)
                    .OrderByDescending(_ => _.SD_101.VV_START_DATE).ToList();
                foreach (var d in data.Where(_ => _.BankAccountDC == null &&
                                                  _.VVT_SFACT_POSTAV_DC == null &&
                                                  _.VVT_SFACT_CLIENT_DC == null
                                                  && _.BankFromTransactionCode == null &&
                                                  _.IsCurrencyChange != true
                                                  && _.VVT_KASS_PRIH_ORDER_DC == null &&
                                                  _.VVT_RASH_KASS_ORDER_DC == null))
                    ItemsCollection.Add(new BankPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        CurrencyName = MainReferences.Currencies[d.VVT_CRS_DC].Name,
                        DocDate = d.SD_101.VV_START_DATE,
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal) d.VVT_VAL_PRIHOD,
                        Note = d.VVT_DOC_NUM,
                        Name = d.SD_101.SD_114.SD_44.BANK_NAME,
                        AccountName = d.SD_101.SD_114.BA_RASH_ACC
                    });
            }

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private BankPaymentRow myCurrentItem;
        private readonly decimal kontrDC;

        #endregion

        #region Properties

        public ObservableCollection<BankPaymentRow> ItemsCollection { set; get; } = new ObservableCollection<BankPaymentRow>();
        public ObservableCollection<BankPaymentRow> SelectedItems { set; get; } = new ObservableCollection<BankPaymentRow>();

        public StandartDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public BankPaymentRow CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        #endregion
    }

    public sealed class AddPaymentCashClientInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentCashClientInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentCashClientInvoice");
            this.kontrDC = kontrDC;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            //base.RefreshData(obj);
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_33.Where(_ => _.KONTRAGENT_DC == kontrDC
                                                && (_.KONTR_FROM_DC ?? 0) == 0 && (_.SFACT_DC ?? 0) == 0);
                foreach (var d in data)
                {
                    ItemsCollection.Add(new CashPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Name = MainReferences.Cashs[Convert.ToDecimal(d.CA_DC)].Name,
                        DocNum = d.NUM_ORD.ToString(),
                        DocDate = Convert.ToDateTime(d.DATE_ORD),
                        CurrencyName = MainReferences.Currencies[Convert.ToDecimal(d.CRS_DC)].Name,
                        Summa = Convert.ToDecimal(d.SUMM_ORD),
                        Note = d.NOTES_ORD,
                    });
                }
            }

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private CashPaymentRow myCurrentItem;
        private readonly decimal kontrDC;

        #endregion

        #region Properties

        public ObservableCollection<CashPaymentRow> ItemsCollection { set; get; } = new ObservableCollection<CashPaymentRow>();
        public ObservableCollection<CashPaymentRow> SelectedItems { set; get; } = new ObservableCollection<CashPaymentRow>();

        public StandartDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public CashPaymentRow CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        #endregion
    }

    public sealed class AddPaymentVZClientInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentVZClientInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentVZClientInvoice");
            this.kontrDC = kontrDC;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            //base.RefreshData(obj);
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ItemsCollection.Clear();
                var data = ctx.TD_110
                    .Include(_ => _.SD_110)
                    .Include(_ => _.SD_110.SD_111)
                    .Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0 && _.VZT_KONTR_DC == kontrDC
                                                              && _.VZT_SFACT_DC == null &&
                                                              _.SD_110.SD_111.IsCurrencyConvert != true)
                    .OrderByDescending(_ => _.SD_110.VZ_DATE);
                foreach (var d in data)
                {
                    ItemsCollection.Add(new VZPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        DocNum = d.SD_110.VZ_NUM.ToString() + "/" + d.VZT_DOC_NUM,
                        DocDate = Convert.ToDateTime(d.SD_110.VZ_DATE),
                        CurrencyName = MainReferences.Currencies[Convert.ToDecimal(d.VZT_CRS_DC)].Name,
                        Summa = Convert.ToDecimal(d.VZT_CRS_SUMMA),
                        Note = d.SD_110.VZ_NOTES + " " + d.VZT_DOC_NOTES,
                    });
                }
            }

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private VZPaymentRow myCurrentItem;
        private readonly decimal kontrDC;

        #endregion

        #region Properties

        public ObservableCollection<VZPaymentRow> ItemsCollection { set; get; } 
            = new ObservableCollection<VZPaymentRow>();
        public ObservableCollection<VZPaymentRow> SelectedItems { set; get; } 
            = new ObservableCollection<VZPaymentRow>();

        public StandartDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public VZPaymentRow CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        #endregion
    }

    #region Auxiliary classes

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class PaymentRow
    {
        public decimal DocCode { set; get; }
        public int Code { set; get; }
        public string Name { set; get; }
        public string CurrencyName { set; get; }
        public decimal Summa { set; get; }
        public string DocNum { set; get; }
        public DateTime DocDate { set; get; }
        public string Note { set; get; }
    }

    [MetadataType(typeof(DataAnnotationsBankPaymentRow))]
    public class BankPaymentRow : PaymentRow
    {
        public string AccountName { set; get; }
    }

    [MetadataType(typeof(DataAnnotationsCashPaymentRow))]
    public class CashPaymentRow : PaymentRow
    {
    }

    [MetadataType(typeof(DataAnnotationsVZPaymentRow))]
    public class VZPaymentRow : PaymentRow
    {
    }

    public class DataAnnotationsBankPaymentRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<BankPaymentRow>
    {
        void IMetadataProvider<BankPaymentRow>.BuildMetadata(MetadataBuilder<BankPaymentRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта").ReadOnly();
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Банк").ReadOnly();
            builder.Property(_ => _.AccountName).AutoGenerated().DisplayName("Счет").ReadOnly();
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания").ReadOnly();
        }
    }

    public class DataAnnotationsCashPaymentRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<CashPaymentRow>
    {
        void IMetadataProvider<CashPaymentRow>.BuildMetadata(MetadataBuilder<CashPaymentRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта").ReadOnly();
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Касса").ReadOnly();
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания").ReadOnly();
        }
    }

    public class DataAnnotationsVZPaymentRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<VZPaymentRow>
    {
        void IMetadataProvider<VZPaymentRow>.BuildMetadata(MetadataBuilder<VZPaymentRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта").ReadOnly();
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания").ReadOnly();
        }
    }

    #endregion
}