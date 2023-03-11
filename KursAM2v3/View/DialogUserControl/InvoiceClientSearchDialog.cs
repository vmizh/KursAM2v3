using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursDomain.Repository;
using JetBrains.Annotations;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.InvoicesRepositories;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl
{
    public sealed class InvoiceClientSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool isAccepted;
        private readonly bool isPaymentUse;
        private readonly decimal kontragentDC;
        private readonly Waybill waybill;
        private InvoiceClientViewModel myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;
        private Currency currency;

        public InvoiceClientSearchDialog(bool isPaymentUse, bool isUseAcepted, Currency crs = null)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            this.isPaymentUse = isPaymentUse;
            isAccepted = isUseAcepted;
            currency = crs;
            RefreshData(null);
        }

        public InvoiceClientSearchDialog(decimal kontragentDC, bool isPaymentUse, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            this.kontragentDC = kontragentDC;
            this.isPaymentUse = isPaymentUse;
            isAccepted = isUseAcepted;
            waybill = null;
            RefreshData(null);
        }

        public InvoiceClientSearchDialog([NotNull] Waybill waybill)
        {
            this.waybill = waybill;
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            RefreshData(waybill);
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceClient> ItemsCollection { set; get; } =
            new ObservableCollection<IInvoiceClient>();

        public InvoiceClientViewModel CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem,value)) return;
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

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);
        public DependencyObject LayoutControl { get; }

        public override void RefreshData(object obj)
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            base.RefreshData(obj);

            SearchClear(null);
            Search(null);
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public override void ResetLayout(object form)
        {
            myDataUserControl?.LayoutManager.ResetLayout();
        }

        #region Commands

        public override void Search(object obj)
        {
            try
            {
                ItemsCollection.Clear();
                if (waybill == null)
                {
                    if (kontragentDC > 0)
                        foreach (var d in InvoicesManager.GetInvoicesClient(DateTime.Today.AddDays(-300),
                                     DateTime.Today,
                                     isPaymentUse, kontragentDC, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                    else
                        foreach (var d in InvoicesManager.GetInvoicesClient(DateTime.Today.AddDays(-300),
                                     DateTime.Today,
                                     isPaymentUse, null, SearchText, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                }
                else
                {
                    foreach (var d in InvoicesManager.GetInvoicesClient(waybill))
                    {
                        if (currency == null)
                        {
                            ItemsCollection.Add(d);
                        }
                        else
                        {
                            if (d.Currency.DocCode == currency.DocCode)
                            {
                                ItemsCollection.Add(d);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SearchClear(object obj)
        {
            ItemsCollection.Clear();
            CurrentItem = null;
        }

        #endregion
    }


    public sealed class InvoiceProviderSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceProvider> ItemsCollection { set; get; } =
            new ObservableCollection<IInvoiceProvider>();

        public InvoiceProvider CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem,value)) return;
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
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            base.RefreshData(obj);
            SearchClear(null);
            Search(null);
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public override void ResetLayout(object form)
        {
            myDataUserControl?.LayoutManager.ResetLayout();
        }

        #region Fields

        private readonly bool isAccepted;
        private readonly bool isOnlyLastYear;

        // ReSharper disable once NotAccessedField.Local
        private readonly bool isPaymentUse;
        private readonly decimal kontragentDC;
        private InvoiceProvider myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;
        private Currency currency;


#pragma warning disable 169
        private readonly GenericKursDBRepository<InvoiceProvider> baseRepository;
#pragma warning restore 169

        // ReSharper disable once NotAccessedField.Local
#pragma warning disable 169
        private readonly IInvoiceProviderRepository invoiceProviderRepository;
#pragma warning restore 169

        #endregion

        #region Constructors

        public InvoiceProviderSearchDialog(bool isUsePayment, bool isUseAccepted, 
            bool isOnlyLastYear = false, Currency crs = null)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            isPaymentUse = isUsePayment;
            isAccepted = isUseAccepted;
            this.isOnlyLastYear = isOnlyLastYear;
            currency = crs;
            RefreshData(null);
        }

        public InvoiceProviderSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAccepted,
            bool isOnlyLastYear = false)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            kontragentDC = kontrDC;
            WindowName = "Выбор счета";
            isPaymentUse = isUsePayment;
            isAccepted = isUseAccepted;
            this.isOnlyLastYear = isOnlyLastYear;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);

        public override void Search(object obj)
        {
            try
            {
                ItemsCollection.Clear();
                if (kontragentDC > 0)
                {
                    if (!isOnlyLastYear)
                        foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                                     true,
                                     kontragentDC, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                    else
                        foreach (var d in InvoicesManager.GetInvoicesProvider(DateTime.Today.AddDays(-365),
                                     DateTime.Today,
                                     true,
                                     kontragentDC, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                }

                else
                {
                    if (!isOnlyLastYear)
                        foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                                     true,
                                     SearchText, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                    else
                        foreach (var d in InvoicesManager.GetInvoicesProvider(DateTime.Today.AddDays(-365),
                                     DateTime.Today,
                                     true,
                                     SearchText, isAccepted))
                        {
                            if (currency == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (d.Currency.DocCode == currency.DocCode)
                                {
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SearchClear(object obj)
        {
            ItemsCollection.Clear();
            CurrentItem = null;
        }

        #endregion
    }

    public sealed class InvoiceAllSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool isAccepted;
        private readonly bool isUsePayment;
        private readonly decimal kontragentDC;
        private IInvoiceClient myCurrentClientItem;
        private IInvoiceProvider myCurrentProviderItem;
        private AllInvocesDialogSelectUC myDataUserControl;
        private Currency myCurrency;

        public InvoiceAllSearchDialog(bool isUsePayment, bool isUseAcepted, Currency crs = null)
        {
            LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            this.isUsePayment = isUsePayment;
            isAccepted = isUseAcepted;
            this.myCurrency = crs;
            RefreshData(null);
            
        }

        public InvoiceAllSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            kontragentDC = kontrDC;
            this.isUsePayment = isUsePayment;
            isAccepted = isUseAcepted;
            RefreshData(null);
        }

        public AllInvocesDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceClient> ClientItemsCollection { set; get; } =
            new ObservableCollection<IInvoiceClient>();

        public IInvoiceClient CurrentClientItem
        {
            get => myCurrentClientItem;
            set
            {
                if (Equals(myCurrentClientItem,value)) return;
                myCurrentClientItem = value;
                if (myCurrentClientItem != null)
                    myCurrentProviderItem = null;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<IInvoiceProvider> ProviderItemsCollection { set; get; } =
            new ObservableCollection<IInvoiceProvider>();

        public IInvoiceProvider CurrentProviderItem
        {
            get => myCurrentProviderItem;
            set
            {
                if (Equals(myCurrentProviderItem,value)) return;
                myCurrentProviderItem = value;
                if (myCurrentProviderItem != null)
                    myCurrentClientItem = null;
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl { get; }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            SearchClear(null);
            Search(null);
        }

        public override void ResetLayout(object form)
        {
            myDataUserControl?.LayoutManager.ResetLayout();
        }

        #region Commands

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);

        public override void SearchClear(object obj)
        {
            ClientItemsCollection.Clear();
            ProviderItemsCollection.Clear();
            CurrentClientItem = null;
            CurrentProviderItem = null;
        }

        public override void Search(object obj)
        {
            try
            {
                ProviderItemsCollection.Clear();
                if (kontragentDC > 0)
                {
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                                 true,
                                 kontragentDC, isAccepted))
                        ProviderItemsCollection.Add(d);
                }
                else
                {
                    var providerRepository = new InvoiceProviderRepository(GlobalOptions.GetEntities());
                    foreach (var inv in providerRepository.GetAllByDates(new DateTime(2000, 1, 1), DateTime.Today)
                                 .Where(inv => !isUsePayment || inv.Summa != inv.PaySumma)
                                 .Where(inv => !isAccepted || inv.IsAccepted))
                    {
                        if (myCurrency == null)
                        {
                            ProviderItemsCollection.Add(inv);
                            continue;
                        }
                        if(inv.Currency == myCurrency)
                            ProviderItemsCollection.Add(inv);
                    }
                       
                }


                ClientItemsCollection.Clear();
                if (kontragentDC > 0)
                {
                    foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                                 isUsePayment, kontragentDC, isAccepted))
                        ClientItemsCollection.Add(d);
                }
                else
                {
                    var clientRepository = new InvoiceClientRepository(GlobalOptions.GetEntities());
                    foreach (var inv in clientRepository.GetAllByDates(new DateTime(2000, 1, 1), DateTime.Today)
                                 .Where(inv => !isUsePayment || inv.Summa != inv.PaySumma)
                                 .Where(inv => !isAccepted || inv.IsAccepted))
                        ClientItemsCollection.Add(inv);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #endregion Commands
    }
}
