using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using JetBrains.Annotations;
using KursAM2.Managers.Invoices;

namespace KursAM2.View.DialogUserControl
{
    public class InvoiceClientSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool isAccepted;
        private readonly bool IsPaymentUse;
        private readonly decimal KontragentDC;
        private InvoiceClient myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;
        private readonly Waybill Waybill;

        public InvoiceClientSearchDialog(bool isPaymentUse, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            IsPaymentUse = isPaymentUse;
            isAccepted = isUseAcepted;
            RefreshData(null);
        }

        public InvoiceClientSearchDialog(decimal kontragentDC, bool isPaymentUse, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            KontragentDC = kontragentDC;
            IsPaymentUse = isPaymentUse;
            isAccepted = isUseAcepted;
            Waybill = null;
            RefreshData(null);
        }

        public InvoiceClientSearchDialog([NotNull]Waybill waybill)
        {
            Waybill = waybill;
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
            {  ItemsCollection.Clear();
                if (Waybill == null)
                {

                    if (KontragentDC > 0)
                        foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                            IsPaymentUse, KontragentDC, isAccepted))
                            ItemsCollection.Add(d);
                    else
                        foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                            IsPaymentUse, SearchText, isAccepted))
                            ItemsCollection.Add(d);
                }
                else
                {
                    foreach (var d in InvoicesManager.GetInvoicesClient(Waybill))
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

    public class InvoiceProviderSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly bool isAccepted;

        // ReSharper disable once NotAccessedField.Local
        private readonly bool IsPaymentUse;
        private readonly decimal KontragentDC;
        private InvoiceProvider myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public InvoiceProviderSearchDialog(bool isUsePayment, bool isUseAccepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            IsPaymentUse = isUsePayment;
            isAccepted = isUseAccepted;
            RefreshData(null);
        }

        public InvoiceProviderSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAccepted)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            KontragentDC = kontrDC;
            WindowName = "Выбор счета";
            IsPaymentUse = isUsePayment;
            isAccepted = isUseAccepted;
            RefreshData(null);
        }

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

        #region Commands

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);

        public override void Search(object obj)
        {
            try
            {
                ItemsCollection.Clear();
                if (KontragentDC > 0)
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        KontragentDC, isAccepted))
                        ItemsCollection.Add(d);
                else
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        SearchText, isAccepted))
                        ItemsCollection.Add(d);
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

    public class InvoiceAllSearchDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly decimal KontragentDC;
        private readonly bool isAccepted;
        private readonly bool IsUsePayment;
        private InvoiceClient myCurrentClientItem;
        private InvoiceProvider myCurrentProviderItem;
        private AllInvocesDialogSelectUC myDataUserControl;

        public InvoiceAllSearchDialog(bool isUsePayment, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            IsUsePayment = isUsePayment;
            isAccepted = isUseAcepted;
            RefreshData(null);
        }

        public InvoiceAllSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAcepted)
        {
            LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор счета";
            KontragentDC = kontrDC;
            IsUsePayment = isUsePayment;
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
                if (KontragentDC > 0)
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        KontragentDC, isAccepted))
                        ProviderItemsCollection.Add(d);
                else
                    foreach (var d in InvoicesManager.GetInvoicesProvider(new DateTime(2000, 1, 1), DateTime.Today,
                        true,
                        SearchText, isAccepted))
                        ProviderItemsCollection.Add(d);
                ClientItemsCollection.Clear();
                if (KontragentDC > 0)
                    foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                        IsUsePayment, KontragentDC, isAccepted))
                        ClientItemsCollection.Add(d);
                else
                    foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000, 1, 1), DateTime.Today,
                        IsUsePayment, SearchText, isAccepted))
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