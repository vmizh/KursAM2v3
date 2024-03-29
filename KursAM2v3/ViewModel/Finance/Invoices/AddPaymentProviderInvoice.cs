using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.ViewModel.Base;
using KursAM2.View.DialogUserControl;
using KursDomain;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class AddPaymentBankProviderInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentBankProviderInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentBankProviderInvoice");
            myKontrDC = kontrDC;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            //base.RefreshData(obj);
            ItemsCollection.Clear();
            TempCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.TD_101
                    .Include(_ => _.SD_101)
                    .Include(_ => _.SD_114)
                    .Include(_ => _.SD_114.SD_44)
                    .Where(_ => _.VVT_KONTRAGENT == myKontrDC
                                && _.BankAccountDC == null &&
                                _.VVT_SFACT_POSTAV_DC == null &&
                                _.VVT_SFACT_CLIENT_DC == null
                                && _.BankFromTransactionCode == null &&
                                _.IsCurrencyChange != true
                                && _.VVT_KASS_PRIH_ORDER_DC == null &&
                                _.VVT_RASH_KASS_ORDER_DC == null
                                && _.VVT_VAL_PRIHOD == 0).ToList();
                var sql = "SELECT code AS Code, t101.VVT_VAL_RASHOD AS Summa, " +
                          "SUM(pip.Summa) AS PaySumma " +
                          "FROM td_101 t101 " +
                          "INNER JOIN ProviderInvoicePay pip ON t101.CODE = pip.BankCode " +
                          "GROUP BY code,t101.VVT_VAL_RASHOD " +
                          "HAVING t101.VVT_VAL_RASHOD > SUM(pip.Summa) ";
                var data2 = ctx.Database.SqlQuery<BankTransList>(sql).ToList();
                var existCodes = ctx.ProviderInvoicePay.Where(_ => _.BankCode != null).Select(_ => _.BankCode).ToList();
                foreach (var d in data)
                {
                    if(existCodes.Contains(d.CODE)) continue;
                    ItemsCollection.Add(new BankPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC)).Name,
                        DocDate = d.SD_101.VV_START_DATE,
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = (decimal)d.VVT_VAL_RASHOD,
                        Note = d.VVT_DOC_NUM,
                        Name = d.SD_101.SD_114.SD_44.BANK_NAME,
                        AccountName = d.SD_101.SD_114.BA_RASH_ACC,
                        Remainder = (decimal)d.VVT_VAL_RASHOD
                    });
                }

                foreach (var d in data2)
                {
                    var item = ctx.TD_101
                        .Include(_ => _.SD_101)
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_114.SD_44)
                        .FirstOrDefault(_ => _.CODE == d.Code);
                    if (item == null) continue;
                    ItemsCollection.Add(new BankPaymentRow
                    {
                        DocCode = item.DOC_CODE,
                        Code = item.CODE,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(item.VVT_CRS_DC)).Name,
                        DocDate = item.SD_101.VV_START_DATE,
                        // ReSharper disable once PossibleInvalidOperationException
                        Summa = d.Summa,
                        Note = item.VVT_DOC_NUM,
                        Name = item.SD_101.SD_114.SD_44.BANK_NAME,
                        AccountName = item.SD_101.SD_114.BA_RASH_ACC,
                        AlreadyPay = d.PaySumma,
                        Remainder = d.Summa - d.PaySumma
                    });
                }
            }

            foreach (var item in TempCollection.OrderByDescending(_ => _.DocDate)) ItemsCollection.Add(item);

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Вспомогательные классы

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        // ReSharper disable once ClassNeverInstantiated.Global
        public class BankTransList
        {
            public int Code { set; get; }
            public decimal Summa { set; get; }
            public decimal PaySumma { set; get; }
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private BankPaymentRow myCurrentItem;
        private readonly decimal myKontrDC;

        #endregion

        #region Properties

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<BankPaymentRow> ItemsCollection { set; get; } =
            new ObservableCollection<BankPaymentRow>();

        public ObservableCollection<BankPaymentRow> SelectedItems { set; get; } =
            new ObservableCollection<BankPaymentRow>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<BankPaymentRow> TempCollection { set; get; } =
            new ObservableCollection<BankPaymentRow>();

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

    public sealed class AddPaymentCashProviderInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentCashProviderInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentCashProviderInvoice");
            myKontrDC = kontrDC;
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
                var data = ctx.SD_34.Where(_ => _.KONTRAGENT_DC == myKontrDC
                                                && (_.KONTR_FROM_DC ?? 0) == 0 && (_.SPOST_DC ?? 0) == 0)
                    .OrderByDescending(_ => _.DATE_ORD);

                foreach (var d in data)
                    ItemsCollection.Add(new CashPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Name = ((IName)GlobalOptions.ReferencesCache.GetCashBox(Convert.ToDecimal(d.CA_DC))).Name,
                        DocNum = d.NUM_ORD.ToString(),
                        DocDate = Convert.ToDateTime(d.DATE_ORD),
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(Convert.ToDecimal(d.CRS_DC)))
                            .Name,
                        Summa = Convert.ToDecimal(d.SUMM_ORD),
                        Note = d.NOTES_ORD
                    });
            }

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private CashPaymentRow myCurrentItem;
        private readonly decimal myKontrDC;

        #endregion

        #region Properties

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<CashPaymentRow> ItemsCollection { set; get; } =
            new ObservableCollection<CashPaymentRow>();

        public ObservableCollection<CashPaymentRow> SelectedItems { set; get; } =
            new ObservableCollection<CashPaymentRow>();

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

    public sealed class AddPaymentVZProviderInvoice : RSWindowViewModelBase
    {
        #region Constructors

        public AddPaymentVZProviderInvoice(decimal kontrDC)
        {
            myDataUserControl = new StandartDialogSelectUC("AddPaymentVZProviderInvoice");
            myKontrDC = kontrDC;
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
                    .Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1 && _.VZT_KONTR_DC == myKontrDC
                                                              && _.VZT_SFACT_DC == null &&
                                                              _.SD_110.SD_111.IsCurrencyConvert != true)
                    .OrderByDescending(_ => _.SD_110.VZ_DATE);
                foreach (var d in data)
                    ItemsCollection.Add(new VZPaymentRow
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        DocNum = d.SD_110.VZ_NUM + "/" + d.VZT_DOC_NUM,
                        DocDate = Convert.ToDateTime(d.SD_110.VZ_DATE),
                        CurrencyName =
                            ((IName)GlobalOptions.ReferencesCache.GetCurrency(Convert.ToDecimal(d.VZT_CRS_DC))).Name,
                        Summa = Convert.ToDecimal(d.VZT_CRS_SUMMA),
                        Note = d.SD_110.VZ_NOTES + " " + d.VZT_DOC_NOTES
                    });
            }

            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region Fields

        private StandartDialogSelectUC myDataUserControl;
        private VZPaymentRow myCurrentItem;
        private readonly decimal myKontrDC;

        #endregion

        #region Properties

        // ReSharper disable once CollectionNeverQueried.Global
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
}
