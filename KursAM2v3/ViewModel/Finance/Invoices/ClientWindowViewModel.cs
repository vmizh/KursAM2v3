using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Employee;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.Vzaimozachet;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Managers.Nomenkl;
using KursAM2.ReportManagers.SFClientAndWayBill;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.Calculations;
using Reports.Base;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class ClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private InvoicePaymentDocument myCurrentPaymentDoc;

        // ReSharper disable once InconsistentNaming
        private InvoiceClientRow myCurrentRow;
        private ShipmentRowViewModel myCurrentShipmentRow;
        private InvoiceClient myDocument;
        private decimal myOtgruzheno;
        private RSWindowViewModelBase myParentForm;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public readonly GenericKursDBRepository<SD_84> GenericClientRepository;
        public IInvoiceClientRepository InvoiceClientRepository;
        private readonly List<decimal> myUsedNomenklsDC = new List<decimal>();

        #endregion

        #region Constructors

        public ClientWindowViewModel()
        {
            GenericClientRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
            InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
            // ReSharper disable once ObjectCreationAsStatement
            new ReportManager();
            CreateReports();
            // ReSharper disable once VirtualMemberCallInConstructor
            IsDocNewCopyAllow = true;
            // ReSharper disable once VirtualMemberCallInConstructor
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Счет-фактура клиенту (новая)";
            CreateReportsMenu();
        }

        public ClientWindowViewModel(decimal? dc) : this()
        {
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
                    SF_FACT_SUMMA = 0
                };
                UnitOfWork.Context.SD_84.Add(doc);
                Document = new InvoiceClient(doc, UnitOfWork)
                {
                    State = RowStatus.NewRow
                };
            }
            else
            {
                Document = new InvoiceClient(doc, UnitOfWork)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                Document.myState = RowStatus.NotEdited;
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                SetVisualOnStart();
            }
        }

        #endregion

        #region Properties

        public List<Currency> CurrencyList => MainReferences.Currencies.Values.ToList();
        public List<CentrOfResponsibility> COList => MainReferences.COList.Values.Where(_ => _.DocCode > 1).ToList();
        public List<Employee> EmployeeList => MainReferences.Employees.Values.ToList();
        public List<FormPay> FormRaschets => MainReferences.FormRaschets.Values.ToList();
        public List<VzaimoraschetType> VzaimoraschetTypes => MainReferences.VzaimoraschetTypes.Values.ToList();
        public List<PayCondition> PayConditions => MainReferences.PayConditions.Values.ToList();

        public List<CountriesViewModel> Countries => MainReferences.Countries.Values.ToList();

        public override string LayoutName => "InvoiceClientView2";


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
                if (myCurrentShipmentRow != null && myCurrentShipmentRow.Equals(value)) return;
                myCurrentShipmentRow = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceClientRow CurrentRow
        {
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
            get => myCurrentRow;
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

        public InvoiceClient Document
        {
            get => myDocument;
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
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

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.DocCode = -1;
            Document.DocDate = DateTime.Today;
            Document.REGISTER_DATE = DateTime.Today;
            Document.CREATOR = GlobalOptions.UserInfo.Name;
            Document.InnerNumber = -1;
            Document.OuterNumber = null;
            Document.IsAccepted = false;
            Document.myState = RowStatus.NewRow;
            Document.Id = newId;
            Document.DeletedRows = new List<InvoiceClientRow>();
            Document.PaymentDocs.Clear();
            Document.ShipmentRows.Clear();
            UnitOfWork.Context.SD_84.Add(Document.Entity);
            if (isCopy)
            {
                var newCode = 1;
                foreach (var item in Document.Rows)
                {
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    item.DocCode = -1;
                    item.Id = Guid.NewGuid();
                    item.DocId = newId;
                    item.DocCode = newCode;
                    item.Shipped = 0;
                    item.State = RowStatus.NewRow;
                    newCode++;
                }

                foreach (var r in Document.Rows)
                {
                    UnitOfWork.Context.TD_84.Add(r.Entity);
                    r.State = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.TD_84.Clear();
                }

                Document.Rows.Clear();
            }
        }

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
                Document.RaisePropertyChanged("Summa");
                RaisePropertiesChanged(nameof(Document));
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
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Счет",
                Command = PrintSFSchetCommand
            });
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Счет экспорт",
                Command = PrintSFSchetExportCommand
            });
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Счет фактура",
                Command = PrintSFCommand
            });
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Экспорт",
                Command = ExportSFCommand
            });
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
            ReportManager.Reports.Add("Экспорт", new SFClientSchetFacturaReport(this)
            {
                PrintOptions = new KursReportA4PrintOptions(),
                ShowType = ReportShowType.Spreadsheet,
                XlsFileName = "SFClient"
            });
            ReportManager.Reports.Add("Счет", new SFClientSFSChet(this)
            {
                PrintOptions = new KursReportA4PrintOptions(),
                ShowType = ReportShowType.Report,
                XlsFileName = "Account2"
            });
            ReportManager.Reports.Add("Счет-фактура", new SFClientSchetFacturaReport(this)
            {
                PrintOptions = new KursReportLandscapeA4PrintOptions(),
                ShowType = ReportShowType.Report,
                XlsFileName = "SFClient"
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

        #endregion

        #region Command

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.InvoiceClient, null, Document.DocCode, null);
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

            var kontr = StandartDialogs.SelectKontragent(Document.Currency);
            if (kontr == null) return;
            if (Document.Rows.Any(_ => !_.IsUsluga && _.Nomenkl.Currency.DocCode != kontr.BalansCurrency.DocCode))
            {
                WindowManager.ShowMessage(
                    "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
                    "Предупреждение", MessageBoxImage.Information);
                return;
            }

            Document.Client = kontr;
            Document.Currency = kontr.BalansCurrency;
            Document.Entity.SF_KONTR_CRS_RATE = 1;
        }

        public ICommand DilerSelectCommand
        {
            get { return new Command(DilerSelect, _ => true); }
        }

        private void DilerSelect(object obj)
        {
            if (Document == null)
                return;
            var kontr = StandartDialogs.SelectKontragent(Document.Currency);
            if (kontr == null) return;
            if (Document.Rows.Any(_ => !_.IsUsluga && _.Nomenkl.Currency.DocCode != kontr.BalansCurrency.DocCode))
            {
                WindowManager.ShowMessage(
                    "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
                    "Предупреждение", MessageBoxImage.Information);
                return;
            }

            Document.Diler = kontr;
            Document.SF_DILER_CRS_DC = kontr.BalansCurrency.DocCode;
            Document.SF_DILER_SUMMA = 0;
            Document.SF_DILER_RATE = 1;
        }

        public ICommand ReceiverSelectCommand
        {
            get { return new Command(ReceiverSelect, _ => true); }
        }

        private void ReceiverSelect(object obj)
        {
            if (Document == null)
                return;
            var kontr = StandartDialogs.SelectKontragent(Document.Currency);
            if (kontr == null) return;
            if (Document.Rows.Any(_ => !_.IsUsluga && _.Nomenkl.Currency.DocCode != kontr.BalansCurrency.DocCode))
            {
                WindowManager.ShowMessage(
                    "По счету есть товары с валютой, отличной от валюты контрагента. Изменить контрагента нельзя.",
                    "Предупреждение", MessageBoxImage.Information);
                return;
            }

            Document.Receiver = kontr;
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            myUsedNomenklsDC.Clear();
            if (IsCanSaveData)
            {
                var res = WinManager.ShowWinUIMessageBox("В документ внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        return;
                    case MessageBoxResult.No:
                        foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        RaiseAll();
                        Document.myState = RowStatus.NotEdited;
                        Document.RaisePropertyChanged("State");
                        foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                        return;
                }
            }

            if (Document.DocCode > 0 && Document.State != RowStatus.NewRow)
            {
                foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                LoadFromExternal();
                
                foreach (var r in Document.Rows)
                {
                    r.myState = RowStatus.NotEdited;
                    AddUsedNomenkl(r.Nomenkl.DocCode);
                }
                RaiseAll();
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
            }
        }


        private void LoadFromExternal()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                Document.PaymentDocs.Clear();
                foreach (var c in ctx.TD_101.Include(_ => _.SD_101)
                    .Where(_ => _.VVT_SFACT_CLIENT_DC == Document.DocCode))
                    Document.PaymentDocs.Add(new InvoicePaymentDocument
                    {
                        DocCode = c.DOC_CODE,
                        Code = c.CODE,
                        DocumentType = DocumentType.Bank,
                        DocumentName =
                            // ReSharper disable once PossibleInvalidOperationException
                            $"{c.SD_101.VV_START_DATE.ToShortDateString()} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                        Summa = (decimal) c.VVT_VAL_PRIHOD,
                        Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                        Note = c.VVT_DOC_NUM
                    });
            }
        }

        private void RaiseAll()
        {
            foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
            foreach (var s in Document.ShipmentRows) s.RaisePropertyAllChanged();
            foreach (var pay in Document.PaymentDocs) pay.RaisePropertyAllChanged();
            Document.RaisePropertyAllChanged();
        }

        public override void SaveData(object data)
        {
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

            UnitOfWork.CreateTransaction();
            // ReSharper disable once CollectionNeverUpdated.Local
            try
            {
                if (Document.State == RowStatus.NewRow || Document.DocCode < 0)
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
                    Document.SF_KONTR_CRS_DC = Document.Client.BalansCurrency.DocCode;

                if (Document.Entity.SF_KONTR_CRS_SUMMA == null) Document.Entity.SF_KONTR_CRS_SUMMA = Document.Summa;
                foreach (var row in Document.Rows)
                    if (row.Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS == null)
                        row.Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = row.Summa;
                UnitOfWork.Save();
                UnitOfWork.Commit();
                RecalcKontragentBalans.CalcBalans(Document.Client.DocCode, Document.DocDate);
                NomenklManager.RecalcPrice(myUsedNomenklsDC);
                foreach (var ndc in Document.Rows.Select(_ => _.Nomenkl.DocCode)) AddUsedNomenkl(ndc);
                NomenklManager.RecalcPrice(myUsedNomenklsDC);
                myUsedNomenklsDC.Clear();
                //foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                //RaiseAll();
                Document.myState = RowStatus.NotEdited;
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

                foreach (var f in Document.ShipmentRows) f.myState = RowStatus.NotEdited;

                foreach (var p in Document.PaymentDocs) p.myState = RowStatus.NotEdited;
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceClient), null,
                    Document.DocCode, null, (string) Document.ToJson());
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.InvoiceClient, Document.Id, Document.DocCode,
                    Document.CREATOR, "", Document.Description);
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
                            m.VZT_SFACT_DC = null;
                            break;
                    }

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
                    param => CurrentRow != null && Document != null && Document.PaySumma == 0);
            }
        }

        private void DeleteRow(object obj)
        {
            if (CurrentRow != null && CurrentRow.State != RowStatus.NewRow)
            {
                Document.DeletedRows.Add(CurrentRow);
                Document.Entity.TD_84.Remove(CurrentRow.Entity);
                Document.Rows.Remove(CurrentRow);
            }

            UpdateVisualData(null);
        }

        public override void DocDelete(object obj)
        {
            if (Document == null) return;
            var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNoCancel,
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
                        GenericClientRepository.Delete(Document.Entity);
                        UnitOfWork.Save();
                        UnitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        UnitOfWork.Rollback();
                        WindowManager.ShowError(ex);
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
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewClient();
            frm.Show();
            frm.DataContext = ctx;
        }

        public override bool IsDocNewCopyAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsDocNewCopyRequisiteAllow => Document != null && Document.State != RowStatus.NewRow;

        public override void DocNewCopyRequisite(object obj)
        {
            if (Document == null) return;
            var ctx = new ClientWindowViewModel(Document.DocCode);
            ctx.SetAsNewCopy(false);
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
            var ctx = new ClientWindowViewModel(Document.DocCode);
            ctx.SetAsNewCopy(true);
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
            get { return new Command(AddUsluga, _ => true); }
        }

        private void AddUsluga(object obj)
        {
            var k = StandartDialogs.SelectNomenkls();
            if (k != null)
            {
                var newCode = Document?.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var item in k)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    if (Document.Rows.Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var r = new InvoiceClientRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        NDSPercent = nds,
                        Quantity = 1,
                        Price = 0,
                        Parent = Document,
                        IsNDSInPrice = Document.IsNDSIncludeInPrice,
                        Note = "",
                        Id = Guid.NewGuid(),
                        DocId = Document.Id
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
            get { return new Command(AddNomenkl, param => true); }
        }

        private void AddNomenkl(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(Document?.Currency, true);
            if (k != null)
            {
                var newCode = Document?.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var item in k)
                {
                    if (Document != null && Document.Rows.Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var r = new InvoiceClientRow
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Parent = Document,
                        NDSPercent = nds,
                        Quantity = 1,
                        Price = 0,
                        IsNDSInPrice = Document.IsNDSIncludeInPrice,
                        Note = "",
                        Id = Guid.NewGuid(),
                        DocId = Document.Id
                    };
                    r.Entity.SFT_NEMENKL_DC = item.DOC_CODE;
                    Document?.Rows.Add(r);
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

        public ICommand PrintZajavkaCommand
        {
            get { return new Command(PrintZajavka, param => true); }
        }

        private void PrintZajavka(object obj)
        {
            ReportManager.Reports["Заявка"].Show();
        }

        public ICommand PrintZajavkaExportCommand
        {
            get { return new Command(PrintZajavkaExport, param => true); }
        }

        private void PrintZajavkaExport(object obj)
        {
            ReportManager.Reports["Заявка экспорт"].Show();
        }

        public Command PrintWaybillExportCommand
        {
            get { return new Command(PrintWaybillExport, param => true); }
        }

        private void PrintWaybillExport(object obj)
        {
            ReportManager.Reports["Торг12Экспорт"].Show();
        }

        public Command PrintWaybillCommand
        {
            get { return new Command(PrintWaybill, param => true); }
        }

        private void PrintWaybill(object obj)
        {
            ReportManager.Reports["Торг12"].Show();
        }

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, param => true); }
        }

        public void ExportSF(object obj)
        {
            ReportManager.Reports["Экспорт"].Show();
        }

        public Command PrintSFSchetCommand
        {
            get { return new Command(PrintSChet, param => true); }
        }

        public void PrintSChet(object obj)
        {
            ReportManager.Reports["Счет"].Show();
        }

        public Command PrintSFSchetExportCommand
        {
            get { return new Command(PrintSChetExport, param => true); }
        }

        public void PrintSChetExport(object obj)
        {
            ReportManager.Reports["Счет"].ShowSpreadsheet();
        }

        public Command PrintZakazCommand
        {
            get { return new Command(PrintZakaz, param => true); }
        }

        public Command PrintZakazWOManagerCommand
        {
            get { return new Command(PrintZakazWOManager, param => true); }
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
            get { return new Command(PrintSF, param => true); }
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
                        $"{MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = (decimal) c.SUMM_ORD,
                    Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
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
                        $"{c.SD_101.VV_START_DATE.ToShortDateString()} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                    Summa = (decimal) c.VVT_VAL_PRIHOD,
                    Currency = MainReferences.Currencies[c.VVT_CRS_DC],
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
                    Summa = (decimal) c.VZT_CRS_SUMMA,
                    Currency = MainReferences.Currencies[c.SD_110.CurrencyFromDC],
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
    }
}