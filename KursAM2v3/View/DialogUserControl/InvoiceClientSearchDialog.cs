﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using JetBrains.Annotations;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.InvoicesRepositories;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;

namespace KursAM2.View.DialogUserControl;

public sealed class InvoiceClientSearchDialog : RSWindowViewModelBase, IDataUserControl
{
    private readonly List<decimal> _ExcludeSfDCs;
    private readonly Currency currency;
    private readonly bool isAccepted;
    private readonly bool isPaymentUse;
    private readonly decimal kontragentDC;
    private readonly Waybill waybill;
    private InvoiceClientViewModel myCurrentItem;
    private StandartDialogSelectUC myDataUserControl;

    public InvoiceClientSearchDialog(bool isPaymentUse, bool isUseAcepted, Currency crs = null)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета клиенту";
        this.isPaymentUse = isPaymentUse;
        isAccepted = isUseAcepted;
        currency = crs;
        RefreshData(null);
    }

    public InvoiceClientSearchDialog(bool isPaymentUse, bool isUseAcepted, List<decimal> ecxcludeSfDCs,
        Currency crs = null)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета клиенту";
        this.isPaymentUse = isPaymentUse;
        isAccepted = isUseAcepted;
        currency = crs;
        _ExcludeSfDCs = ecxcludeSfDCs;
        RefreshData(null);
    }

    public InvoiceClientSearchDialog(decimal kontragentDC, bool isPaymentUse, bool isUseAcepted)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета клиенту";
        this.kontragentDC = kontragentDC;
        this.isPaymentUse = isPaymentUse;
        isAccepted = isUseAcepted;
        waybill = null;
        RefreshData(null);
    }

    public InvoiceClientSearchDialog(decimal kontragentDC, bool isPaymentUse, bool isUseAcepted,
        List<decimal> ecxcludeSfDCs)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета клиенту";
        this.kontragentDC = kontragentDC;
        this.isPaymentUse = isPaymentUse;
        isAccepted = isUseAcepted;
        waybill = null;
        _ExcludeSfDCs = ecxcludeSfDCs;
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
                        if (currency == null)
                        {
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                        else
                        {
                            if (d.Currency.DocCode != currency.DocCode) continue;
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                else
                    foreach (var d in InvoicesManager.GetInvoicesClient(DateTime.Today.AddDays(-300),
                                 DateTime.Today,
                                 isPaymentUse, null, SearchText, isAccepted))
                        if (currency == null)
                        {
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                        else
                        {
                            if (d.Currency.DocCode != currency.DocCode) continue;
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
            }
            else
            {
                foreach (var d in InvoicesManager.GetInvoicesClient(waybill))
                    if (currency == null)
                    {
                        if (_ExcludeSfDCs == null)
                        {
                            ItemsCollection.Add(d);
                        }
                        else
                        {
                            if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                            ItemsCollection.Add(d);
                        }
                    }
                    else
                    {
                        if (d.Currency.DocCode != currency.DocCode) continue;
                        if (_ExcludeSfDCs == null)
                        {
                            ItemsCollection.Add(d);
                        }
                        else
                        {
                            if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                            ItemsCollection.Add(d);
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
    private readonly List<decimal> _ExcludeSfDCs;

    // ReSharper disable once CollectionNeverQueried.Global
    public ObservableCollection<IInvoiceProvider> ItemsCollection { set; get; } =
        new ObservableCollection<IInvoiceProvider>();

    public InvoiceProvider CurrentItem
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
    private readonly Currency currency;


#pragma warning disable 169
    private readonly GenericKursDBRepository<InvoiceProvider> baseRepository;
#pragma warning restore 169

    // ReSharper disable once NotAccessedField.Local
#pragma warning disable 169
    private readonly IInvoiceProviderRepository invoiceProviderRepository;
    private DateTime myDateStart;
    private DateTime myDateEnd;
#pragma warning restore 169

    #endregion

    #region Constructors

    public InvoiceProviderSearchDialog(bool isUsePayment, bool isUseAccepted,
        bool isOnlyLastYear = false, Currency crs = null)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета поставщика";
        isPaymentUse = isUsePayment;
        isAccepted = isUseAccepted;
        this.isOnlyLastYear = isOnlyLastYear;
        currency = crs;
        DateEnd = DateTime.Today;
        DateStart = DateTime.Today.AddDays(-365);
        PeriodPanelVisibility = Visibility.Visible;
        RefreshData(null);
    }

    public InvoiceProviderSearchDialog(bool isUsePayment, bool isUseAccepted, List<decimal> excludeSfDCs,
        bool isOnlyLastYear = false, Currency crs = null)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета поставщика";
        isPaymentUse = isUsePayment;
        isAccepted = isUseAccepted;
        this.isOnlyLastYear = isOnlyLastYear;
        currency = crs;
        _ExcludeSfDCs = excludeSfDCs;
        DateEnd = DateTime.Today;
        DateStart = DateTime.Today.AddDays(-365);
        PeriodPanelVisibility = Visibility.Visible;
        RefreshData(null);
    }

    public InvoiceProviderSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAccepted,
        bool isOnlyLastYear = false)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        kontragentDC = kontrDC;
        WindowName = "Выбор счета поставщика";
        isPaymentUse = isUsePayment;
        isAccepted = isUseAccepted;
        this.isOnlyLastYear = isOnlyLastYear;
        DateEnd = DateTime.Today;
        DateStart = DateTime.Today.AddDays(-365);
        PeriodPanelVisibility = Visibility.Visible;
        RefreshData(null);
    }

    public InvoiceProviderSearchDialog(decimal kontrDC, bool isUsePayment, bool isUseAccepted,
        List<decimal> excludeSfDCs, bool isOnlyLastYear = false)
    {
        LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        kontragentDC = kontrDC;
        WindowName = "Выбор счета поставщика";
        isPaymentUse = isUsePayment;
        isAccepted = isUseAccepted;
        this.isOnlyLastYear = isOnlyLastYear;
        _ExcludeSfDCs = excludeSfDCs;
        DateEnd = DateTime.Today;
        DateStart = DateTime.Today.AddDays(-365);
        PeriodPanelVisibility = Visibility.Visible;
        RefreshData(null);
    }

    #endregion

    #region Properties

    public Visibility PeriodPanelVisibility { set; get; } = Visibility.Hidden;

    public DateTime DateStart
    {
        set
        {
            if (value.Equals(myDateStart)) return;
            myDateStart = value;
            RaisePropertyChanged();
        }
        get => myDateStart;
    }

    public DateTime DateEnd
    {
        set
        {
            if (value.Equals(myDateEnd)) return;
            myDateEnd = value;
            RaisePropertyChanged();
        }
        get => myDateEnd;
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
                    foreach (var d in InvoicesManager.GetInvoicesProvider(DateStart, DateEnd,
                                 true,
                                 kontragentDC, isAccepted))
                        if (currency == null)
                        {
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                        else
                        {
                            if (d.Currency.DocCode == currency.DocCode)
                            {
                                if (_ExcludeSfDCs == null)
                                {
                                    ItemsCollection.Add(d);
                                }
                                else
                                {
                                    if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
                else
                    foreach (var d in InvoicesManager.GetInvoicesProvider(DateStart,
                                 DateEnd,
                                 true,
                                 kontragentDC, isAccepted))
                        if (currency == null)
                        {
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                        else
                        {
                            if (d.Currency.DocCode == currency.DocCode)
                            {
                                if (_ExcludeSfDCs == null)
                                {
                                    ItemsCollection.Add(d);
                                }
                                else
                                {
                                    if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                    ItemsCollection.Add(d);
                                }
                            }
                        }
            }

            else
            {
                foreach (var d in InvoicesManager.GetInvoicesProvider(DateStart,
                             DateEnd,
                             isPaymentUse,
                             SearchText, isAccepted))
                    if (currency == null)
                    {
                        if (_ExcludeSfDCs == null)
                        {
                            ItemsCollection.Add(d);
                        }
                        else
                        {
                            if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                            ItemsCollection.Add(d);
                        }
                    }
                    else
                    {
                        if (d.Currency.DocCode == currency.DocCode)
                        {
                            if (_ExcludeSfDCs == null)
                            {
                                ItemsCollection.Add(d);
                            }
                            else
                            {
                                if (_ExcludeSfDCs.Contains(d.DocCode)) continue;
                                ItemsCollection.Add(d);
                            }
                        }
                    }
                //}
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
    private readonly Currency myCurrency;
    private IInvoiceClient myCurrentClientItem;
    private IInvoiceProvider myCurrentProviderItem;
    private AllInvocesDialogSelectUC myDataUserControl;

    public InvoiceAllSearchDialog(bool isUsePayment, bool isUseAcepted, Currency crs = null)
    {
        LayoutControl = myDataUserControl = new AllInvocesDialogSelectUC(GetType().Name);
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        WindowName = "Выбор счета";
        this.isUsePayment = isUsePayment;
        isAccepted = isUseAcepted;
        myCurrency = crs;
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
            if (Equals(myCurrentClientItem, value)) return;
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
            if (Equals(myCurrentProviderItem, value)) return;
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
        //SearchClear(null);
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
            ProviderItemsCollection.Clear();
            if (kontragentDC > 0)
            {
                foreach (var d in InvoicesManager.GetInvoicesProvider(DateTime.Today.AddDays(-365), DateTime.Today,
                             true,
                             kontragentDC, isAccepted))
                    ProviderItemsCollection.Add(d);
            }
            else
            {
                var providerRepository = new InvoiceProviderRepository(GlobalOptions.GetEntities());
                foreach (var inv in providerRepository.GetAllByDates(new DateTime(2000,1,1), DateTime.Today)
                             .Where(_ => (_.IsExcludeFromPays ?? false) == false)
                             .Where(inv => !isUsePayment || inv.Summa > inv.PaySumma)
                             .Where(inv => !isAccepted || inv.IsAccepted))
                {
                    if (myCurrency == null)
                    {
                        ProviderItemsCollection.Add(inv);
                        continue;
                    }

                    if (inv.Currency == myCurrency)
                        ProviderItemsCollection.Add(inv);
                }
            }

            ClientItemsCollection.Clear();
            if (kontragentDC > 0)
            {
                foreach (var d in InvoicesManager.GetInvoicesClient(new DateTime(2000,1,1), DateTime.Today,
                             isUsePayment, kontragentDC, isAccepted))
                    ClientItemsCollection.Add(d);
            }
            else
            {
                var clientRepository = new InvoiceClientRepository(GlobalOptions.GetEntities());
                foreach (var inv in clientRepository.GetAllByDates(DateTime.Today.AddDays(-365), DateTime.Today)
                             .Where(inv => !isUsePayment || inv.Summa > inv.PaySumma)
                             .Where(inv => !isAccepted || inv.IsAccepted))
                {
                    if (myCurrency == null)
                    {
                        ClientItemsCollection.Add(inv);
                        continue;
                    }

                    if (inv.Currency == myCurrency)
                        ClientItemsCollection.Add(inv);
                }
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    #endregion Commands
}
