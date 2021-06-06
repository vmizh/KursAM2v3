using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Employee;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.Vzaimozachet;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.ReportManagers.SFClientAndWayBill;
using KursAM2.View.Finance.Invoices;
using KursAM2.ViewModel.Management.Calculations;
using Reports.Base;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public class ClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private InvoicePaymentDocument myCurrentPaymentDoc;

        // ReSharper disable once InconsistentNaming
        private InvoiceClientRow myCurrentRow;
        private ShipmentRowViewModel myCurrentShipmentRow;
        private InvoiceClient myDocument;
        private decimal myOtgruzheno;
        private RSWindowViewModelBase myParentForm;

        #endregion

        #region Constructors

        public ClientWindowViewModel()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ReportManager();
            CreateReports();
            // ReSharper disable once VirtualMemberCallInConstructor
            IsDocNewCopyAllow = true;
            // ReSharper disable once VirtualMemberCallInConstructor
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            CreateReportsMenu();
        }

        public ClientWindowViewModel(decimal? dc) : this()
        {
            if (dc != null)
                // ReSharper disable once VirtualMemberCallInConstructor
                RefreshData(dc);
            // ReSharper disable once VirtualMemberCallInConstructor
            WindowName = Document.ToString();
        }

        #endregion

        #region Properties

        public List<Currency> CurrencyList => MainReferences.Currencies.Values.ToList();
        public List<CentrOfResponsibility> COList => MainReferences.COList.Values.ToList();
        public List<Employee> EmployeeList => MainReferences.Employees.Values.ToList();
        public List<FormPay> FormRaschets => MainReferences.FormRaschets.Values.ToList();
        public List<VzaimoraschetType> VzaimoraschetTypes => MainReferences.VzaimoraschetTypes.Values.ToList();
        public List<PayCondition> PayConditions => MainReferences.PayConditions.Values.ToList();

        public List<CountriesViewModel> Countries => MainReferences.Countries.Values.ToList();

        public override string LayoutName => "InvoiceClientView2";

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

        public override bool IsCanSaveData
        {
            get
            {
                if (Document == null) return false;
                FooterText = string.Empty;
                if (Document.State != RowStatus.NotEdited)
                {
                    Document.RaisePropertyChanged("SummaOtgruz");
                    Document.RaisePropertyChanged("SF_DILER_SUMMA");
                    Document.RaisePropertyChanged("Summa");
                    Document.RaisePropertyChanged("PaySumma");
                }

                // ReSharper disable once UseNameofExpression
                Document.RaisePropertyChanged("State");
                if (Document.State == RowStatus.NotEdited) return false;
                if (Document.PaySumma > Document.Summa)
                {
                    FooterText =
                        $"Сумма оплаты {Document.PaySumma:n2} больше суммы счета {Document.Summa:n2}";
                    return false;
                }

                var res = Document.Entity.SF_CLIENT_DC != null && Document.SF_RECEIVER_KONTR_DC != null
                                                               && Document.Entity.SF_CRS_DC > 0 &&
                                                               Document.SF_CENTR_OTV_DC != null
                                                               && Document.PayCondition != null &&
                                                               Document.SF_VZAIMOR_TYPE_DC != null
                                                               && Document.SF_FORM_RASCH_DC != null
                          || Document.DeletedRows.Count > 0 || Document.Rows.Any(_ => _.State != RowStatus.NotEdited);
                //if (!res)
                //{
                //    if (Document.Entity.SF_CLIENT_DC == 0)
                //        FooterText += "Не выбран контрагент. ";
                //    if (Document.Entity.SF_CRS_DC == 0)
                //        FooterText += "Не выбрана валюта документа. ";
                //    if (Document.SF_CENTR_OTV_DC == null)
                //        FooterText += "Не выбран Центр ответственности. ";
                //    if (Document.PayCondition == null)
                //        FooterText += "Не выбрано условие оплаты. ";
                //    if (Document.SF_VZAIMOR_TYPE_DC == null)
                //        FooterText += "Не выбран тип продукции. ";
                //    if (Document.SF_FORM_RASCH_DC == null)
                //        FooterText += "Не выбрана форма расчетов. ";
                //}

                return res;
            }
        }

        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow
                                                                  && Document.PaymentDocs?.Count == 0 &&
                                                                  Document?.ShipmentRows.Count == 0;

        public override string WindowName => Document?.Name;

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

        public void UpdateVisualData()
        {
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("Summa");
            Document.SF_DILER_SUMMA = Document.Rows.Sum(_ => _.SFT_NACENKA_DILERA);
            if (Form is InvoiceClientView frm)
            {
                frm.KontrSelectButton.IsEnabled = Document.PaymentDocs.Count == 0 && Document.ShipmentRows.Count == 0;
                if (Document.IsNDSIncludeInPrice)
                {
                    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                    if (colPrice != null) colPrice.ReadOnly = true;
                    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
                    if (colSumma != null) colSumma.ReadOnly = false;
                }
                else
                {
                    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                    if (colPrice != null) colPrice.ReadOnly = false;
                    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
                    if (colSumma != null) colSumma.ReadOnly = true;
                }

                frm.gridRows.RefreshData();
                RaisePropertiesChanged(nameof(Document));
            }
        }

        private void SetVisualOnStart()
        {
            var frm = Form as InvoiceClientView;
            if (frm != null)
            {
                if (Document.IsNDSIncludeInPrice)
                {
                    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                    if (colPrice != null) colPrice.ReadOnly = true;
                    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
                    if (colSumma != null) colSumma.ReadOnly = false;
                }
                else
                {
                    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                    if (colPrice != null) colPrice.ReadOnly = false;
                    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
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
            Document.SF_KONTR_CRS_RATE = 1;
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
            if (obj is decimal dc)
            {
                Document = InvoicesManager.GetInvoiceClient(dc);
                Document.WindowViewModel = this;
                SetVisualOnStart();
                return;
            }

            if (IsCanSaveData)
            {
                var res = WinManager.ShowWinUIMessageBox("В документ внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        InvoicesManager.SaveClient(Document);
                        Document = InvoicesManager.GetInvoiceClient(Document.DocCode);
                        Document.WindowViewModel = this;
                        SetVisualOnStart();
                        return;
                    case MessageBoxResult.No:
                        Document = InvoicesManager.GetInvoiceClient(Document.DocCode);
                        Document.WindowViewModel = this;
                        SetVisualOnStart();
                        RaisePropertiesChanged(nameof(Document));
                        return;
                }
            }
            else
            {
                Document = InvoicesManager.GetInvoiceClient(Document.DocCode);
                Document.WindowViewModel = this;
                SetVisualOnStart();
            }
        }

        public override void SaveData(object data)
        {
            if (Document.Rows.Any(_ => _.Nomenkl.IsUsluga) && !Document.IsAccepted)
            {
                var res = WinManager.ShowWinUIMessageBox("В счете имеются услуги. Акцептовать счет?", "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes) Document.IsAccepted = true;
            }

            var dc = InvoicesManager.SaveClient(Document);
            if (dc > 0)
            {
                if (Document.Entity.SF_CLIENT_DC != null)
                    RecalcKontragentBalans.CalcBalans((decimal) Document.Entity.SF_CLIENT_DC, Document.DocDate);
                if (Document.SF_DILER_DC != null)
                    RecalcKontragentBalans.CalcBalans((decimal) Document.SF_DILER_DC, Document.DocDate);
                Document.DocCode = dc;
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
                RefreshData(null);
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.InvoiceClient, Document.Id, Document.DocCode,
                    Document.CREATOR,
                    "", Document.Description);
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
                Document.DeletedRows.Add(CurrentRow);
            Document.Rows.Remove(CurrentRow);
            UpdateVisualData();
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
                    if (Document.State == RowStatus.NewRow)
                    {
                        Form.Close();
                        return;
                    }

                    InvoicesManager.DeleteClient(Document.DocCode);
                    // ReSharper disable once PossibleInvalidOperationException
                    RecalcKontragentBalans.CalcBalans((decimal) Document.Entity.SF_CLIENT_DC, Document.DocDate);
                    if (Document.SF_DILER_DC != null)
                        RecalcKontragentBalans.CalcBalans((decimal) Document.SF_DILER_DC, Document.DocDate);
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
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewClientRequisite(Document.DocCode);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (Document == null) return;
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel
            {
                Form = frm, Document = InvoicesManager.NewClientCopy(Document.DocCode)
            };
            frm.Show();
            frm.DataContext = ctx;
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
                    if (Document.Rows.Any(_ => _.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    Document.Rows.Add(new InvoiceClientRow
                    {
                        DOC_CODE = Document.DocCode,
                        Code = newCode,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = (double) nds,
                        SFT_KOL = 1,
                        SFT_ED_CENA = 0
                    });
                    newCode++;
                }
            }

            UpdateVisualData();
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
                    if (Document != null && Document.Rows.Any(_ => _.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    Document?.Rows.Add(new InvoiceClientRow
                    {
                        DOC_CODE = Document.DocCode,
                        Code = newCode,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = (double) nds,
                        SFT_KOL = 1,
                        SFT_ED_CENA = 0
                    });
                    newCode++;
                }
            }

            UpdateVisualData();
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