﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core.EntityViewModel;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.Xpf;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.ReportManagers.SFClientAndWayBill;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.DialogUserControl;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using KursDomain.WindowsManager.WindowsManager;
using Newtonsoft.Json;
using Reports.Base;
using StackExchange.Redis;
using NomenklProductType = KursDomain.References.NomenklProductType;

namespace KursAM2.ViewModel.Finance.Invoices;

public sealed class ClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
{
    #region Fields

    private InvoicePaymentDocument myCurrentPaymentDoc;
    private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

    // ReSharper disable once InconsistentNaming
    private InvoiceClientRowViewModel _myCurrentRow;
    private ShipmentRowViewModel myCurrentShipmentRow;
    private InvoiceClientViewModel myDocument;
    private decimal myOtgruzheno;
    private RSWindowViewModelBase myParentForm;

    public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
        new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

    public readonly GenericKursDBRepository<SD_84> GenericClientRepository;
    public readonly IInvoiceClientRepository InvoiceClientRepository;
    private readonly List<decimal> myUsedNomenklsDC = new List<decimal>();

    // ReSharper disable once NotAccessedField.Local
    public bool IsLoadPay = true;
    private readonly ConnectionMultiplexer redis;
    private readonly ISubscriber mySubscriber;

    #endregion

    #region Constructors

    public ClientWindowViewModel()
    {
        try
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            if (mySubscriber.IsConnected())
            {
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        if (KursNotyficationService != null)
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => ShowNotify(message));
                        }
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.CashIn, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        if (KursNotyficationService != null)
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateCash(message));
                        }
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        if (KursNotyficationService != null)
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateCash(message));
                        }
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        if (KursNotyficationService != null)
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateCash(message));
                        }
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.WayBill, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        if (KursNotyficationService != null)
                        {
                            Console.WriteLine($@"Redis - {message}");
                            Form.Dispatcher.Invoke(() => UpdateWarehouse(message));
                        }
                    });
            }
        }
        catch
        {
            Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
        }


        ShipmentRowDeleted = new List<ShipmentRowViewModel>();
        GenericClientRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
        InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
        // ReSharper disable once ObjectCreationAsStatement
        new ReportManager();
        CreateReports();
        // ReSharper disable once VirtualMemberCallInConstructor
        IsDocNewCopyAllow = true;
        // ReSharper disable once VirtualMemberCallInConstructor
        IsDocNewCopyRequisiteAllow = true;
        LeftMenuBar = GlobalOptions.UserInfo.IsAdmin
            ? MenuGenerator.DocWithCreateLinkDocumentLeftBar(this)
            : MenuGenerator.DocWithRowsLeftBar(this);
        RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        WindowName = "Счет-фактура клиенту (новая)";
        CreateReportsMenu();
    }

    public ClientWindowViewModel(decimal? dc, bool isLoadPay = true) : this()
    {
        IsLoadPay = isLoadPay;
        var doc = dc != null ? GenericClientRepository.GetById(dc.Value) : null;
        if (doc == null)
        {
            doc = new SD_84
            {
                DOC_CODE = -1,
                SF_DATE = DateTime.Today,
                REGISTER_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                SF_CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                Id = Guid.NewGuid(),
                SF_CRS_SUMMA_K_OPLATE = 0,
                SF_PAY_FLAG = 0,
                SF_FACT_SUMMA = 0,
                SF_NDS_1INCLUD_0NO = 1,
                SF_KONTR_CRS_SUMMA = 0,
                SF_DILER_SUMMA = 0,
                SF_SUMMA_V_UCHET_VALUTE = 0,
                SF_RUB_SUMMA_K_OPLATE = 0
            };
            UnitOfWork.Context.SD_84.Add(doc);
            Document = new InvoiceClientViewModel(doc, UnitOfWork, isLoadPay)
            {
                State = RowStatus.NewRow
            };
        }
        else
        {
            Document = new InvoiceClientViewModel(doc, UnitOfWork, true)
            {
                State = RowStatus.NotEdited
            };
            if (Document != null)
                WindowName = Document.ToString();
            UpdateShipped();
            Document.myState = RowStatus.NotEdited;
            foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>()) r.myState = RowStatus.NotEdited;
            SetVisualOnStart();
            LastDocumentManager.SaveLastOpenInfo(DocumentType.InvoiceClient, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
        }

        if (mySubscriber != null && mySubscriber.IsConnected())
        {
            var message = new RedisMessage
            {
                DocumentType = DocumentType.InvoiceClient,
                DocCode = Document.DocCode,
                DocDate = Document.DocDate,
                OperationType = RedisMessageDocumentOperationTypeEnum.Open,
                IsDocument = true,
                Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' открыл счет {Document.Description}"
            };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
            if (Document.State != RowStatus.NewRow)
                mySubscriber.Publish(
                    new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto), json);
        }
    }

    #endregion

    #region Properties

    public List<ShipmentRowViewModel> ShipmentRowDeleted { set; get; }

    public string NotifyInfo { set; get; }


    public List<Currency> CurrencyList => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>()
        .OrderBy(_ => _.Name).ToList();

    public List<CentrResponsibility> COList => GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll()
        .Cast<CentrResponsibility>().OrderBy(_ => _.Name).ToList();

    public List<Employee> EmployeeList => GlobalOptions.ReferencesCache.GetEmployees().Cast<Employee>()
        .OrderBy(_ => _.Name).ToList();

    public List<PayForm> FormRaschets => GlobalOptions.ReferencesCache.GetPayFormAll().Cast<PayForm>()
        .OrderBy(_ => _.Name).ToList();

    public List<NomenklProductType> VzaimoraschetTypes => GlobalOptions.ReferencesCache.GetNomenklProductTypesAll()
        .Cast<NomenklProductType>()
        .OrderBy(_ => _.Name).ToList();

    public List<PayCondition> PayConditions => GlobalOptions.ReferencesCache.GetPayConditionAll()
        .Cast<PayCondition>()
        .OrderBy(_ => _.Name).ToList();

    public List<Country> Countries => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Country>()
        .OrderBy(_ => _.Name).ToList();

    public override string LayoutName => "InvoiceClientView2";

    public Visibility IsPaysEnabled => isPaysEnabled();

    private Visibility isPaysEnabled()
    {
        if (Document.PaymentDocs.Count > 0)
        {
            if (Document.PaymentDocs.Any(_ => _.DocumentType == DocumentType.CashIn) && !Document.PaymentDocs
                    .Where(_ => _.DocumentType == DocumentType.CashIn)
                    .Any(_ => GlobalOptions.UserInfo.CashAccess.Contains(_.FromDC))) return Visibility.Hidden;
            if (Document.PaymentDocs.Any(__ => __.DocumentType == DocumentType.Bank) && !Document.PaymentDocs
                    .Where(__ => __.DocumentType == DocumentType.Bank)
                    .Any(__ => GlobalOptions.UserInfo.BankAccess
                        .Contains(__.FromDC))) return Visibility.Hidden;
        }

        return Visibility.Visible;
    }

    public bool IsCurrencyEnabled => Document.Client == null;

    public RSWindowViewModelBase ParentForm
    {
        get => myParentForm;
        set
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (myParentForm == value) return;
            myParentForm = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<ShipmentRowViewModel> SelectedShipmnetRows { set; get; }
        = new ObservableCollection<ShipmentRowViewModel>();

    public ShipmentRowViewModel CurrentShipmentRow
    {
        get => myCurrentShipmentRow;
        set
        {
            if (Equals(myCurrentShipmentRow, value)) return;
            myCurrentShipmentRow = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceClientRowViewModel CurrentRow
    {
        set
        {
            if (Equals(_myCurrentRow, value)) return;
            _myCurrentRow = value;
            RaisePropertyChanged();
        }
        get => _myCurrentRow;
    }

    public decimal Otgruzheno
    {
        get => myOtgruzheno;
        set
        {
            if (myOtgruzheno == value) return;
            myOtgruzheno = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceClientViewModel Document
    {
        get => myDocument;
        set
        {
            if (Equals(myDocument, value)) return;
            myDocument = value;
            RaisePropertyChanged();
        }
    }

    public override RowStatus State => Document?.State ?? RowStatus.NotEdited;

    public override bool IsCanSaveData => Document.State != RowStatus.NotEdited
                                          && Document.Entity.SF_CLIENT_DC != null
                                          && Document.SF_RECEIVER_KONTR_DC != null
                                          && Document.Entity.SF_CRS_DC > 0 &&
                                          Document.SF_CENTR_OTV_DC != null && Document.SF_CENTR_OTV_DC != 0
                                          && Document.PayCondition != null &&
                                          Document.SF_VZAIMOR_TYPE_DC != null
                                          && Document.SF_FORM_RASCH_DC != null;


    public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

    public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow
                                                              && Document.PaymentDocs?.Count == 0 &&
                                                              Document.ShipmentRows?.Count == 0;

    public override string WindowName =>
        Document == null || Document.DocCode < 0 || Document.State == RowStatus.NewRow
            ? "Счет-фактура клиенту (новая)"
            : Document?.Name;

    public InvoicePaymentDocument CurrentPaymentDoc
    {
        get => myCurrentPaymentDoc;
        set
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (myCurrentPaymentDoc == value) return;
            myCurrentPaymentDoc = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    private void AddUsedNomenkl(decimal nomdc)
    {
        if (myUsedNomenklsDC.All(_ => _ != nomdc)) myUsedNomenklsDC.Add(nomdc);
    }

    public void UpdateVisualData(object obj)
    {
        // ReSharper disable once NotResolvedInText
        Document.SF_DILER_SUMMA = Document.Rows.Sum(_ => _.SFT_NACENKA_DILERA);
        if (Form is InvoiceClientView frm)
        {
            //frm.KontrSelectButton.IsEnabled = Document.PaymentDocs.Count == 0 && Document.ShipmentRows.Count == 0;
            if (Document.IsNDSIncludeInPrice)
            {
                var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
                if (colPrice != null) colPrice.ReadOnly = false;
                var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
                if (colSumma != null) colSumma.ReadOnly = false;
            }
            else
            {
                var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
                if (colPrice != null) colPrice.ReadOnly = false;
                var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
                if (colSumma != null) colSumma.ReadOnly = true;
            }

            frm.gridRows.RefreshData();
            frm.gridRows.UpdateTotalSummary();
            RaisePropertyChanged(nameof(Document));
        }
    }

    public override void OnWindowClosing(object obj)
    {
        mySubscriber?.UnsubscribeAll();
        base.OnWindowClosing(obj);
    }

    protected override void OnWindowLoaded(object obj)
    {
        base.OnWindowLoaded(obj);
        UpdateVisualObjects();
    }

    public override void UpdateVisualObjects()
    {
        base.UpdateVisualObjects();
        if (Form is InvoiceClientView frm)
        {
            var delList = new List<GridSummaryItem>();
            foreach (var s in frm.gridRows.TotalSummary)
                if (s.FieldName is nameof(IInvoiceClientRow.Price) or nameof(IInvoiceClientRow.PriceWithNDS)
                    or nameof(IInvoiceClientRow.NDSPercent))
                    delList.Add(s);

            foreach (var s in delList) frm.gridRows.TotalSummary.Remove(s);
            frm.tableViewRows.FormatConditions.Clear();
            var notShippedFormatCondition = new FormatCondition
            {
                FieldName = "Shipped",
                ApplyToRow = true,
                Format = new Format
                {
                    Foreground = Brushes.Red
                },
                ValueRule = ConditionRule.Equal,
                Value1 = 0m
            };
            var serviceFormatCondition = new FormatCondition
            {
                FieldName = "IsUsluga",
                ApplyToRow = true,
                Format = new Format
                {
                    Foreground = Brushes.Black
                },
                ValueRule = ConditionRule.Equal,
                Value1 = true
            };

            var shippedFormatCondition = new FormatCondition
            {
                Expression = "[Quantity] > [Shipped]",
                FieldName = "Shipped",
                ApplyToRow = true,
                Format = new Format
                {
                    Foreground = Brushes.Blue
                }
            };
            frm.tableViewRows.FormatConditions.Add(shippedFormatCondition);
            frm.tableViewRows.FormatConditions.Add(notShippedFormatCondition);
            frm.tableViewRows.FormatConditions.Add(serviceFormatCondition);
            RaisePropertyChanged(nameof(IsPaysEnabled));
        }
    }

    private void SetVisualOnStart()
    {
        if (Form is InvoiceClientView frm)
        {
            if (Document.IsNDSIncludeInPrice)
            {
                var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
                if (colPrice != null) colPrice.ReadOnly = true;
                var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
                if (colSumma != null) colSumma.ReadOnly = false;
            }
            else
            {
                var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Price");
                if (colPrice != null) colPrice.ReadOnly = false;
                var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
                if (colSumma != null) colSumma.ReadOnly = true;
            }
        }
    }

    public void GetDefaultValue()
    {
        Document.State = RowStatus.NotEdited;
    }

    private void CreateReportsMenu()
    {
        var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
        if (prn == null) return;

        #region Заказ

        var zakPrint = new MenuButtonInfo
        {
            Caption = "Заказ"
        };
        zakPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Заказ",
            Command = PrintZakazCommand
        });
        zakPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Заказ без менеджера",
            Command = PrintZakazWOManagerCommand
        });

        #endregion

        #region Заявка на отгрузку со склада

        var zajavkaSkladPrint = new MenuButtonInfo
        {
            Caption = "Отгрузка со склада"
        };
        zajavkaSkladPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Заявка на отгрузку",
            Command = PrintZajavkaCommand
        });
        zajavkaSkladPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Экспорт",
            Command = PrintZajavkaExportCommand
        });

        #endregion

        prn.SubMenu.Add(zakPrint);
        prn.SubMenu.Add(zajavkaSkladPrint);
        prn.SubMenu.Add(new MenuButtonInfo
        {
            IsSeparator = true
        });

        #region Счет

        var schetPrint = new MenuButtonInfo
        {
            Caption = "Счет"
        };
        schetPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Печать",
            Command = PrintSFSchetCommand
        });
        schetPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Экспорт",
            Command = PrintSFSchetExportCommand
        });
        prn.SubMenu.Add(schetPrint);

        #endregion

        #region Счет-фактура

        var schetFPrint = new MenuButtonInfo
        {
            Caption = "Счет-фактура"
        };
        schetFPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Печать",
            Command = PrintSFCommand
        });
        schetFPrint.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Экспорт",
            Command = ExportSFCommand
        });
        prn.SubMenu.Add(schetFPrint);

        #endregion


        prn.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Товарная накладная",
            Command = PrintWaybillCommand
        });
        prn.SubMenu.Add(new MenuButtonInfo
        {
            Caption = "Товарная накладная - экспорт",
            Command = PrintWaybillExportCommand
        });
    }

    private void CreateReports()
    {
        ReportManager.Reports.Add("Экспорт", new SFClientSchetFacturaReportNew(this)
        {
            PrintOptions = new KursReportA4PrintOptions(),
            ShowType = ReportShowType.Spreadsheet,
            XlsFileName = "UPD"
        });
        ReportManager.Reports.Add("Счет", new SFClientSFSсhetNew(this)
        {
            PrintOptions = new KursReportA4PrintOptions(),
            ShowType = ReportShowType.Report,
            XlsFileName = "AccountNew"
        });
        ReportManager.Reports.Add("Счет-фактура", new SFClientSchetFacturaReportNew(this)
        {
            PrintOptions = new KursReportLandscapeA4PrintOptions(),
            ShowType = ReportShowType.Report,
            XlsFileName = "UPD"
        });
        ReportManager.Reports.Add("Заказ", new SFClientZakazReport(this)
        {
            PrintOptions = new KursReportLandscapeA4PrintOptions(),
            ShowType = ReportShowType.Report,
            XlsFileName = "Zakaz"
        });
        ReportManager.Reports.Add("Заказ без менеджера", new SFClientZakazReport(this)
        {
            IsManagerPrint = false,
            PrintOptions = new KursReportLandscapeA4PrintOptions(),
            ShowType = ReportShowType.Report,
            XlsFileName = "Zakaz"
        });
        ReportManager.Reports.Add("Заявка", new SFClientZajavkaSkladReport(this)
        {
            PrintOptions = new KursReportLandscapeA4PrintOptions(),
            ShowType = ReportShowType.Report,
            XlsFileName = "Zajavka"
        });
        ReportManager.Reports.Add("Заявка экспорт", new SFClientZajavkaSkladReport(this)
        {
            IsManagerPrint = false,
            PrintOptions = new KursReportLandscapeA4PrintOptions(),
            ShowType = ReportShowType.Spreadsheet,
            XlsFileName = "Zajavka"
        });
        ReportManager.Reports.Add("Торг12", new SFClientTorg12Report(this)
        {
            PrintOptions = new KursReportLandscapeA4PrintOptions
            {
                FitToWidth = 1
            },
            ShowType = ReportShowType.Report,
            XlsFileName = "torg12"
        });
        ReportManager.Reports.Add("Торг12Экспорт", new SFClientTorg12Report(this)
        {
            PrintOptions = new KursReportLandscapeA4PrintOptions
            {
                FitToWidth = 1
            },
            ShowType = ReportShowType.Spreadsheet,
            XlsFileName = "torg12"
        });
    }

    private void UpdateShipped()
    {
        foreach (var row in Document.Rows)
            row.Shipped = Document.ShipmentRows.Where(_ => _.Nomenkl.DocCode == row.Nomenkl.DocCode)
                .Sum(_ => _.DDT_KOL_RASHOD);
    }

    private void GenerateNaklad(WaybillWindowViewModel2 vm)
    {
        vm.Document.Client = Document.Client;
        vm.Document.InvoiceClientViewModel = InvoicesManager.GetInvoiceClient(Document.DocCode);
        var newCode = 1;
        foreach (var item in Document.Rows.Where(_ => !_.IsUsluga && _.Quantity > _.Shipped))
        {
            if (GlobalOptions.ReferencesCache.GetNomenkl(item.Nomenkl.DocCode) is Nomenkl n &&
                vm.Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
            {
                var newItem = new WaybillRow
                {
                    DocCode = vm.Document.DocCode,
                    Code = newCode,
                    Id = Guid.NewGuid(),
                    DocId = vm.Document.Id,
                    DDT_SFACT_DC = item.DocCode,
                    DDT_SFACT_ROW_CODE = item.Code,
                    Nomenkl = n,
                    DDT_KOL_RASHOD = item.Quantity - item.Shipped,
                    Unit = n.Unit as Unit,
                    Currency = n.Currency as Currency,
                    InvoiceClientViewModel = vm.Document.InvoiceClientViewModel,
                    State = RowStatus.NewRow,
                    SchetLinkedRowViewModel =
                        vm.Document.InvoiceClientViewModel.Rows.FirstOrDefault(_ => _.Code == item.Code) as
                            InvoiceClientRowViewModel
                };
                vm.Document.Rows.Add(newItem);
                vm.Document.Entity.TD_24.Add(newItem.Entity);
            }

            newCode++;
        }
    }

    #endregion

    #region Command

    public override bool CanCreateLinkDocument => Document.State == RowStatus.NotEdited &&
                                                  Document.Summa - Document.Rows.Where(_ => _.IsUsluga)
                                                      .Sum(s => s.Summa)
                                                  > Document.SummaOtgruz;

    public override void CreateLinkDocument(object obj)
    {
        var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
        var ctx = new WaybillWindowViewModel2(null) { Form = frm };
        ctx.Document.Client = Document.Client;
        GenerateNaklad(ctx);
        frm.DataContext = ctx;
        frm.Show();
    }

    public override void ShowHistory(object data)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        DocumentHistoryManager.LoadHistory(DocumentType.InvoiceClient, null, Document.DocCode, null);
    }

    public ICommand AddStoreLinkCommand
    {
        get { return new Command(AddStoreLink, _ => true); }
    }

    private void AddStoreLink(object obj)
    {
        var winManager = new WindowManager();
        winManager.ShowWinUIMessageBox("Функция не реализована", "Сообщение",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public ICommand RestColumnDataCommand
    {
        get { return new Command(RestColumnData, _ => true); }
    }


    private void RestColumnData(object obj)
    {
        if (obj is not UnboundColumnRowArgs args) return;
        if (!args.IsGetData) return;
        var item = (InvoiceClientRowViewModel)args.Item;
        args.Value = item.IsUsluga ? item.Quantity : item.Quantity - item.Shipped;
    }


    public ICommand UpdateCalcRowSummaCommand
    {
        get { return new Command(UpdateVisualData, _ => true); }
    }

    public ICommand ClientSelectCommand
    {
        get { return new Command(ClientSelect, _ => true); }
    }

    private void ClientSelect(object obj)
    {
        if (Document == null)
            return;
        if (Document.ShipmentRows.Count > 0)
        {
            WindowManager.ShowMessage("По счету есть расходные накладные. Изменить контрагента нельзя.",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        if (Document.PaySumma != 0)
        {
            WindowManager.ShowMessage("По счету есть Оплата. Изменить контрагента нельзя.",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        var kontr = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
            { Currency = Document.Currency });
        if (kontr == null) return;
        if (Document.Rows.Any(_ =>
                !_.IsUsluga && ((IDocCode)_.Nomenkl.Currency).DocCode != ((IDocCode)kontr.Currency).DocCode))
        {
            WindowManager.ShowMessage(
                "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        Document.Client = kontr;
        Document.Currency = kontr.Currency as Currency;
        Document.Entity.SF_KONTR_CRS_RATE = 1;
    }

    public ICommand AddNomenklSimpleCommand
    {
        get { return new Command(AddNomenklSimple, _ => Document?.Currency != null); }
    }

    private IEnumerable<NomenklInfo> LoadNomenkl(string srchText, decimal? crsDC)
    {
        var ret = new List<NomenklInfo>();
        var sql =
            @"SELECT np.NOM_DC AS DocCode, sum(np.KOL_IN)- sum(np.KOL_OUT) AS Quantity, max(np.DATE) as LastOperation
                                FROM NOM_PRICE np
                                GROUP BY np.NOM_DC
                                HAVING sum(np.KOL_IN) - sum(np.KOL_OUT) > 0";
        using (var ctx = GlobalOptions.GetEntities())
        {
            var noms = GlobalOptions.ReferencesCache.GetNomenklsAll()
                .Where(_ => ((IDocCode)_.Currency).DocCode == Document.Currency.DocCode).Cast<Nomenkl>().Where(_ =>
                    (_.Name + _.NomenklNumber + _.FullName).ToUpper().Contains(srchText.ToUpper()))
                .OrderBy(_ => _.Name).ToList();

            var quans = ctx.Database.SqlQuery<NomenklQuantityInfo>(sql).ToList();
            foreach (var n in noms)
            {
                var item = new NomenklInfo(n);
                var q = quans.FirstOrDefault(_ => _.DocCode == n.DocCode);
                if (q != null)
                {
                    item.QuantityOnStores = q.Quantity;
                    // ReSharper disable once PossibleInvalidOperationException
                    item.LastOperationDate = (DateTime)q.LastOperation;
                }

                ret.Add(item);
            }

            return crsDC is null
                ? ret.OrderByDescending(_ => _.QuantityOnStores).ThenBy(_ => _.Name)
                : ret.Where(_ => ((IDocCode)_.Currency).DocCode == crsDC.Value)
                    .OrderByDescending(_ => _.QuantityOnStores).ThenBy(_ => _.Name);
        }
    }

    private void AddNomenklSimple(object obj)
    {
        var dtx = new TableSearchWindowViewMove<NomenklInfo>(LoadNomenkl, "Выбор номенклатур",
            "NomenklSipmleListView", Document.Currency.DocCode);
        var service = this.GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Выбор номенклатур", dtx) == MessageResult.OK
            || dtx.DialogResult)
        {
            var newCode = UnitOfWork.Context.TD_84.Any() ?  UnitOfWork.Context.TD_84.Max(_ => _.CODE) + 1 : 1;
            foreach (var item in dtx.SelectedItems)
            {
                if (Document != null && Document.Rows.Cast<InvoiceClientRowViewModel>()
                        .Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                decimal nds;
                if (item.DefaultNDSPercent == null)
                    nds = 0;
                else
                    nds = (decimal)item.DefaultNDSPercent;
                // ReSharper disable once UseObjectOrCollectionInitializer
                var r = new InvoiceClientRowViewModel
                {
                    // ReSharper disable once PossibleNullReferenceException
                    DocCode = Document.DocCode,
                    Code = newCode,
                    Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.DocCode) as Nomenkl,
                    Parent = Document,
                    NDSPercent = nds,
                    Quantity = 1,
                    Price = 0,
                    IsNDSInPrice = Document.IsNDSIncludeInPrice,
                    Note = "",
                    Id = Guid.NewGuid(),
                    DocId = Document.Id
                };
                r.Entity.SFT_NEMENKL_DC = item.DocCode;
                Document?.Rows.Add(r);
                if (Document != null)
                    Document.Entity.TD_84.Add(r.Entity);
                newCode++;
            }
        }

        UpdateVisualData(null);
    }

    public ICommand DilerSelectCommand
    {
        get { return new Command(DilerSelect, _ => true); }
    }

    private void DilerSelect(object obj)
    {
        if (Document == null)
            return;
        var kontr = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
            { Currency = Document.Currency });
        if (kontr == null) return;
        if (Document.Rows.Any(_ =>
                !_.IsUsluga && ((IDocCode)_.Nomenkl.Currency).DocCode != ((IDocCode)kontr.Currency).DocCode))
        {
            WindowManager.ShowMessage(
                "По счету есть товары с валютой, отличной от валюты дилера. Изменить контрагента нельзя.",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        Document.Diler = kontr;
        Document.SF_DILER_CRS_DC = ((IDocCode)kontr.Currency).DocCode;
        Document.SF_DILER_SUMMA = 0;
        Document.SF_DILER_RATE = 1;
        if (Form is InvoiceClientView frm)
        {
            var colDiler = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_NACENKA_DILERA");
            if (colDiler != null)
                colDiler.ReadOnly = false;
        }
    }

    public ICommand ReceiverSelectCommand
    {
        get { return new Command(ReceiverSelect, _ => true); }
    }

    private void ReceiverSelect(object obj)
    {
        if (Document == null)
            return;
        var kontr = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
            { Currency = Document.Currency });
        if (kontr == null) return;
        if (Document.Rows.Any(_ =>
                !_.IsUsluga && ((IDocCode)_.Nomenkl.Currency).DocCode != ((IDocCode)kontr.Currency).DocCode))
        {
            WindowManager.ShowMessage(
                "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
                "Предупреждение", MessageBoxImage.Information);
            return;
        }

        Document.Receiver = kontr;
    }

    private void ShowNotify(string notify)
    {
        if (string.IsNullOrWhiteSpace(notify)) return;
        var msg = JsonConvert.DeserializeObject<RedisMessage>(notify);
        if (msg == null || msg.UserId == GlobalOptions.UserInfo.KursId) return;
        if (msg.DocCode == Document.DocCode)
        {
            NotifyInfo = msg.Message;
            var notification = KursNotyficationService.CreateCustomNotification(this);
            notification.ShowAsync();
        }
    }

    private void UpdateWarehouse(RedisValue message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
        if (msg == null || msg.DbId != GlobalOptions.DataBaseId) return;
        Document.LoadShipments();
        NotifyInfo = msg.Message;
        var notification = KursNotyficationService.CreateCustomNotification(this);
        notification.ShowAsync();
        RaisePropertyChanged(nameof(Document));
    }

    private void UpdateCash(RedisValue message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
        if (msg == null || msg.DbId != GlobalOptions.DataBaseId) return;
        Document.LoadPayments();
        NotifyInfo = msg.Message;
        var notification = KursNotyficationService.CreateCustomNotification(this);
        notification.ShowAsync();
        RaisePropertyChanged(nameof(Document));
    }

    public override void RefreshData(object obj)
    {
        base.RefreshData(obj);
        myUsedNomenklsDC.Clear();
        if (IsCanSaveData)
        {
            var winManager = new WindowManager();
            var res = winManager.ShowWinUIMessageBox("В документ внесены изменения, сохранить?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    SaveData(null);
                    Document.DeletedRows.Clear();
                    return;
                case MessageBoxResult.No:
                    foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                    Document.LoadReferences();
                    //LoadFromExternal();

                    foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>())
                    {
                        r.myState = RowStatus.NotEdited;
                        AddUsedNomenkl(r.Nomenkl.DocCode);
                    }

                    RaiseAll();
                    Document.myState = RowStatus.NotEdited;
                    Document.RaisePropertyChanged("State");
                    foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>())
                        r.myState = RowStatus.NotEdited;
                    Document.DeletedRows.Clear();
                    return;
            }
        }

        if (Document.DocCode > 0 && Document.State != RowStatus.NewRow)
        {
            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            Document.LoadReferences();
            //LoadFromExternal();

            foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>())
            {
                r.myState = RowStatus.NotEdited;
                AddUsedNomenkl(r.Nomenkl.DocCode);
            }

            UpdateShipped();
            RaiseAll();
            Document.DeletedRows.Clear();
            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
        }

        RaisePropertyChanged(nameof(IsPaysEnabled));
        UpdateVisualObjects();
    }

    private void RaiseAll()
    {
        foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>()) r.RaisePropertyAllChanged();
        foreach (var s in Document.ShipmentRows) s.RaisePropertyAllChanged();
        foreach (var pay in Document.PaymentDocs) pay.RaisePropertyAllChanged();
        Document.RaisePropertyAllChanged();
    }

    public override void SaveData(object data)
    {
        var WinManager = new WindowManager();
        var closePeriod = UnitOfWork.Context.PERIOD_CLOSED
            .SingleOrDefault(_ => _.CLOSED_DOC_TYPE.ID.ToString() == "b57d269e-e17f-4dc2-86da-821db51bcc9e");
        if (closePeriod != null && Document.DocDate < closePeriod.DateClosed)
        {
            WinManager.ShowWinUIMessageBox(
                $"Документ находится в закрытом периоде.Дата закрытия {closePeriod.DateClosed.ToShortDateString()}"
                , "Ограничение",
                MessageBoxButton.OK,
                MessageBoxImage.Stop);
            return;
        }

        if (!Document.IsAccepted)
        {
            var res = WinManager.ShowWinUIMessageBox("Счет не акцептован, акцептовать?", "Предупреждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes) Document.IsAccepted = true;
        }

        bool isOldExist;
        var isCreateNum = true;
        using (var ctx = GlobalOptions.GetEntities())
        {
            isOldExist = ctx.SD_84.Any(_ => _.DOC_CODE == Document.DocCode);
        }

        if (!isOldExist && Document.State != RowStatus.NewRow)
        {
            var res = WinManager.ShowWinUIMessageBox("Документ уже удален! Сохранить заново?", "Предупреждение",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.No:
                    Form?.Close();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }

            Document.State = RowStatus.NewRow;
            UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Added;
            foreach (var r in Document.Entity.TD_84) UnitOfWork.Context.Entry(r).State = EntityState.Added;
            isCreateNum = false;
        }

        UnitOfWork.CreateTransaction();
        // ReSharper disable once CollectionNeverUpdated.Local
        try
        {
            if (Document.State == RowStatus.NewRow || Document.DocCode < 0)
                if (isCreateNum)
                {
                    Document.InnerNumber = UnitOfWork.Context.SD_84.Any()
                        ? UnitOfWork.Context.SD_84.Max(_ => _.SF_IN_NUM) + 1
                        : 1;
                    Document.DocCode = UnitOfWork.Context.SD_84.Any()
                        ? UnitOfWork.Context.SD_84.Max(_ => _.DOC_CODE) + 1
                        : 10840000001;
                    foreach (var row in Document.Rows) row.DocCode = Document.DocCode;
                }

            if (Document.SF_CRS_RATE == 0) Document.SF_CRS_RATE = 1;

            if (Document.Entity.SF_KONTR_CRS_RATE == null) Document.Entity.SF_KONTR_CRS_RATE = 1;

            if (Document.SF_UCHET_VALUTA_RATE == null) Document.SF_UCHET_VALUTA_RATE = 1;

            if (Document.SF_KONTR_CRS_DC == null)
                Document.SF_KONTR_CRS_DC = ((IDocCode)Document.Client.Currency).DocCode;

            if (Document.Entity.SF_KONTR_CRS_SUMMA == null) Document.Entity.SF_KONTR_CRS_SUMMA = Document.Summa;
            foreach (var row in Document.Rows.Cast<InvoiceClientRowViewModel>())
                if (row.Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS == null)
                    row.Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = row.Summa;
            foreach (var nakl in ShipmentRowDeleted)
            foreach (var old in UnitOfWork.Context.TD_24.Where(_ => _.DDT_SFACT_DC == nakl.DDT_SFACT_DC
                                                                    && _.DDT_SFACT_ROW_CODE ==
                                                                    nakl.DDT_SFACT_ROW_CODE))
            {
                old.DDT_SFACT_DC = null;
                old.DDT_SFACT_ROW_CODE = null;
            }

            foreach (var r in Document.Rows)
                if (string.IsNullOrEmpty(r.Note))
                    r.Note = " ";

            var modifiedObjects = UnitOfWork.Context.ChangeTracker.Entries();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceClient), null,
                Document.DocCode, null, (string)Document.ToJson());
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var str = Document.State == RowStatus.NewRow ? "создал" : "сохранил";
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.InvoiceClient,
                    DocCode = Document.DocCode,
                    DocDate = Document.DocDate,
                    IsDocument = true,
                    OperationType = Document.myState == RowStatus.NewRow
                        ? RedisMessageDocumentOperationTypeEnum.Create
                        : RedisMessageDocumentOperationTypeEnum.Update,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} счет {Document.Description}"
                };
                message.ExternalValues.Add("KontragentDC", Document.Client.DocCode);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(
                    new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto), json);
            }

            RecalcKontragentBalans.CalcBalans(Document.Client.DocCode, Document.DocDate);
            nomenklManager.RecalcPrice(myUsedNomenklsDC);
            foreach (var ndc in Document.Rows.Select(_ => _.Nomenkl.DocCode)) AddUsedNomenkl(ndc);
            nomenklManager.RecalcPrice(myUsedNomenklsDC);
            myUsedNomenklsDC.Clear();
            //foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            //RaiseAll();
            Document.myState = RowStatus.NotEdited;
            foreach (var r in Document.Rows.Cast<InvoiceClientRowViewModel>()) r.myState = RowStatus.NotEdited;

            ShipmentRowDeleted.Clear();
            foreach (var p in Document.PaymentDocs) p.myState = RowStatus.NotEdited;
            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
            LastDocumentManager.SaveLastOpenInfo(DocumentType.InvoiceClient, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            RaisePropertyChanged(nameof(WindowName));
        }
        catch (Exception ex)
        {
            UnitOfWork.Rollback();
            WindowManager.ShowError(ex);
        }
    }

    public ICommand OpenStoreLinkDocumentCommand
    {
        get { return new Command(OpenStoreLinkDocument, _ => CurrentShipmentRow != null); }
    }

    private void OpenStoreLinkDocument(object obj)
    {
        DocumentsOpenManager.Open(DocumentType.Waybill, CurrentShipmentRow.DOC_CODE);
    }

    public ICommand PayDocumentRemoveCommand
    {
        get { return new Command(PayDocumentRemove, _ => CurrentPaymentDoc != null); }
    }

    private void PayDocumentRemove(object obj)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            try
            {
                switch (CurrentPaymentDoc.DocumentType)
                {
                    case DocumentType.CashIn:
                        var ord = ctx.SD_33.FirstOrDefault(_ => _.DOC_CODE == CurrentPaymentDoc.DocCode);
                        if (ord == null) return;
                        ord.SFACT_DC = null;
                        ord.SFACT_CRS_DC = null;
                        ord.SFACT_CRS_RATE = 0;
                        break;
                    case DocumentType.Bank:
                        var b = ctx.TD_101.FirstOrDefault(_ => _.DOC_CODE == CurrentPaymentDoc.DocCode &&
                                                               _.CODE == CurrentPaymentDoc.Code);
                        if (b == null) return;
                        b.VVT_SFACT_CLIENT_DC = null;
                        break;
                    case DocumentType.MutualAccounting:
                        var m = ctx.TD_110.FirstOrDefault(_ => _.DOC_CODE == CurrentPaymentDoc.DocCode
                                                               && _.CODE == CurrentPaymentDoc.Code);
                        if (m == null) return;
                        ctx.TD_110.Remove(m);
                        break;
                }

                ctx.Database.ExecuteSqlCommand(
                    $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.DocCode)}");

                ctx.SaveChanges();
                Document.PaymentDocs.Remove(CurrentPaymentDoc);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }

    public ICommand DeleteRowCommand
    {
        get
        {
            return new Command(DeleteRow,
                _ => CurrentRow != null && Document != null
                                        && Document.ShipmentRows.All(x =>
                                            x.Nomenkl.DocCode != CurrentRow.Nomenkl.DocCode));
        }
    }

    public ICommand DeleteStoreLinkCommand
    {
        get { return new Command(DeleteStoreLink, _ => CurrentShipmentRow != null); }
    }

    private void DeleteStoreLink(object obj)
    {
        if (CurrentShipmentRow != null)
        {
            ShipmentRowDeleted.Add(CurrentShipmentRow);
            Document.ShipmentRows.Remove(CurrentShipmentRow);
            var t = Document.Entity.TD_84.Single(_ => _.CODE == CurrentShipmentRow.Code).TD_24;
            t.Remove(t.First());
            UpdateShipped();
            //Document.RaisePropertyAllChanged();
            RaisePropertyChanged(nameof(Document));
            if (Document.myState != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }
    }

    private void DeleteRow(object obj)
    {
        var modif = UnitOfWork.Context.ChangeTracker.Entries();
        if (CurrentRow == null) return;
        if (CurrentRow.State != RowStatus.NewRow)
        {
            Document.DeletedRows.Add(CurrentRow);
            if (Document.myState != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        var d = UnitOfWork.Context.TD_84.FirstOrDefault(_ => _.CODE == CurrentRow.Code);
        if (d is not null)
        {
            UnitOfWork.Context.TD_84.Remove(d); 
            UnitOfWork.Context.TD_84.Remove(CurrentRow.Entity);
            Document.Rows.Remove(CurrentRow);
        }
        UpdateVisualData(null);
    }

    public override void DocDelete(object obj)
    {
        var WinManager = new WindowManager();
        if (Document == null) return;
        var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        switch (res)
        {
            case MessageBoxResult.Yes:
                var dc = Document.DocCode;
                var dilerdc = Document.Diler?.DocCode;
                var docdate = Document.DocDate;
                if (Document.State == RowStatus.NewRow)
                {
                    Form.Close();
                    return;
                }

                try
                {
                    UnitOfWork.CreateTransaction();

                    foreach (var cash in Document.Entity.SD_33)
                    {
                        cash.SFACT_DC = null;
                        cash.SFACT_CRS_DC = null;
                        cash.SFACT_CRS_RATE = null;
                    }

                    foreach (var bank in Document.Entity.TD_101) bank.VVT_SFACT_CLIENT_DC = null;

                    foreach (var zach in Document.Entity.TD_110) zach.VZT_SFACT_DC = null;

                    foreach (var nakl in Document.Entity.SD_24) nakl.DD_SFACT_DC = null;

                    GenericClientRepository.Delete(Document.Entity);
                    UnitOfWork.Save();
                    UnitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    UnitOfWork.Rollback();
                    WindowManager.ShowError(ex);
                }

                if (mySubscriber != null && mySubscriber.IsConnected())
                {
                    var message = new RedisMessage
                    {
                        DocumentType = DocumentType.InvoiceClient,
                        DocCode = Document.DocCode,
                        DocDate = Document.DocDate,
                        IsDocument = true,
                        OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                        Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил счет {Document.Description}"
                    };
                    message.ExternalValues.Add("KontragentDC", Document.Client.DocCode);
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                    if (Document.State != RowStatus.NewRow)
                        mySubscriber.Publish(
                            new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto),
                            json);
                }

                // ReSharper disable once PossibleInvalidOperationException
                RecalcKontragentBalans.CalcBalans(dc, docdate);
                if (dilerdc != null)
                    RecalcKontragentBalans.CalcBalans(dilerdc.Value, docdate);
                Form.Close();
                return;
            case MessageBoxResult.No:
                Form.Close();
                return;
            case MessageBoxResult.Cancel:
                return;
        }
    }

    public override void DocNewEmpty(object form)
    {
        var frm = new InvoiceClientView { Owner = Application.Current.MainWindow };
        var ctx = new ClientWindowViewModel { Form = frm };
        ctx.Document = InvoicesManager.NewClient();
        frm.Show();
        frm.DataContext = ctx;
    }

    public override bool IsDocNewCopyAllow => Document != null && Document.State != RowStatus.NewRow;
    public override bool IsDocNewCopyRequisiteAllow => Document != null && Document.State != RowStatus.NewRow;

    public override void DocNewCopyRequisite(object obj)
    {
        if (Document == null) return;
        var ctx = new ClientWindowViewModel
        {
            IsLoadPay = false,
        };
        ctx.Document = ctx.InvoiceClientRepository.GetFullCopy(Document.DocCode);
        ctx.Document.Rows.Clear();
        ctx.Document.Entity.TD_84.Clear();
        ctx.UnitOfWork.Context.SD_84.Add(ctx.Document.Entity);
        foreach (var ent in  ctx.UnitOfWork.Context.ChangeTracker.Entries())
        {
            if (ent.Entity is SD_84 or TD_84) continue;
            ent.State = EntityState.Unchanged;
        }
        var frm = new InvoiceClientView
        {
            Owner = Application.Current.MainWindow,
            DataContext = ctx
        };
        ctx.Form = frm;
        frm.Show();
    }

    public override void DocNewCopy(object obj)
    {
        if (Document == null) return;
        var ctx = new ClientWindowViewModel
        {
            IsLoadPay = false,
        };
        ctx.Document = ctx.InvoiceClientRepository.GetFullCopy(Document.DocCode);
        ctx.UnitOfWork.Context.SD_84.Add(ctx.Document.Entity);
        foreach (var ent in  ctx.UnitOfWork.Context.ChangeTracker.Entries())
        {
            if (ent.Entity is SD_84 or TD_84) continue;
            ent.State = EntityState.Unchanged;
        }
        var frm = new InvoiceClientView
        {
            Owner = Application.Current.MainWindow,
            DataContext = ctx
        };
        ctx.Form = frm;
        frm.Show();
    }

    public ICommand AddUslugaCommand
    {
        get { return new Command(AddUsluga, _ => Document?.Currency != null); }
    }

    private void AddUsluga(object obj)
    {
        var k = StandartDialogs.SelectNomenkls(Document.Currency);
        if (k != null)
        {
            var newCode = Document?.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var item in k)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (Document.Rows.Cast<InvoiceClientRowViewModel>()
                    .Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                decimal nds;
                if (item.DefaultNDSPercent == null)
                    nds = 0;
                else
                    nds = (decimal)item.DefaultNDSPercent;
                // ReSharper disable once UseObjectOrCollectionInitializer
                var r = new InvoiceClientRowViewModel
                {
                    DocCode = Document.DocCode,
                    Nomenkl = item,
                    Code = newCode,
                    NDSPercent = nds,
                    Quantity = 1,
                    Price = 0,
                    Parent = Document,
                    IsNDSInPrice = Document.IsNDSIncludeInPrice,
                    Note = "",
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Shipped = 1
                };
                r.Entity.SFT_NEMENKL_DC = item.DocCode;
                Document.Rows.Add(r);
                Document.Entity.TD_84.Add(r.Entity);
                newCode++;
            }
        }

        UpdateVisualData(null);
    }

    public ICommand OpenPayDocumentCommand
    {
        get { return new Command(OpenPayDocument, _ => CurrentPaymentDoc != null); }
    }

    private void OpenPayDocument(object obj)
    {
        switch (CurrentPaymentDoc.DocumentType)
        {
            case DocumentType.CashIn:
                DocumentsOpenManager.Open(DocumentType.CashIn, CurrentPaymentDoc.DocCode);
                break;
            case DocumentType.Bank:
                DocumentsOpenManager.Open(DocumentType.Bank, CurrentPaymentDoc.Code);
                break;
            case DocumentType.MutualAccounting:
                DocumentsOpenManager.Open(DocumentType.MutualAccounting, CurrentPaymentDoc.DocCode);
                break;
        }
    }

    public ICommand AddNomenklCommand
    {
        get { return new Command(AddNomenkl, _ => Document?.Currency != null); }
    }

    private void AddNomenkl(object obj)
    {
        var k = StandartDialogs.SelectNomenkls(Document?.Currency, true);
        if (k != null)
        {
            var newCode = UnitOfWork.Context.TD_84.Any() ?  UnitOfWork.Context.TD_84.Max(_ => _.CODE) + 1 : 1;
            foreach (var item in k)
            {
                if (Document != null && Document.Rows.Cast<InvoiceClientRowViewModel>()
                        .Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                decimal nds;
                if (item.DefaultNDSPercent == null)
                    nds = 0;
                else
                    nds = (decimal)item.DefaultNDSPercent;
                // ReSharper disable once UseObjectOrCollectionInitializer
                var r = new InvoiceClientRowViewModel
                {
                    // ReSharper disable once PossibleNullReferenceException
                    DocCode = Document.DocCode,
                    Code = newCode,
                    Nomenkl = item,
                    Parent = Document,
                    NDSPercent = nds,
                    Quantity = 1,
                    Price = 0,
                    IsNDSInPrice = Document.IsNDSIncludeInPrice,
                    Note = "",
                    Id = Guid.NewGuid(),
                    DocId = Document.Id
                };
                //r.Entity.SFT_NEMENKL_DC = item.DocCode;
                Document?.Rows.Add(r);
                if (Document != null)
                    Document.Entity.TD_84.Add(r.Entity);
                newCode++;
            }
        }

        UpdateVisualData(null);
    }

    public override bool IsRedoAllow
    {
        get => Document?.DeletedRows != null && Document?.DeletedRows.Count > 0;
        set => base.IsRedoAllow = value;
    }

    public override bool IsPrintAllow => Document.State != RowStatus.NewRow;

    public ICommand PrintZajavkaCommand
    {
        get { return new Command(PrintZajavka, _ => true); }
    }

    private void PrintZajavka(object obj)
    {
        ReportManager.Reports["Заявка"].Show();
    }

    public ICommand PrintZajavkaExportCommand
    {
        get { return new Command(PrintZajavkaExport, _ => true); }
    }

    private void PrintZajavkaExport(object obj)
    {
        ReportManager.Reports["Заявка экспорт"].Show();
    }

    public Command PrintWaybillExportCommand
    {
        get { return new Command(PrintWaybillExport, _ => true); }
    }

    private void PrintWaybillExport(object obj)
    {
        ReportManager.Reports["Торг12Экспорт"].Show();
    }

    public Command PrintWaybillCommand
    {
        get { return new Command(PrintWaybill, _ => true); }
    }

    private void PrintWaybill(object obj)
    {
        ReportManager.Reports["Торг12"].Show();
    }

    public Command ExportSFCommand
    {
        get { return new Command(ExportSF, _ => true); }
    }

    public void ExportSF(object obj)
    {
        ReportManager.Reports["Экспорт"].Show();
    }

    public Command PrintSFSchetCommand
    {
        get { return new Command(PrintSChet, _ => true); }
    }

    public void PrintSChet(object obj)
    {
        ReportManager.Reports["Счет"].Show();
    }

    public Command PrintSFSchetExportCommand
    {
        get { return new Command(PrintSChetExport, _ => true); }
    }

    public void PrintSChetExport(object obj)
    {
        ReportManager.Reports["Счет"].ShowSpreadsheet();
    }

    public Command PrintZakazCommand
    {
        get { return new Command(PrintZakaz, _ => true); }
    }

    public Command PrintZakazWOManagerCommand
    {
        get { return new Command(PrintZakazWOManager, _ => true); }
    }

    public void PrintZakaz(object obj)
    {
        ReportManager.Reports["Заказ"].Show();
    }

    public void PrintZakazWOManager(object obj)
    {
        ReportManager.Reports["Заказ без менеджера"].Show();
    }

    public Command PrintSFCommand
    {
        get { return new Command(PrintSF, _ => true); }
    }

    public void PrintSF(object obj)
    {
        ReportManager.Reports["Счет-фактура"].Show();
    }

    public override void UpdatePropertyChangies()
    {
        RaisePropertyChanged(nameof(Document));
        // ReSharper disable once NotResolvedInText
        Document.RaisePropertyChanged("Rows");
    }

    private void UpdatePayDocuments(ALFAMEDIAEntities ctx)
    {
        Document.PaymentDocs.Clear();
        foreach (var c in ctx.SD_33.Where(_ => _.SFACT_DC == Document.DocCode).ToList())
            Document.PaymentDocs.Add(new InvoicePaymentDocument
            {
                DocCode = c.DOC_CODE,
                Code = 0,
                DocumentType = DocumentType.CashIn,
                // ReSharper disable once PossibleInvalidOperationException
                DocumentName =
                    $"{c.NUM_ORD} от {c.DATE_ORD.Value.ToShortDateString()} на {c.SUMM_ORD} " +
                    // ReSharper disable once PossibleInvalidOperationException
                    $"{GlobalOptions.ReferencesCache.GetCurrency(c.CRS_DC)} ({c.CREATOR})",
                // ReSharper disable once PossibleInvalidOperationException
                Summa = (decimal)c.SUMM_ORD,
                Currency = GlobalOptions.ReferencesCache.GetCurrency(c.CRS_DC) as Currency,
                Note = c.NOTES_ORD
            });
        foreach (var c in ctx.TD_101.Include(_ => _.SD_101)
                     .Where(_ => _.VVT_SFACT_CLIENT_DC == Document.DocCode)
                     .ToList())
            Document.PaymentDocs.Add(new InvoicePaymentDocument
            {
                DocCode = c.DOC_CODE,
                Code = c.CODE,
                DocumentType = DocumentType.Bank,
                DocumentName =
                    // ReSharper disable once PossibleInvalidOperationException
                    $"{c.SD_101.VV_START_DATE.ToShortDateString()} на {(decimal)c.VVT_VAL_PRIHOD} {GlobalOptions.ReferencesCache.GetBankAccount(c.SD_101.VV_ACC_DC)}",
                Summa = (decimal)c.VVT_VAL_PRIHOD,
                Currency = GlobalOptions.ReferencesCache.GetCurrency(c.VVT_CRS_DC) as Currency,
                Note = c.VVT_DOC_NUM
            });
        foreach (var c in ctx.TD_110.Include(_ => _.SD_110)
                     .Where(_ => _.VZT_SFACT_DC == Document.DocCode).ToList())
            Document.PaymentDocs.Add(new InvoicePaymentDocument
            {
                DocCode = c.DOC_CODE,
                Code = c.CODE,
                DocumentType = DocumentType.MutualAccounting,
                DocumentName =
                    // ReSharper disable once PossibleInvalidOperationException
                    $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE.ToShortDateString()} на {c.VZT_CRS_SUMMA}",
                // ReSharper disable once PossibleInvalidOperationException
                Summa = (decimal)c.VZT_CRS_SUMMA,
                Currency = GlobalOptions.ReferencesCache.GetCurrency(c.SD_110.CurrencyFromDC) as Currency,
                Note = c.VZT_DOC_NOTES
            });
    }

    public ICommand AddPaymentFromBankCommand
    {
        get
        {
            return new Command(AddPaymentFromBank,
                _ => Document?.Client != null && Document.PaySumma < Document.Summa);
        }
    }

    private void AddPaymentFromBank(object obj)
    {
        var oper = StandartDialogs.SelectBankOperationForClientInvoice(Document.Client.DocCode);
        if (oper == null) return;
        using (var ctx = GlobalOptions.GetEntities())
        {
            var old = ctx.TD_101.Single(_ => _.CODE == oper.Code);
            if (old != null) old.VVT_SFACT_CLIENT_DC = Document.DocCode;

            ctx.SaveChanges();
            UpdatePayDocuments(ctx);
        }
    }

    public ICommand AddPaymentFromCashCommand
    {
        get
        {
            return new Command(AddPaymentFromCash,
                _ => Document?.Client != null && Document.PaySumma < Document.Summa);
        }
    }

    private void AddPaymentFromCash(object obj)
    {
        var oper = StandartDialogs.SelectCashOperationForClientInvoice(Document.Client.DocCode);
        if (oper == null) return;
        using (var ctx = GlobalOptions.GetEntities())
        {
            var old = ctx.SD_33.Single(_ => _.DOC_CODE == oper.DocCode);
            if (old != null)
            {
                old.SFACT_DC = Document.DocCode;
                old.SFACT_CRS_DC = Document.Currency.DocCode;
                old.SFACT_CRS_RATE = 1;
            }

            ctx.SaveChanges();
            UpdatePayDocuments(ctx);
        }
    }

    public ICommand AddPaymentFromVZCommand
    {
        get
        {
            return new Command(AddPaymentFromVZ,
                _ => Document?.Client != null && Document.PaySumma < Document.Summa);
        }
    }

    private void AddPaymentFromVZ(object obj)
    {
        var oper = StandartDialogs.SelectVZOperationForClientInvoice(Document.Client.DocCode);
        if (oper == null) return;
        using (var ctx = GlobalOptions.GetEntities())
        {
            var old = ctx.TD_110.Single(_ => _.DOC_CODE == oper.DocCode && _.CODE == oper.Code);
            if (old != null) old.VZT_SFACT_DC = Document.DocCode;
            ctx.SaveChanges();
            UpdatePayDocuments(ctx);
        }
    }

    #endregion

    #region IDataErrorInfo

    public string this[string columnName] => null;

    public string Error { get; } = null;

    #endregion

    #region Helper class

    internal class NomenklInfo : Nomenkl
    {
        public NomenklInfo(Nomenkl nom)
        {
            Name = nom.Name;
            DocCode = nom.DocCode;
            Currency = nom.Currency;
            SDRSchet = nom.SDRSchet;
            Group = nom.Group;
            FullName = nom.FullName;
            Id = nom.Id;
            Unit = nom.Unit;
            DefaultNDSPercent = nom.DefaultNDSPercent;
            IsCurrencyTransfer = nom.IsCurrencyTransfer;
            IsDeleted = nom.IsDeleted;
            IsNakladExpense = nom.IsNakladExpense;
            IsProguct = nom.IsProguct;
            IsUsluga = nom.IsUsluga;
            IsUslugaInRentabelnost = nom.IsUslugaInRentabelnost;
            MainId = nom.MainId;
            NomenklMain = nom.NomenklMain;
            NomenklNumber = nom.NomenklNumber;
            NomenklType = nom.NomenklType;
            Notes = nom.Notes;
        }

        [Display(AutoGenerateField = true, Name = "Кол-во", Description = "кол-во на всех складах")]
        public decimal QuantityOnStores { set; get; }

        [Display(AutoGenerateField = true, Name = "Посл. операц.")]
        public DateTime LastOperationDate { set; get; }
    }

    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    private class NomenklQuantityInfo
    {
        public decimal DocCode { get; }
        public decimal Quantity { get; }
        public DateTime? LastOperation { get; }
    }

    #endregion
}
