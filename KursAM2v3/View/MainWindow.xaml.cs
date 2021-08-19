using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.UI.ModuleInjection;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.LayoutControl;
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
using KursAM2.View.Reconciliation;
using KursAM2.View.Repozit;
using KursAM2.View.Shop;
using KursAM2.ViewModel.Dogovora;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Logistiks.AktSpisaniya;
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
using KursAM2.ViewModel.Reference.Nomenkl;
using KursAM2.ViewModel.Repozit;
using KursAM2.ViewModel.Shop;
using KursAM2.ViewModel.Signatures;
using KursAM2.ViewModel.StartLogin;
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
        private int currentMainGroupId;

        //private readonly string myLayoutFileName = $"{Environment.CurrentDirectory}\\Layout\\{"MainWindow"}.xml";

        private Timer myVersionUpdateTimer;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
#if (!DEBUG)
                myVersionUpdateTimer = new Timer(_ => CheckUpdateVersion(), null, 1000 * 360 * 3, Timeout.Infinite);

#endif
                LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, dockLayout1);
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

        // ReSharper disable once UnusedMember.Local
        private void CheckUpdateVersion()
        {
            myVersionUpdateTimer.Dispose();
            var ver = new VersionManager();
            ver.CheckVersion();
            myVersionUpdateTimer = new Timer(_ => CheckUpdateVersion(), null, 1000 * 60, Timeout.Infinite);
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
            LayoutManager.Save();
            foreach (var f in from Window f in Application.Current.Windows where f != null select f)
            {
                if (f is KursBaseSearchWindow s)
                    if (s.DataContext is DistributeNakladSearchViewModel ctx)
                        ctx.OnWindowClosing();

                if (f is KursBaseWindow b)
                    if (b.DataContext is DistributeNakladViewModel ctx)
                        ctx.OnWindowClosing();
                if (f is ILayout l) l.SaveLayout();
                f.Close();
            }
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        { LayoutManager.Load();
            DocumentLayoutPanel.Caption = "";
        }

        public void OpenForm(Window win, Window owner, RSWindowViewModelBase datacontext)
        {
        }

        public void OpenWindow(string formName)
        {
            try
            {
                // ReSharper disable once TooWideLocalVariableScope
                Window form;
                switch (formName)
                {
                        //Настройка подписей
                        case"Настройка подписей": 
                            form = new KursStandartFormView
                            {
                                Owner = Application.Current.MainWindow,
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
                    case "Внебалансовые начисления для клиентов":
                        var aad = new AccuredAmountForClientSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = aad
                        };
                        aad.Form = form;
                        form.Show();
                        break;

                    case "Внебалансовые начисления от поставщиков":
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
                    case "Списание материалов":
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
                        form = new DebitorCreditorView {Owner = Application.Current.MainWindow};
                        form.DataContext = dbctx;
                        dbctx.Form = form;
                        form.Show();
                        break;
                    case "Контрагенты для документов":
                        form = new KontragentRefOutView {Owner = Application.Current.MainWindow};
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
                        var ctxk = new KontragentBalansWindowViewModel();
                        form = new KontragentBalansForm {Owner = Application.Current.MainWindow, DataContext = ctxk};
                        ctxk.Form = form;
                        form.Show();
                        break;
                    case "  Рентабельность":
                        var renCtx = new BreakEvenWindowViewModel();
                        form = new BreakEvenForm
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = renCtx
                        };
                        renCtx.Form = form;
                        form.Show();
                        break;
                    case "Лицевые счета сотрудников":
                        var psCtx = new EmployeePayWindowViewModel();
                        form = new PersonalPaysView {Owner = Application.Current.MainWindow, DataContext = psCtx};
                        psCtx.Form = form;
                        psCtx.RefreshData(null);
                        form.Show();
                        break;
                    case "Справочник начислений для з/п":
                        var zpCtx = new PayrollTypeWindowViewModel();
                        form = new PayRollTypeReference {Owner = Application.Current.MainWindow, DataContext = zpCtx};
                        zpCtx.Form = form;
                        form.Show();
                        break;
                    case "Права доступа к лицевым счетам сотрудников":
                        form = new UsersRightToPayRoll {Owner = Application.Current.MainWindow};
                        form.Show();
                        break;
                    case "Ведомости начислений з/платы":
                        var vedCtx = new PayrollSearchWindowViewModel();
                        vedCtx.RefreshData(null);
                        form = new PayRollDocSearch
                        {
                            Owner = Application.Current.MainWindow
                        };
                        vedCtx.Form = form;
                        form.Show();
                        form.DataContext = vedCtx;
                        break;
                    case "Управленческий баланс":
                        var ctx1 = new ManagementBalansWindowViewModel {CurrentDate = DateTime.Today};
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
                        form = new ProfitAndLosses2
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxpb2
                        };
                        ctxpb2.Form = form;
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
                        form = new StandartSearchView {Owner = Application.Current.MainWindow};
                        var ctxsf = new InvoiceClientSearchViewModel(form);
                        form.DataContext = ctxsf;
                        form.Show();
                        break;
                    case "Счета-фактуры поставщиков":
                        form = new StandartSearchView {Owner = Application.Current.MainWindow};
                        var dtx = new SearchInvoiceProviderViewModel(form);
                        form.DataContext = dtx;
                        form.Show();
                        break;
                    case "Расходные накладные для клиентов":

                        var ctxNaklad = new WaybillSearchViewModel();
                        form = new StandartSearchView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxNaklad
                        };
                        form.Show();
                        break;
                    case "Справочник сотрудников":
                        form = new PersonaReference {Owner = Application.Current.MainWindow};
                        var ctxPers = new PersonaReferenceWindowViewModel();
                        ctxPers.Form = form;
                        form.Show();
                        form.DataContext = ctxPers;
                        break;
                    case "  Доступ к сч. контр-тов":
                        form = new KontragentRightsControl {Owner = Application.Current.MainWindow};
                        form.Show();
                        break;
                    case "Справочник контрагентов":
                        form = new KontragentReference2View {Owner = Application.Current.MainWindow};
                        var ctxRef = new KontragentReferenceWindowViewModel();
                        ctxRef.Form = form;
                        form.Show();
                        form.DataContext = ctxRef;
                        break;
                    case "Справочник складов":
                        var frm = new TreeListFormBaseView
                        {
                            LayoutManagerName = "NomenklStore",
                            Owner = Application.Current.MainWindow
                        };
                        var ctxRef1 = new Store2ReferenceWindowViewModel {Form = frm};
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
                        var ctxRef2 = new RegionRefViewModel {Form = frm1};
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
                        form = new ActOfReconciliation {Owner = Application.Current.MainWindow, DataContext = aorCtx};
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
                        ctxost.RefreshData(null);
                        form = new SkladOstatki
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxost
                        };
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
                        ((KursBaseControlViewModel) v.DataContext).Form = dnakForm;
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
                        var mutCtx2 = new MutualAccountingWindowSearchViewModel
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
                        form.DataContext = new MutualAccountingWindowSearchViewModel(true)
                        {
                            WindowName = "Поиск актов конвертации",
                            Form = form
                        };
                        form.Show();
                        break;
                    case "Проекты":
                        var pctx = new ProjectWindowViewModel();
                        form = new ProjectsView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = pctx
                        };
                        pctx.Form = form;
                        form.Show();
                        pctx.RefreshData(null);
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
                        //var ctxAccessRight = new AccessRightsViewModel();
                        //form = new AccessRightsView
                        //{
                        //    Owner = Application.Current.MainWindow,
                        //    DataContext = ctxAccessRight
                        //};
                        ctxAccessRight.Form = form2;
                        form2.Show();
                        break;

                    case "Распределение приходов":
                        var ctxProjectPrihod = new ProjectProviderPrihodWindowViewModel();
                        form = new ProjectProviderPrihodView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxProjectPrihod
                        };
                        ctxProjectPrihod.Form = form;
                        form.Show();
                        break;
                    case "Прибыли и убытки по проектам":
                        var ctxProjectProfitAndLoss = new ProjectProfitAndLossesWindowViewModel();
                        form = new ProjectProfitAndLossView
                        {
                            Owner = Application.Current.MainWindow,
                            DataContext = ctxProjectProfitAndLoss
                        };
                        ctxProjectProfitAndLoss.Form = form;
                        form.Show();
                        break;
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
                    case "Справочник типов товаров":
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
                            LayoutManagerName = "NomenklCategory",
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
            currentMainGroupId = (int) tileClickEventArgs.Tile.Tag;
            var grp = GlobalOptions.UserInfo.MainTileGroups.FirstOrDefault(
                _ => (int) tileClickEventArgs.Tile.Tag == _.Id);
            if (grp == null) return;
            foreach (var item in tileMainGroup.Children)
                if (item is Tile t)
                    t.BorderThickness = new Thickness(0, 0, 0, 0);
            tileClickEventArgs.Tile.BorderThickness = new Thickness(3, 3, 3, 3);
            tileClickEventArgs.Tile.BorderBrush = Brushes.Orange;

            foreach (var tile in grp.TileItems.OrderBy(_ => _.OrderBy))
            {
                var newTile = new Tile
                {
                    Header = tile.Name,
                    Width = 250,
                    Height = 100,
                    Tag = tile.Id
                };
                newTile.HorizontalAlignment = HorizontalAlignment.Center;
                newTile.HorizontalHeaderAlignment = HorizontalAlignment.Center;
                tileDocumentItems.Children.Add(newTile);
            }
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var login = new StartLogin {Topmost = true};
                login.ShowDialog();

                if (!login.IsConnectSuccess)
                {
                    Close();
                }
                else
                {
                    foreach (var tileGroup in GlobalOptions.UserInfo.MainTileGroups.OrderBy(_ => _.OrderBy))
                    {
                        var newTileGroup = new Tile {Tag = tileGroup.Id};
                        if (tileGroup.Picture != null)
                        {
                            newTileGroup.Width = tileGroup.Picture.Source.Width;
                            newTileGroup.Height = tileGroup.Picture.Source.Height;
                            newTileGroup.Margin = new Thickness(0, 0, 0, 0);
                            newTileGroup.Padding = new Thickness(0, 0, 0, 0);
                            newTileGroup.Content = tileGroup.Picture;
                            var bc = new BrushConverter();
                            var brush = (Brush) bc.ConvertFrom("#FFD0D6D3");
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

                    Title = $"Курс АМ2. {GlobalOptions.DataBaseName} Версия {GlobalOptions.Version}";
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void tileDocumentItems_TileClick(object sender, TileClickEventArgs e)
        {
            OpenWindow(e.Tile.Header as string);
        }

        private void BarButtonItem3_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private void BarButtonItem2_OnItemClick(object sender, ItemClickEventArgs e)
        {
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
                    var old = ctx.UserMenuOrder.FirstOrDefault(_ => _.IsGroup && _.TileId == (int) t.Tag);
                    if (old == null)
                        ctx.UserMenuOrder.Add(new UserMenuOrder
                        {
                            Id = Guid.NewGuid(),
                            IsGroup = true,
                            UserId = GlobalOptions.UserInfo.KursId,
                            TileId = (int) t.Tag,
                            Order = i
                        });
                    else
                        old.Order = i;
                    ctx.SaveChanges();
                    i++;
                }
            }
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
                    var menuItem = menuGroup?.TileItems.FirstOrDefault(_ => _.Id == (int) t.Tag);
                    if (menuItem != null) menuItem.OrderBy = i;
                    var old = ctx.UserMenuOrder.FirstOrDefault(_ => !_.IsGroup && _.TileId == (int) t.Tag);
                    if (old == null)
                        ctx.UserMenuOrder.Add(new UserMenuOrder
                        {
                            Id = Guid.NewGuid(),
                            IsGroup = false,
                            UserId = GlobalOptions.UserInfo.KursId,
                            TileId = (int) t.Tag,
                            Order = i
                        });
                    else
                        old.Order = i;
                    ctx.SaveChanges();
                    i++;
                }
            }
        }

        private void BarButtonItem4_OnItemClick(object sender, ItemClickEventArgs e)
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

        private void BarButtonItem5_OnItemClick(object sender, ItemClickEventArgs e)
        {
            SaveHistoryStart.SaveHistory();
        }
    }
}