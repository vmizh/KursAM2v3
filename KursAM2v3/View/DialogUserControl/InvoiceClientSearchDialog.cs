using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using JetBrains.Annotations;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.InvoicesRepositories;

namespace KursAM2.View.DialogUserControl
{
    public sealed class InvoiceClientSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool isAccepted;
        private readonly bool isPaymentUse;
        private readonly decimal kontragentDC;
        private readonly Waybill waybill;
        private InvoiceClient myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public InvoiceClientSearchDialog(bool isPaymentUse, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            this.isPaymentUse = isPaymentUse;
            isAccepted = isUseAcepted;
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

        public ObservableCollection<InvoiceClient> ItemsCollection { set; get; } =
            new ObservableCollection<InvoiceClient>();

        public InvoiceClient CurrentItem
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

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);
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

        public override void Search(object obj)
        {
            try
            {
                ItemsCollection.Clear();
                if (waybill == null)
                {
                    if (kontragentDC > 0)
                        foreach (var d in InvoicesManager.GetInvoicesClient(DateTime.Today.AddDays(-300), DateTime.Today,
                            isPaymentUse, kontragentDC, isAccepted))
                            ItemsCollection.Add(d);
                    else
                        foreach (var d in InvoicesManager.GetInvoicesClient(DateTime.Today.AddDays(-300), DateTime.Today,
                            isPaymentUse, SearchText, isAccepted))
                            ItemsCollection.Add(d);
                }
                else
                {
                    foreach (var d in InvoicesManager.GetInvoicesClient(waybill))
                        ItemsCollection.Add(d);
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
        public ObservableCollection<InvoiceProvider> ItemsCollection { set; get; } =
            new ObservableCollection<InvoiceProvider>();

        public InvoiceProvider CurrentItem
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
            base.RefreshData(obj);
            SearchClear(null);
            Search(null);
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

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork = new UnitOfWork<ALFAMEDIAEntities>();
        // ReSharper disable once NotAccessedField.Local
#pragma warning disable 169
        private readonly GenericKursDBRepository<InvoiceProvider> baseRepository;
#pragma warning restore 169

        // ReSharper disable once NotAccessedField.Local
#pragma warning disable 169
        private readonly IInvoiceProviderRepository invoiceProviderRepository;
#pragma warning restore 169

        #endregion

        #region Constructors

        public InvoiceProviderSearchDialog(bool isUsePayment, bool isUseAccepted,bool isOnlyLastYear = false)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            isPaymentUse = isUsePayment;
            isAccepted = isUseAccepted;
            this.isOnlyLastYear = isOnlyLastYear;
            RefreshData(null);
        }

        public InvoiceProviderSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAccepted, bool isOnlyLastYear = false)
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
                    if(!isOnlyLastYear)
                        foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                            true,
                            kontragentDC, isAccepted))
                            ItemsCollection.Add(d);
                    else
                    {
                        foreach (var d in InvoicesManager.GetInvoicesProvider(DateTime.Today.AddDays(-365), DateTime.Today,
                            true,
                            kontragentDC, isAccepted))
                            ItemsCollection.Add(d);
                    }

                else
                {
                    if (!isOnlyLastYear)
                        foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                            true,
                            SearchText, isAccepted))
                            ItemsCollection.Add(d);
                    else
                    {
                        foreach (var d in InvoicesManager.GetInvoicesProvider(DateTime.Today.AddDays(-365), DateTime.Today,
                            true,
                            SearchText, isAccepted))
                            ItemsCollection.Add(d);
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
        private InvoiceClient myCurrentClientItem;
        private InvoiceProvider myCurrentProviderItem;
        private AllInvocesDialogSelectUC myDataUserControl;

        public InvoiceAllSearchDialog(bool isUsePayment, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            this.isUsePayment = isUsePayment;
            isAccepted = isUseAcepted;
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

        public ObservableCollection<InvoiceClient> ClientItemsCollection { set; get; } =
            new ObservableCollection<InvoiceClient>();

        public InvoiceClient CurrentClientItem
        {
            get => myCurrentClientItem;
            set
            {
                if (myCurrentClientItem != null && myCurrentClientItem.Equals(value)) return;
                myCurrentClientItem = value;
                if (myCurrentClientItem != null)
                    myCurrentProviderItem = null;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<InvoiceProvider> ProviderItemsCollection { set; get; } =
            new ObservableCollection<InvoiceProvider>();

        public InvoiceProvider CurrentProviderItem
        {
            get => myCurrentProviderItem;
            set
            {
                if (myCurrentProviderItem != null && myCurrentProviderItem.Equals(value)) return;
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
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        kontragentDC, isAccepted))
                        ProviderItemsCollection.Add(d);
                else
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        SearchText, isAccepted))
                        ProviderItemsCollection.Add(d);
                ClientItemsCollection.Clear();
                if (kontragentDC > 0)
                    foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                        isUsePayment, kontragentDC, isAccepted))
                        ClientItemsCollection.Add(d);
                else
                    foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                        isUsePayment, SearchText, isAccepted))
                        ClientItemsCollection.Add(d);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #endregion Commands
    }
}