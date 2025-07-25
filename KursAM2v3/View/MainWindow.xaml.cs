﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.Auxiliary;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.Finance;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Finance.DistributeNaklad;
using KursAM2.View.KursReferences;
using KursAM2.View.Logistiks;
using KursAM2.View.Management;
using KursAM2.View.Period;
using KursAM2.View.Personal;
using KursAM2.View.Projects;
using KursAM2.View.Reconciliation;
using KursAM2.View.Repozit;
using KursAM2.View.Shop;
using KursAM2.View.StockHolder;
using KursAM2.ViewModel;
using KursAM2.ViewModel.Dogovora;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Logistiks.AktSpisaniya;
using KursAM2.ViewModel.Logistiks.NomenklReturn;
using KursAM2.ViewModel.Logistiks.TransferOut;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursAM2.ViewModel.Management;
using KursAM2.ViewModel.Management.BreakEven;
using KursAM2.ViewModel.Management.DebitorCreditor;
using KursAM2.ViewModel.Management.ManagementBalans;
using KursAM2.ViewModel.Management.Projects;
using KursAM2.ViewModel.Period;
using KursAM2.ViewModel.Personal;
using KursAM2.ViewModel.Reconcilation;
using KursAM2.ViewModel.Reference;
using KursAM2.ViewModel.Reference.Kontragent;
using KursAM2.ViewModel.Reference.Nomenkl;
using KursAM2.ViewModel.Repozit;
using KursAM2.ViewModel.Shop;
using KursAM2.ViewModel.Signatures;
using KursAM2.ViewModel.StartLogin;
using KursAM2.ViewModel.StockHolder;
using KursDomain;
using KursDomain.Repository.StorageLocationsRepositury;
using LayoutManager;
using NomenklCostReset = KursAM2.View.Logistiks.NomenklCostReset;

namespace KursAM2.View
{
    //TODO Выынести вызов форм в отдельный класс
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ILayout
    {
        private const int TimeForCheckVersion = 1000 * 25;
        private static Mutex _mutex = new Mutex();
        private int currentMainGroupId;

        private Tile myCurrentTile;
        public DispatcherTimer VersionUpdateTimer;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                LayoutManager =
                    new LayoutManager.LayoutManager(GlobalOptions.KursSystem(), "MainWindow", this, dockLayout1);
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                Closed += MainWindow_Closed;
            }
            catch (Exception ex)
            {
                var s = new StringBuilder();
                s.Append(ex.Message + "\nInnerExcepption:");
                var ex1 = ex.InnerException;
                while (ex1?.InnerException != null)
                {
                    s.Append(ex1.InnerException.Message);
                    ex1 = ex1.InnerException;
                }

                MessageBox.Show(s.ToString());
            }
        }

        public Tile CurrentTile
        {
            get => myCurrentTile;
            set
            {
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (myCurrentTile == value) return;
                myCurrentTile = value;
            }
        }

        public ICommand ProgramCloseCommnd
        {
            get { return new Command(ProgramClose, _ => true); }
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }


        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void versionUpdater_Thick(object sender, EventArgs e)
        {
            var ver = new VersionManager((MainWindowViewModel)DataContext);
            var checkMultiUser = ver.GetCanUpdate();
            if (checkMultiUser == false)
            {
                ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = checkMultiUser;
                return;
            }
            var res = ver.CheckVersion();
            if (res != null)
            {
                switch (res.UpdateStatus)
                {
                    case 0:
                        ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = false;
                        break;
                    case 1:
                    case 2:
                        ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = true;
                        if (res.UpdateStatus == 2) ver.KursUpdate();
                        break;
                }
            }

        }


        private void ProgramClose(object obj)
        {
            Close();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //LayoutManager.Save();
            foreach (var f in from Window f in Application.Current.Windows where f != null select f)
            {
                if (f is MainWindow) continue;
                if (f is KursBaseSearchWindow s)
                    if (s.DataContext is DistributeNakladSearchViewModel ctx)
                        ctx.OnWindowClosing(null);
                if (f is ILayout l)

                    l.SaveLayout();
                f.Close();
            }
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            DocumentLayoutPanel.Caption = "";
            if (DataContext is MainWindowViewModel dtx)
            {
                dtx.Form = this;
#if RELEASE 
                var vers = new VersionManager(dtx);
                var res = vers.CheckVersion();
                if (res.UpdateStatus == 0)
                {
                    ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = false;
                    return;
                }
                var ver = vers.GetCanUpdate();
                    if (ver == false)
                    {
                        ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = ver;
                        return;
                    }
                    ((MainWindowViewModel)DataContext).IsVersionUpdateStatus = true;
#endif

            }
        }

        public void OpenForm(Window win, Window owner, RSWindowViewModelBase datacontext)
        {
        }

        public static void OpenWindow(string formName)
        {
            try
            {
                // ReSharper disable once TooWideLocalVariableScope
                Window form;
                switch (formName)
                {
                    //Возврат товара от клиента
                    case "Возврат товара от клиента":
                        var retClient = new NomenklReturnOfClientSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = retClient
                        };
                        retClient.Form = form;
                        form.Show();
                        break;

                    case "Возвра товара поставщику":
                        var retProvider = new NomenklReturnToProviderSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = retProvider
                        };
                        retProvider.Form = form;
                        form.Show();
                        break;

                    case "Перевод за баланс":
                        var transOut = new TransferOutBalansSearchViewModel();
                        transOut.Show();
                        break;
                    case "Товары за балансом":
                        var outBalans = new TransferOutBalansRemainsViewModel();
                        outBalans.Show();
                        Task.Run(() => outBalans.InitializeAsync(0));
                        break;
                    case "Справочник мест хранения":
                        var slocCtx =
                            new StorageLocationViewModel(new StorageLocationsRepository(GlobalOptions.GetEntities()));
                        slocCtx.Show();
                        Task.Run(() => slocCtx.OnRefreshDataAsync());
                        break;
                    //Справочник валют
                    case "Справочник валют":
                        var crsref = new CurrencyReferenceWindowViewModel();
                        var formcrs = new KursStandartFormView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = crsref
                        };
                        formcrs.Show();
                        break;

                    case "Справочник банков":
                        var bankref = new BankReferenceWindowViewModel();
                        var formbank = new KursStandartFormView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = bankref
                        };
                        bankref.Form = formbank;
                        formbank.Show();
                        break;

                    //Лицевые счета акционеров
                    case "Лицевые счета акционеров":
                        var shbls = new StockHoldersBalancesWindowViewModel();
                        form = new StockHoldersBalancesView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = shbls
                        };
                        shbls.Form = form;
                        form.Show();
                        shbls.RefreshData(null);
                        break;
                    //Ведомости наяислений акционерам
                    case "Ведомости начислений акционерам":
                        var sholda = new StockHolderAccrualSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = sholda
                        };
                        sholda.Form = form;
                        form.Show();
                        break;

                    //Типы начислений акционерам
                    case "Типы начислений акционерам":
                        var sholdat = new StockHolderAccrualTypeWindowViewModel();
                        form = new StockHolderAccrualTypeView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = sholdat
                        };
                        sholdat.Form = form;
                        form.Show();
                        break;

                    //Реестр акционеров
                    case "Реестр акционеров":
                        var shold = new StockHolderReestrWindowViewModel();
                        form = new StockHolderReestrView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = shold
                        };
                        shold.Form = form;
                        form.Show();
                        break;

                    //Настройка подписей
                    case "Настройка подписей":
                        form = new KursStandartFormView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var sign = new SignaturesWindowViewModel
                        {
                            Form = form
                        };
                        form.DataContext = sign;
                        sign.Form = form;
                        form.Show();
                        break;
                    // Начисления вынебалансовым Клиентам
                    case "Прямые услуги для клиентов":
                        var aad = new AccuredAmountForClientSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = aad
                        };
                        aad.Form = form;
                        form.Show();
                        break;

                    case @"Реестр прямых затрат":
                        var aap = new AccuredAmountOfSupplierSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = aap
                        };
                        aap.Form = form;
                        form.Show();
                        break;

                    case "Договора от поставщиков":
                        var dop = new DogovorOfSupplierSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = dop
                        };
                        dop.Form = form;
                        form.Show();
                        break;

                    // Типы начислений для внебалансовых контрагентов
                    case "Типы начислений для внебаласовых контрагентов":
                        var aat = new AccruedAmountTypeWindowViewModel();
                        form = new AccruedAmountTypeView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = aat
                        };
                        aat.Form = form;
                        form.Show();
                        break;

                    // Акт списания материалов
                    case "Акт списания материалов":
                        var actCtx = new AktSpisaniyaNomenklSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = actCtx;
                        actCtx.Form = form;
                        form.Show();
                        break;
                    //Разбор данных для Shop
                    case "Разбор данных для Shop":
                        var shopCTX = new ShopParserExtFilesWindowViewModel();
                        form = new ShopParserExtFiles
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = shopCTX;
                        shopCTX.Form = form;
                        form.Show();
                        break;
                    case "Последние документы пользователей":
                        var ldoc = new LastUsersDocumentWindowViewModel();
                        form = new LastUsersDocumentView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = ldoc;
                        ldoc.Form = form;
                        form.Show();
                        break;
                    case "  Дебиторы / Кредиторы":
                        var dbctx = new DebitorCreditorWindowViewModel();
                        form = new DebitorCreditorView { Owner = Application.Current.MainWindow };
                        dbctx.Form = form;
                        form.DataContext = dbctx;
                        form.Show();
                        break;
                    case "Контрагенты для документов":
                        form = new KontragentRefOutView { Owner = Application.Current.MainWindow };
                        var kdocCtx = new KontragentReferenceOutWindowViewModel(form);
                        form.DataContext = kdocCtx;
                        form.Show();
                        break;
                    case "Договора для клиентов":
                        var ctxdog = new DogovorClientSearchViewModel
                        {
                            Form = new StandartSearchView
                            {
                                Owner = Application.Current.MainWindow
                            }
                        };
                        ctxdog.Form.DataContext = ctxdog;
                        ctxdog.Form.Show();
                        break;
                    case "Лицевые счета контрагентов":
                        form = new KontragentBalansView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var ctxk = new KontragentBalansWindowViewModel
                        {
                            Form = form
                        };
                        form.DataContext = ctxk;
                        form.Show();
                        break;
                    case "  Рентабельность":
                        var renCtx = new BreakEvenWindowViewModel();
                        form = new BreakEvenForm2
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = renCtx
                        };
                        renCtx.Form = form;
                        form.Show();
                        break;
                    case "Лицевые счета сотрудников":
                        var psCtx = new EmployeePayWindowViewModel();
                        form = new PersonalPaysView { Owner = Application.Current.MainWindow, DataContext = psCtx };
                        psCtx.Form = form;
                        psCtx.RefreshData(null);
                        form.Show();
                        break;
                    case "Справочник начислений для з/п":
                        var zpCtx = new PayrollTypeWindowViewModel();
                        form = new PayRollTypeReference { Owner = Application.Current.MainWindow, DataContext = zpCtx };
                        zpCtx.Form = form;
                        form.Show();
                        break;
                    case "Права доступа к лицевым счетам сотрудников":
                        form = new UsersRightToPayRoll { Owner = Application.Current.MainWindow };
                        form.Show();
                        break;
                    case "Ведомости начислений з/платы":
                        var vedCtx = new PayrollSearchWindowViewModel();
                        form = new PayRollDocSearch
                        {
                            Owner = Application.Current.MainWindow
                        };
                        vedCtx.Form = form; 
                        form.DataContext = vedCtx;
                        vedCtx.RefreshData(null);
                        form.Show();
                        break;
                    case "Управленческий баланс":
                        var ctx1 = new ManagementBalansWindowViewModel { CurrentDate = DateTime.Today };
                        form = new ManagementBalansView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctx1
                        };
                        form.DataContext = ctx1;
                        ctx1.Form = form;
                        form.Show();
                        break;
                    case "Прибыли и убытки 2":
                        var ctxpb2 = new ProfitAndLossesWindowViewModel2();
                        form = new ProfitAndLoss
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxpb2
                        };
                        ctxpb2.Form = form;
                        ctxpb2.RaisePropertyAllChanged();
                        form.Show();

                        break;
                    case "Грузовые реквизиты контрагентов":
                        var ctx3 = new KontragentGruzoInfoWindowViewModel();
                        ctx3.RefreshData(null);
                        form = new KontragentGruzoInfoView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctx3
                        };
                        ctx3.Form = form;
                        form.Show();
                        break;
                    case "Счета-Фактуры клиентам":
                        form = new StandartSearchView { Owner = Application.Current.MainWindow };
                        var ctxsf = new InvoiceClientSearchViewModel(form);
                        form.DataContext = ctxsf;
                        form.Show();
                        break;
                    case "Счета-фактуры поставщиков":
                        form = new StandartSearchView { Owner = Application.Current.MainWindow };
                        var dtx = new SearchInvoiceProviderViewModel(form);
                        form.DataContext = dtx;
                        form.Show();
                        break;
                    case "Расходные накладные для клиентов":
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var ctxNaklad = new WaybillSearchViewModel
                        {
                            Form = form
                        };
                        form.DataContext = ctxNaklad;
                        form.Show();
                        break;
                    case "Справочник сотрудников":
                        form = new PersonaReference { Owner = Application.Current.MainWindow };
                        var ctxPers = new PersonaReferenceWindowViewModel();
                        ctxPers.Form = form;
                        form.Show();
                        form.DataContext = ctxPers;
                        break;
                    case "  Доступ к сч. контр-тов":
                        form = new KontragentRightsControl { Owner = Application.Current.MainWindow };
                        form.Show();
                        break;
                    case "Справочник контрагентов":
                        form = new KontragentReference2View { Owner = Application.Current.MainWindow };
                        var ctxRef = new KontragentReferenceWindowViewModel
                        {
                            Form = form
                        };
                        form.DataContext = ctxRef;
                        form.Show();

                        break;
                    case "Справочник складов":
                        var frm = new TreeListFormBaseView2
                        {
                            LayoutManagerName = "NomenklStore",
                            Owner = Application.Current.MainWindow
                        };
                        var ctxRef1 = new Store2ReferenceWindowViewModel { Form = frm };
                        ctxRef1.Form = frm;
                        ctxRef1.RefreshData(null);
                        frm.DataContext = ctxRef1;
                        frm.Show();
                        break;
                    case "Справочник регионов":
                        var frm1 = new TreeListFormBaseView
                        {
                            LayoutManagerName = "RegionReference",
                            Owner = Application.Current.MainWindow
                        };
                        var ctxRef2 = new RegionRefViewModel { Form = frm1 };
                        ctxRef2.Form = frm1;
                        ctxRef2.RefreshData(null);
                        frm1.DataContext = ctxRef2;
                        frm1.Show();
                        break;
                    case "Закрытие периодов":
                        var ctx = new PeriodGroupsReferenceViewModel();
                        form = new PeriodCloseManagementView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctx
                        };
                        ctx.Form = form;
                        form.Show();
                        break;
                    case "Приходный складской ордер":
                        var ctxq = new WarehouseOrderInSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxq
                        };
                        ctxq.Form = form;
                        form.Show();
                        break;
                    case "Расходный складской ордер":
                        var ctxr = new WarehouseOrderOutSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxr
                        };
                        ctxr.Form = form;
                        form.Show();
                        break;
                    case " Акты сверки":
                        var aorCtx = new AOFViewModel();
                        form = new ActOfReconciliation { Owner = Application.Current.MainWindow, DataContext = aorCtx };
                        aorCtx.Form = form;
                        form.Show();
                        break;
                    case " Рентабельность ШОП":
                        var shopCtx = new ShopRentabelnostViewModel();
                        form = new ShopRentabelnost
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = shopCtx
                        };
                        shopCtx.Form = form;
                        form.Show();
                        break;
                    case "Остатки товаров на складах":
                        var ctxost = new SkladOstatkiWindowViewModel();
                        form = new SkladOstatkiView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost
                        };
                        ctxost.Form = form;
                        //ctxost.RefreshData(null);
                        form.Show(); 
                        break;
                    case "Калькуляция себестоимости":
                        var ctxost1 = new NomenklCostCalculatorWindowViewModel(null);
                        form = new NomenklCostCalculator
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost1
                        };
                        ctxost1.Form = form;
                        ctxost1.RefreshData(null);
                        form.Show();
                        break;
                    case "Внутреннее перемещение товаров":
                        var ctxost2 = new NomenklInnerMoveWindowViewModel();
                        form = new NomenklInnerMove
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost2
                        };
                        ctxost2.Form = form;
                        ctxost2.RefreshData(null);
                        form.Show();
                        break;
                    case "Переоценка товара":
                        var ctxost3 = new NomenklCostResetWindowViewModel();
                        form = new NomenklCostReset
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost3
                        };
                        ctxost3.Form = form;
                        ctxost3.RefreshData(null);
                        form.Show();
                        break;
                    case "Права доступа для пользователей":

                        var ctxost4 = new UsersHorizontalRightWindowViewModel();
                        form = new UsersHorizontalRight
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost4
                        };
                        ctxost4.Form = form;
                        ctxost4.RefreshData(null);
                        form.Show();
                        break;
                    case "Таксировка валюты товаров":
                        form = new SaleTaxNomenklView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = new SaleTaxNomenklWindowViewModel(form);
                        form.Show();
                        break;
                    case "Движение денежных средств":
                        form = new MoneyPeriodMove
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var dv = new MoneyPeriodWindowViewModel(form);
                        dv.RefreshData(null);
                        form.DataContext = dv;
                        form.Show();
                        break;
                    case "Движение товаров":
                        var form1 = new NomenklMoveOnSklad
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var ctxNomMove = new NomenklMoveOnSkladWindowViewModel(form1);
                        form1.DataContext = ctxNomMove;
                        form1.Show();
                        break;
                    case "Номенклатурный справочник":
                        var nCtx = new ReferenceWindowViewModel();
                        form = new NomenklReferenceView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = nCtx
                        };
                        nCtx.Form = form;
                        form.Show();
                        break;
                    case "Акт валютной таксировки номенклатур":
                        try
                        {
                            var taxCtx = new NomenklTransferSearchViewModel();
                            form = new NomenklTransferSearchView
                            {
                                Owner = Application.Current.MainWindow,
                                DataContext = taxCtx
                            };
                            taxCtx.Form = form;
                            form.Show();
                        }
                        catch (Exception ex)
                        {
                            WindowManager.ShowError(ex);
                        }

                        break;
                    case "Справочник проектов":
                        var prjCtx = new ProjectReferenceWindowViewModel();
                        form = new ProjectReferenceView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = prjCtx
                        };
                        prjCtx.Form = form;
                        form.Show();
                        break;
                    case "Справочник центров ответственности":
                        var centerCtx = new ReferenceOfResponsibilityCentersWindowViewModel();
                        form = new ReferenceOfResponsibilityCentersView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = centerCtx
                        };
                        centerCtx.Form = form;
                        form.Show();
                        break;
                    case "Справочник стран":
                        var countryCtx = new CountriesRefWindowViewModel();
                        form = new CountryReferenceView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = countryCtx
                        };
                        countryCtx.Form = form;
                        form.Show();
                        break;
                    case "Инвентаризационные ведомости":
                        var invCtx = new InventorySheetSearchWindowViewModel();
                        form = new InventorySheetSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = invCtx
                        };
                        invCtx.Form = form;
                        form.Show();
                        break;
                    case "Распределение накладных расходов":
                        var dnakForm = new KursBaseSearchWindow
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var v = new DistributeNakladSearchView(dnakForm);
                        dnakForm.modelViewControl.Content = v;
                        ((KursBaseControlViewModel)v.DataContext).Form = dnakForm;
                        dnakForm.DataContext = v.DataContext;
                        dnakForm.Show();
                        break;

                    case "Сравнение балансов":
                        form = new ManagementBalansCompareView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = new ManagementBalansCompareWindowViewModel(form);
                        form.Show();
                        break;
                    case "Типы актов взаимозачета":
                        form = new MUtualAccountingTypeRefView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var mutCtx = new MutualAcountingRefWindowViewModel();
                        mutCtx.Form = form;
                        form.DataContext = mutCtx;
                        form.Show();
                        break;
                    case "Акты взаимозачетов":
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var mutCtx2 = new MutualAccountingWindowSearchViewModel()
                        {
                            WindowName = "Поиск актов взаимозачета",
                            Form = form
                        };
                        form.DataContext = mutCtx2;
                        form.Show();
                        break;
                    case "Акты валютной конвертации":
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        var mutCtx3 = new MutualAccountingConvertWindowSearchViewModel()
                        {
                            WindowName = "Поиск актов конвертации",
                            Form = form
                        };
                        form.DataContext = mutCtx3;
                        form.Show();
                        break;
                    case "Проекты":
                        var pctx = new ProjectManagerWindowViewModel();
                        form = new ProjectManager
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = pctx
                        };
                        pctx.Form = form;
                        form.Show();
                        break;
                    case "Справочник касс и банковских счетов":
                        var rcbctx = new BankAndCashReferenceViewModel();
                        form = new CashAndBanksReferenceView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = rcbctx
                        };
                        rcbctx.Form = form;
                        form.Show();
                        break;
                    case "Банковские операции":
                        form = new BankOperationsView2
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = new BankOperationsWindowViewModel2(form);
                        form.Show();
                        break;
                    case "Приходный кассовый ордер":
                        form = new CashInSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = new CashInWindowSearchViewModel
                        {
                            Form = form
                        };
                        form.Show();
                        break;
                    case "Расходный кассовый ордер":
                        form = new CashOutSearchView
                        {
                            Owner = Application.Current.MainWindow
                        };
                        form.DataContext = new CashOutWindowSearchViewModel
                        {
                            Form = form
                        };
                        form.Show();
                        break;
                    case "Настройка прав доступа":
                        var ctxAccessRight = new UsersManagerViewModel();
                        var form2 = new UsersManagerView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxAccessRight
                        };
                        ctxAccessRight.Form = form2;
                        form2.Show();
                        break;

                    case "Движение товара по проектам":
                        var ctxProjectNomenkl = new ProjectNomenklMoveViewModel();
                        form = new ProjectNomenklMove
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxProjectNomenkl
                        };
                        ctxProjectNomenkl.Form = form;
                        form.Show();
                        break;
                    //case "Прибыли и убытки по проектам":
                    //    var ctxProjectProfitAndLoss = new ProjectProfitAndLossesWindowViewModelOld();
                    //    form = new ProjectProfitAndLossView
                    //    {
                    //        Owner = Application.Current.MainWindow,
                    //        DataContext = ctxProjectProfitAndLoss
                    //    };
                    //    ctxProjectProfitAndLoss.Form = form;
                    //    form.Show();
                    //    break;
                    case "Кассовая книга":
                        var ctxCash = new CashBookWindowViewModel();
                        form = new CashBookView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxCash
                        };
                        ctxCash.Form = form;
                        form.Show();
                        break;
                    case "Справочник единиц измерения":
                        var ctxUnit = new UnitReferenceWindowViewModel();
                        form = new GridFormBaseView
                        {
                            DataContext = ctxUnit,
                            LayoutManagerName = "Unit",
                            Owner = Application.Current.MainWindow
                        };
                        ctxUnit.Form = form;
                        form.Show();
                        break;
                    case "Справочник условий оплаты":
                        var ctxUsuagePay = new UsagePayReferenceWindowViewModel();
                        form = new GridFormBaseView
                        {
                            DataContext = ctxUsuagePay,
                            LayoutManagerName = "PayCondition",
                            Owner = Application.Current.MainWindow
                        };
                        ctxUsuagePay.Form = form;
                        form.Show();
                        break;
                    case "Справочник форм оплаты":
                        var ctxFormPay = new FormPayReferenceWindowViewModel();
                        form = new GridFormBaseView
                        {
                            DataContext = ctxFormPay,
                            LayoutManagerName = "FormPay",
                            Owner = Application.Current.MainWindow
                        };
                        ctxFormPay.Form = form;
                        form.Show();
                        break;
                    case "Справочник категорий клиентов":
                        var ctxKontrCat = new KontragentCategoryRefWindowViewModel();
                        form = new KontragentCategoryReferenceView
                        {
                            DataContext = ctxKontrCat,
                            Owner = Application.Current.MainWindow
                        };
                        ctxKontrCat.Form = form;
                        form.Show();
                        break;
                    case "Справочник видов продукции":
                        var ctxvidprod = new NomenklKindReferenceWindowViewModel();
                        form = new TreeListFormBaseView
                        {
                            DataContext = ctxvidprod,
                            LayoutManagerName = "NomenklProductKind",
                            Owner = Application.Current.MainWindow
                        };
                        ctxvidprod.Form = form;
                        ctxvidprod.RefreshData(null);
                        form.Show();
                        break;
                    case "Справочник типов номенклатур":
                        var ctxtypeprod = new ProductTypeReferenceWindowViewModel();
                        form = new GridFormBaseView
                        {
                            DataContext = ctxtypeprod,
                            LayoutManagerName = "NomenklProductType",
                            Owner = Application.Current.MainWindow
                        };
                        ctxtypeprod.Form = form;
                        form.Show();
                        break;
                    case "Справочник типов продукции":
                        var ctxtypeprod1 = new TypeOfProductReferenceWindowViewModel();
                        form = new GridFormBaseView
                        {
                            DataContext = ctxtypeprod1,
                            LayoutManagerName = "ProductOfType",
                            Owner = Application.Current.MainWindow
                        };
                        ctxtypeprod1.Form = form;
                        form.Show();
                        break;
                    case "Справочник категорий товара":
                        var ctxcatprod = new CategoryReferenceWindowViewModel();
                        form = new TreeListFormBaseView
                        {
                            DataContext = ctxcatprod,
                            LayoutManagerName = "NomenklGroup",
                            Owner = Application.Current.MainWindow
                        };
                        ctxcatprod.Form = form;
                        ctxcatprod.RefreshData(null);
                        form.Show();
                        break;
                    case "Справочник счетов доходов/расходов":
                        var sdrCtx = new SDRReferenceWindowViewModel();
                        form = new SDRReferenceView
                        {
                            DataContext = sdrCtx,
                            Owner = Application.Current.MainWindow
                        };
                        sdrCtx.Form = form;
                        form.Show();
                        break;
                    default:
                        MessageBox.Show("Не реализован");
                        break;
                }
            }
            catch (Exception ex)
            {
                var s = new StringBuilder();
                s.Append(ex.Message + "\n");
                if (ex.InnerException != null)
                    s.Append(ex.InnerException?.Message);
                MessageBox.Show(s.ToString());
            }
        }

        private void tileMainGroup_TileClick(object sender, TileClickEventArgs tileClickEventArgs)
        {
            DocumentLayoutPanel.Caption = tileClickEventArgs.Tile.ToolTip;
            tileDocumentItems.Children.Clear();
            currentMainGroupId = (int)tileClickEventArgs.Tile.Tag;
            var grp = GlobalOptions.UserInfo.MainTileGroups.FirstOrDefault(
                _ => (int)tileClickEventArgs.Tile.Tag == _.Id);
            if (grp == null) return;
            foreach (var item in tileMainGroup.Children)
                if (item is Tile t)
                    t.BorderThickness = new Thickness(0, 0, 0, 0);
            tileClickEventArgs.Tile.BorderThickness = new Thickness(3, 3, 3, 3);
            tileClickEventArgs.Tile.BorderBrush = Brushes.Orange;
            List<UserMenuFavorites> fvrts;
            using (var sctx = GlobalOptions.KursSystem())
            {
                fvrts = sctx.UserMenuFavorites
                    .Include(_ => _.DataSources)
                    .Include(_ => _.Users)
                    .Include(_ => _.KursMenuItem)
                    .AsNoTracking()
                    .Where(_ => _.DbId == GlobalOptions.DataBaseId
                                && _.UserId == GlobalOptions.UserInfo.KursId)
                    .ToList();
            }

            if (currentMainGroupId != 11)
                foreach (var tile in grp.TileItems.OrderBy(_ => _.OrderBy))
                {
                    var newTile = new Tile
                    {
                        Header = tile.Name,
                        Width = 250,
                        Height = 100,
                        Tag = tile.Id,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        HorizontalHeaderAlignment = HorizontalAlignment.Center
                    };
                    if (fvrts.All(_ => _.MenuId != tile.Id))
                    {
                        newTile.MouseMove += NewTile_MouseMove;
                        var menuAdd = new MenuItem
                        {
                            Header = "Добавить в избранное"
                        };
                        menuAdd.Click += AddToFun;
                        newTile.ContextMenu = new ContextMenu();
                        newTile.ContextMenu.Items.Add(menuAdd);
                    }

                    tileDocumentItems.Children.Add(newTile);
                }
            else
                using (var sctx = GlobalOptions.KursSystem())
                {
                    var data = sctx.UserMenuFavorites
                        .Include(_ => _.DataSources)
                        .Include(_ => _.Users)
                        .Include(_ => _.KursMenuItem)
                        .AsNoTracking()
                        .Where(_ => _.DbId == GlobalOptions.DataBaseId
                                    && _.UserId == GlobalOptions.UserInfo.KursId)
                        .ToList();
                    if (data.Any())
                        foreach (var d in data)
                        {
                            var newTile = new Tile
                            {
                                Header = d.KursMenuItem.Name,
                                Width = 250,
                                Height = 100,
                                Tag = d.KursMenuItem.Id,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                HorizontalHeaderAlignment = HorizontalAlignment.Center
                            };
                            newTile.MouseMove += NewTile_MouseMove;
                            var menuRemove = new MenuItem
                            {
                                Header = "Убрать из избранного"
                            };
                            menuRemove.Click += RemoveFromFun;
                            newTile.ContextMenu = new ContextMenu();
                            newTile.ContextMenu.Items.Add(menuRemove);
                            tileDocumentItems.Children.Add(newTile);
                        }
                }

            if (DataContext is MainWindowViewModel dtx)
            {
                dtx.SearchText = null;
                dtx.CurrentDocumentTiles.Clear();
                foreach (var tile in tileDocumentItems.Children.Cast<Tile>()) dtx.CurrentDocumentTiles.Add(tile);
            }
        }


        private void RemoveFromFun(object sender, RoutedEventArgs e)
        {
            if (CurrentTile != null)
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var old = ctx.UserMenuFavorites
                        .FirstOrDefault(_ => _.DbId == GlobalOptions.DataBaseId
                                             && _.UserId == GlobalOptions.UserInfo.KursId
                                             && _.MenuId == (int)CurrentTile.Tag);
                    if (old != null)
                    {
                        ctx.UserMenuFavorites.Remove(old);
                        ctx.SaveChanges();
                        tileDocumentItems.Children.Remove(CurrentTile);
                    }
                }
        }

        private void NewTile_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentTile = sender as Tile;
        }

        private void AddToFun(object sender, EventArgs e)
        {
            if (CurrentTile != null)
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var id = Guid.NewGuid();
                    ctx.UserMenuFavorites.Add(new UserMenuFavorites
                    {
                        Id = id,
                        DbId = GlobalOptions.DataBaseId,
                        UserId = GlobalOptions.UserInfo.KursId,
                        MenuId = (int)CurrentTile.Tag
                    });
                    ctx.SaveChanges();
                }
        }

        private void LoadTiles()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var tileOrders = GlobalOptions.KursSystem().UserMenuOrder
                    .Where(_ => _.UserId == GlobalOptions.UserInfo.KursId).ToList();
                var tileItems = ctx.KursMenuGroup.ToList();
                var tileUsersItems = ctx.UserMenuRight.Where(_ => _.DBId == GlobalOptions.DataBaseId
                                                                  && _.LoginName.ToUpper() ==
                                                                  GlobalOptions.UserInfo.NickName.ToUpper())
                    .ToList();
                var tileGroupsTemp = new List<TileGroup>();
                foreach (var grp in tileItems.OrderBy(_ => _.OrderBy))
                {
                    var grpOrd = tileOrders.FirstOrDefault(_ => _.IsGroup && _.TileId == grp.Id);
                    foreach (var t in grp.KursMenuItem)
                        if (tileUsersItems.Any(_ => _.MenuId == t.Id) && tileGroupsTemp.All(_ => _.Id != grp.Id))
                        {
                            var newGrp = new TileGroup
                            {
                                Id = grp.Id,
                                Name = grp.Name,
                                Notes = grp.Note,
                                Picture = ImageManager.ByteToImage(grp.Picture),
                                // ReSharper disable once PossibleInvalidOperationException
                                OrderBy = (int)(grpOrd != null ? grpOrd.Order : grp.Id)
                            };
                            tileGroupsTemp.Add(newGrp);
                        }
                }

                var tileGroups = new List<TileGroup>(tileGroupsTemp.OrderBy(_ => _.OrderBy));
                foreach (var grp in tileGroups)
                {
                    var tItems = new List<TileItem>();
                    foreach (var tile in ctx.KursMenuItem.Where(t => t.GroupId == grp.Id).ToList())
                    {
                        if (tileUsersItems.All(_ => _.MenuId != tile.Id)) continue;
                        var ord = tileOrders.FirstOrDefault(_ => !_.IsGroup && _.TileId == tile.Id);
                        tItems.Add(new TileItem
                        {
                            Id = tile.Id,
                            Name = tile.Name,
                            Notes = tile.Note,
                            Picture = ImageManager.ByteToImage(tile.Picture),
                            GroupId = tile.GroupId,
                            // ReSharper disable once PossibleInvalidOperationException
                            OrderBy = (int)(ord != null ? ord.Order : tile.Id)
                        });
                    }

                    grp.TileItems = new List<TileItem>(tItems.OrderBy(_ => _.OrderBy));
                }

                GlobalOptions.UserInfo.MainTileGroups = new List<TileGroup>(tileGroups.OrderBy(_ => _.OrderBy));
                GlobalOptions.UserInfo.Groups =
                    GlobalOptions.GetEntities().EXT_GROUPS.Select(
                            grp => new UserGroup { Id = grp.GR_ID, Name = grp.GR_NAME })
                        .ToList();
                tileMainGroup.Children.Clear();
                foreach (var tileGroup in GlobalOptions.UserInfo.MainTileGroups.OrderBy(_ => _.OrderBy))
                {
                    var newTileGroup = new Tile { Tag = tileGroup.Id };
                    if (tileGroup.Picture != null)
                    {
                        newTileGroup.Width = tileGroup.Picture.Source.Width;
                        newTileGroup.Height = tileGroup.Picture.Source.Height;
                        newTileGroup.Margin = new Thickness(0, 0, 0, 0);
                        newTileGroup.Padding = new Thickness(0, 0, 0, 0);
                        newTileGroup.Content = tileGroup.Picture;
                        var bc = new BrushConverter();
                        var brush = (Brush)bc.ConvertFrom("#FFD0D6D3");
                        if (brush != null)
                        {
                            brush.Freeze();
                            newTileGroup.Background = brush;
                            newTileGroup.Foreground = brush;
                            newTileGroup.ToolTip = tileGroup.Notes;
                        }
                    }

                    tileMainGroup.Children.Add(newTileGroup);
                }
            }
            GlobalOptions.SystemProfile.Profile.Clear();
            foreach (var p in GlobalOptions.GetEntities().PROFILE)
            {
                GlobalOptions.SystemProfile.Profile.Add(p);
            }
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var login = new StartLogin { Topmost = true };
                login.ShowDialog();

                if (!login.IsConnectSuccess)
                {
                    Close();
                }
                else
                {
#if (!DEBUG)
               //myVersionUpdateTimer = new Timer(_ => CheckUpdateVersion(), null, TimeForCheckVersion, Timeout.Infinite);

               VersionUpdateTimer = new System.Windows.Threading.DispatcherTimer();
               VersionUpdateTimer.Tick += new EventHandler(versionUpdater_Thick);
               VersionUpdateTimer.Interval = new TimeSpan(0,15,0);
            VersionUpdateTimer.Start();
#endif
                    foreach (var tileGroup in GlobalOptions.UserInfo.MainTileGroups.OrderBy(_ => _.OrderBy))
                    {
                        var newTileGroup = new Tile { Tag = tileGroup.Id };
                        if (tileGroup.Picture != null)
                        {
                            newTileGroup.Width = tileGroup.Picture.Source.Width;
                            newTileGroup.Height = tileGroup.Picture.Source.Height;
                            newTileGroup.Margin = new Thickness(0, 0, 0, 0);
                            newTileGroup.Padding = new Thickness(0, 0, 0, 0);
                            newTileGroup.Content = tileGroup.Picture;
                            var bc = new BrushConverter();
                            var brush = (Brush)bc.ConvertFrom("#FFD0D6D3");
                            if (brush != null)
                            {
                                brush.Freeze();
                                newTileGroup.Background = brush;
                                newTileGroup.Foreground = brush;
                                newTileGroup.ToolTip = tileGroup.Notes;
                            }
                        }

                        tileMainGroup.Children.Add(newTileGroup);
                    }

                    Title =
                        $"Курс АМ2. {GlobalOptions.DataBaseName} {GlobalOptions.Version} {GlobalOptions.VersionType}";
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void tileDocumentItems_TileClick(object sender, TileClickEventArgs e)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var menuItem = ctx.LastMenuUserSearch
                    .FirstOrDefault(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                         && _.DbId == GlobalOptions.DataBaseId && _.MenuId == (int)e.Tile.Tag);
                if (menuItem == null)
                    ctx.LastMenuUserSearch.Add(new LastMenuUserSearch
                    {
                        Id = Guid.NewGuid(),
                        DbId = GlobalOptions.DataBaseId,
                        UserId = GlobalOptions.UserInfo.KursId,
                        MenuId = (int)e.Tile.Tag,
                        LastOpen = DateTime.Now,
                        OpenCount = 1
                    });
                else
                    menuItem.OpenCount++;

                ctx.SaveChanges();
            }

            OpenWindow(e.Tile.Header as string);
        }

        private void BarButtonItem6_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private void BarButtonItem2_OnItemClick(object sender, ItemClickEventArgs e)
        {
            LoadTiles();
        }

        private void tileMainGroup_ItemPositionChanged(object sender, ValueChangedEventArgs<int> e)
        {
            var i = 0;
            using (var ctx = GlobalOptions.KursSystem())
            {
                foreach (var item in tileMainGroup.Children)
                {
                    var t = item as Tile;
                    if (t == null) continue;
                    var old = ctx.UserMenuOrder.FirstOrDefault(_ => _.IsGroup && _.TileId == (int)t.Tag);
                    if (old == null)
                        ctx.UserMenuOrder.Add(new UserMenuOrder
                        {
                            Id = Guid.NewGuid(),
                            IsGroup = true,
                            UserId = GlobalOptions.UserInfo.KursId,
                            TileId = (int)t.Tag,
                            Order = i
                        });
                    else
                        old.Order = i;
                    ctx.SaveChanges();
                    i++;
                }
            }

            LayoutManager.Save();
        }

        private void TileDocumentItems_OnItemPositionChanged(object sender, ValueChangedEventArgs<int> e)
        {
            var i = 0;
            using (var ctx = GlobalOptions.KursSystem())
            {
                foreach (var item in tileDocumentItems.Children)
                {
                    var t = item as Tile;
                    if (t == null) continue;
                    var menuGroup =
                        GlobalOptions.UserInfo.MainTileGroups.FirstOrDefault(_ => _.Id == currentMainGroupId);
                    var menuItem = menuGroup?.TileItems.FirstOrDefault(_ => _.Id == (int)t.Tag);
                    if (menuItem != null) menuItem.OrderBy = i;
                    var old = ctx.UserMenuOrder.FirstOrDefault(_ => !_.IsGroup && _.TileId == (int)t.Tag
                                                                               && _.UserId == GlobalOptions.UserInfo
                                                                                   .KursId);
                    if (old == null)
                        ctx.UserMenuOrder.Add(new UserMenuOrder
                        {
                            Id = Guid.NewGuid(),
                            IsGroup = false,
                            UserId = GlobalOptions.UserInfo.KursId,
                            TileId = (int)t.Tag,
                            Order = i
                        });
                    else
                        old.Order = i;
                    ctx.SaveChanges();
                    i++;
                }
            }

            LayoutManager.Save();
        }

        private void BarButtonItem5_OnItemClick(object sender, ItemClickEventArgs e)
        {
            LayoutManager.ResetLayout();
        }

        private void BarButtonItem1_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var ctxUserProfile =
                new UserOptionsWindowViewModel(TypeChangeUser.UserSelfUpdate, GlobalOptions.UserInfo.NickName);
            var formUserProfile = new UserOptionsWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctxUserProfile
            };
            ctxUserProfile.Form = formUserProfile;
            formUserProfile.Show();
        }

        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once UnusedMember.Local
        private void BarButtonItem3_OnItemClick(object sender, ItemClickEventArgs e)
        {
            // ReSharper disable once UnusedVariable **
            var e1 = e;
            SaveHistoryStart.SaveHistory();
        }

        private void SearchMenuBarItem_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var context = new MenuSearchWindowViewModel();
            var frm = new MenuSearchView
            {
                // Owner = Application.Current.MainWindow,
                DataContext = context
            };
            frm.Show();
        }

        private void LayoutPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}
