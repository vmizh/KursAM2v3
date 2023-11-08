using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.Xpf;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.UC;
using KursAM2.ViewModel.Dogovora;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Currency;
using KursDomain.Documents.Dogovora;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using Reports.Base;
using ConditionRule = Helper.ConditionRule;
using NomenklProductType = KursDomain.References.NomenklProductType;

namespace KursAM2.ViewModel.Finance.Invoices
{
    /// <summary>
    ///     Сфчет-фактура поставщика
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public sealed class ProviderWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Methods

        public override void UpdateVisualObjects()
        {
            if (Form is InvoiceProviderView view)
            {
                var cols = view.gridPays.TotalSummary.GetForName("Rate");
                if (cols.Count > 0)
                {
                    view.gridPays.TotalSummary.BeginUpdate();
                    foreach (var c in cols) view.gridPays.TotalSummary.Remove(c);

                    view.gridPays.TotalSummary.Add(new GridSummaryItem
                    {
                        FieldName = "Rate",
                        SummaryType = SummaryItemType.Custom
                    });
                    view.gridPays.TotalSummary.EndUpdate();
                }
            }
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

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.InvoiceProvider, null, Document.DocCode, null);
        }

        private void AddUsedNomenkl(decimal nomdc)
        {
            if (myUsedNomenklsDC.All(_ => _ != nomdc)) myUsedNomenklsDC.Add(nomdc);
        }

        private void DeletePayments()
        {
            //TODO Добавить для кассы и акта взаимозачета
            if (Document.PaymentDocs.All(_ => _.State != RowStatus.NewRow)) return;
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var bankListCodes = ctx.Database.SqlQuery<int>(
                            "SELECT code FROM td_101 WHERE VVT_SFACT_POSTAV_DC IS NOT NULL" +
                            " and CODE NOT IN (SELECT BankCode FROM ProviderInvoicePay WHERE BankCode IS NOT null)")
                        .ToList();
                    foreach (var code in bankListCodes)
                    {
                        var item = ctx.TD_101.FirstOrDefault(_ => _.CODE == code);
                        if (item != null)
                            item.VVT_SFACT_POSTAV_DC = null;
                    }

                    ctx.SaveChanges();
                }
            }
        }

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.Id = newId;
            Document.DocCode = -1;
            Document.SF_POSTAV_NUM = null;
            Document.DocDate = DateTime.Today;
            Document.SF_REGISTR_DATE = DateTime.Today;
            Document.CREATOR = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.PaymentDocs.Clear();
            Document.Facts.Clear();
            Document.IsAccepted = false;
            Document.IsNDSInPrice = true;
            Document.NakladDistributedSumma = 0;

            UnitOfWork.Context.SD_26.Add(Document.Entity);
            Document.DeletedRows.Clear();
            Document.PaymentDocs.Clear();
            Document.Facts.Clear();
            if (isCopy)
            {
                var newCode = 1;
                foreach (var row in Document.Rows.Cast<InvoiceProviderRow>())
                {
                    UnitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.DocCode = -1;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    row.Code = newCode;
                    row.myState = RowStatus.NewRow;
                    row.CurrencyConvertRows.Clear();
                    newCode++;
                }

                foreach (var r in Document.Rows.Cast<InvoiceProviderRow>())
                {
                    UnitOfWork.Context.TD_26.Add(r.Entity);
                    r.CalcRow();
                    r.State = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows.Cast<InvoiceProviderRow>())
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                Document.Entity.TD_26.Clear();
                Document.Rows.Clear();
                Document.SF_FACT_SUMMA = 0;
                Document.SF_KONTR_CRS_SUMMA = 0;
                Document.Entity.SF_CRS_SUMMA = 0;
                Document.SF_FACT_SUMMA = 1;
                Document.SF_CRS_RATE = 1;
                Document.SF_KONTR_CRS_RATE = 1;
                Document.SF_UCHET_VALUTA_RATE = 1;
                Document.SummaFact = 0;
            }
        }

        #endregion

        #region Fields

        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        private InvoiceProvider myDocument;
        private InvoiceProviderRow myCurrentRow;
        public readonly GenericKursDBRepository<SD_26> GenericProviderRepository;
        private readonly WindowManager myWManager = new WindowManager();
        private readonly List<decimal> myUsedNomenklsDC = new List<decimal>();
        private ProviderInvoicePayViewModel myCurrentPaymentDoc;
        private InvoiceProviderRowCurrencyConvertViewModel myCurrentCrsConvertItem;

        // ReSharper disable once NotAccessedField.Local
        public IInvoiceProviderRepository InvoiceProviderRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        #endregion

        #region Constructors

        public ProviderWindowViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);

            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            CreateReportsMenu();
        }

        public ProviderWindowViewModel(decimal? dc) : this()
        {
            var doc = dc != null ? GenericProviderRepository.GetById(dc.Value) : null;
            if (doc == null)
            {
                doc = new SD_26
                {
                    DOC_CODE = -1,
                    SF_POSTAV_DATE = DateTime.Today,
                    SF_REGISTR_DATE = DateTime.Today,
                    CREATOR = GlobalOptions.UserInfo.Name,
                    SF_CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    SF_POSTAV_NUM = null,
                    Id = Guid.NewGuid(),
                    SF_RUB_SUMMA = 0,
                    SF_CRS_SUMMA = 0,
                    SF_PAY_FLAG = 0,
                    SF_FACT_SUMMA = 0,
                    SF_EXECUTED = 0,
                    SF_NDS_VKL_V_CENU = 1,
                    SF_KONTR_CRS_SUMMA = 0,
                    SF_SUMMA_V_UCHET_VALUTE = 0,
                    SF_KONTR_CRS_RATE = 1,
                    TD_26 = new List<TD_26>()
                };
                UnitOfWork.Context.SD_26.Add(doc);

                Document = new InvoiceProvider(doc, UnitOfWork)
                {
                    State = RowStatus.NewRow
                };
            }
            else
            {
                Document = new InvoiceProvider(doc, UnitOfWork)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                Document.myState = RowStatus.NotEdited;
                var crsrates = new CurrencyRates(DateTime.Today.AddDays(-100), DateTime.Today);
                var rate = Math.Round(crsrates.GetRate(Document.Currency.DocCode,
                    GlobalOptions.SystemProfile.NationalCurrency.DocCode, DateTime.Today), 4);
                if (Document.PaymentDocs.Count > 0)
                    rate = Document.PaymentDocs.Sum(_ => _.Summa * _.Rate) / Document.PaymentDocs.Sum(_ => _.Summa);
                foreach (var r in Document.Rows.Cast<InvoiceProviderRow>())
                {
                    AddUsedNomenkl(r.Nomenkl.DocCode);
                    foreach (var rr in r.CurrencyConvertRows)
                    {
                        rr.Rate = rate;
                        rr.myState = RowStatus.NotEdited;
                        AddUsedNomenkl(rr.Nomenkl.DocCode);
                    }

                    r.myState = RowStatus.NotEdited;
                }

                Document.myState = RowStatus.NotEdited;

                //RaiseAll();
            }
        }

        #endregion

        #region Properties

        public List<Currency> CurrencyList => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>()
            .OrderBy(_ => _.Name).ToList();

        public List<CentrResponsibility> COList => GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll()
            .Cast<CentrResponsibility>().OrderBy(_ => _.Name).ToList();

        public List<Employee> EmployeeList => GlobalOptions.ReferencesCache.GetEmployees().Cast<Employee>()
            .OrderBy(_ => _.Name).ToList();

        public List<PayForm> FormRaschetList => GlobalOptions.ReferencesCache.GetPayFormAll().Cast<PayForm>()
            .OrderBy(_ => _.Name).ToList();

        public List<NomenklProductType> VzaimoraschetTypeList => GlobalOptions.ReferencesCache
            .GetNomenklProductTypesAll().Cast<NomenklProductType>()
            .OrderBy(_ => _.Name).ToList();

        public List<PayCondition> PayConditionList => GlobalOptions.ReferencesCache.GetPayConditionAll()
            .Cast<PayCondition>()
            .OrderBy(_ => _.Name).ToList();

        public List<Country> Countries => GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Country>()
            .OrderBy(_ => _.Name).ToList();

        public override string LayoutName => "InvoiceProviderView";

        public override string WindowName =>
            Document?.DocCode > 0 ? Document.ToString() : "Счет-фактура поставщика (новая)";

        public ObservableCollection<FormattingRule> Rules { get; } = new ObservableCollection<FormattingRule>
        {
            new FormattingRule(nameof(InvoiceProviderRow.Shipped), ConditionRule.Equal, 0m, true,
                FormattingType.Foreground)
        };

        public List<Tuple<decimal,int>> DeletedStoreLink = new List<Tuple<decimal,int>>();

        public List<InvoiceProviderRowCurrencyConvertViewModel> DeletedCrsConvertItems { set; get; } =
            new List<InvoiceProviderRowCurrencyConvertViewModel>();

        public bool IsCurrencyEnabled => Document?.Kontragent == null;

        public ProviderInvoicePayViewModel CurrentPaymentDoc
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentPaymentDoc == value) return;
                myCurrentPaymentDoc = value;
                RaisePropertiesChanged();
            }
            get => myCurrentPaymentDoc;
        }

        public InvoiceProvider Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProviderRow CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<InvoiceProviderRow> SelectedRows { set; get; }


        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<WarehouseOrderInRow> SelectedFacts { set; get; } =
            new ObservableCollection<WarehouseOrderInRow>();

        private WarehouseOrderInRow myCurrentFact;

        public WarehouseOrderInRow CurrentFact
        {
            get => myCurrentFact;
            set
            {
                if (myCurrentFact == value) return;
                myCurrentFact = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null
                                              && Document.State != RowStatus.NotEdited
                                              && Document.CO != null && Document.Currency != null
                                              && Document.FormRaschet != null && Document.Kontragent != null
                                              && Document.PayCondition != null && Document.Employee != null;

        public ICommand DogovorOpenCommand
        {
            get { return new Command(DogovorOpen, _ => Document.Contract != null); }
        }

        private void DogovorOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.DogovorOfSupplier, 0, Document.Contract.Id);
        }

        public ICommand DogovorDeleteLinkCommand
        {
            get { return new Command(DogovorDeleteLink, _ => Document.Contract != null); }
        }

        private void DogovorDeleteLink(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Вы действительно хотите удалить связь с договором?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.Yes) Document.Contract = null;
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is InvoiceProviderView frm)
            {
                var col = frm.gridFacts.Columns.FirstOrDefault(_ => _.FieldName == "LinkOrder");
                if (col != null)
                    frm.gridFacts.Columns.Remove(col);
                GridSummaryItem g = null;
                foreach (var s in frm.gridRows.TotalSummary)
                {
                    if (s.FieldName != "NDSPercent") continue;
                    g = s;
                    break;
                }
                if (g != null)
                {
                    frm.gridRows.TotalSummary.Remove(g);
                }
                if (frm.tableViewRows.FormatConditions.Count != 0) return;
                var cond = Rules.First().GetFormatCondition();
                cond.Format = new Format()
                {
                    Foreground = Brushes.Red
                };
                frm.tableViewRows.FormatConditions.Add(cond);
            }
        }

        public ICommand DogovorSelectCommand
        {
            get { return new Command(DogovorSelect, _ => true); }
        }

        private void DogovorSelect(object obj)
        {
            var ctx = new DogovorSelectDialogViewModel(true);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.YesNo, "Выбор договора поставщика", ctx) == MessageResult.Yes)
            {
                Document.Contract = new DogovorOfSupplierViewModel(ctx.CurrentDogovor.Dog);
                if (Document.Kontragent == null)
                {
                    Document.Kontragent = ctx.CurrentDogovor.Kontragent;
                    Document.Currency = Document.Kontragent.Currency as Currency;
                }

                var cr = new CurrencyRates(Document.DocDate, Document.DocDate);
                var newCode = Document.Rows.Any() ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var p in ctx.DogovorPositionList.Where(_ => _.IsSelected))
                {
                    if (Document.Rows.Any(_ => _.Nomenkl.DocCode == p.Nomenkl.DocCode)) continue;
                    AddUsedNomenkl(p.Nomenkl.DocCode);
                    var newRow = new InvoiceProviderRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = p.Nomenkl,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        NDSPercent = p.NDSPercent,
                        PostUnit = p.Unit,
                        UchUnit = p.Unit,
                        Note = " ",
                        State = RowStatus.NewRow,
                        IsIncludeInPrice = Document.IsNDSInPrice,
                        Parent = Document,
                        Entity = { SFT_POST_ED_IZM_DC = p.Unit.DocCode }
                    };
                    newRow.CalcRow();
                    // ReSharper disable once PossibleNullReferenceException
                    switch (Document.Currency.DocCode)
                    {
                        case CurrencyCode.EUR:
                            newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.EUR,
                                Document.DocDate);
                            newRow.EURSumma = newRow.EURRate;
                            break;
                        case CurrencyCode.USD:
                            newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.USD,
                                Document.DocDate);
                            newRow.EURSumma = newRow.USDRate;
                            break;
                        case CurrencyCode.RUB:
                            newRow.RUBRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.RUB,
                                Document.DocDate);
                            newRow.RUBSumma = newRow.RUBRate;
                            break;
                        case CurrencyCode.GBP:
                            newRow.GBPRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.GBP,
                                Document.DocDate);
                            newRow.GBPSumma = newRow.GBPRate;
                            break;
                    }

                    Document.Entity.TD_26.Add(newRow.Entity);
                    Document.Rows.Add(newRow);
                    newCode++;
                }
            }
        }

        public ICommand KontragentSelectCommand
        {
            get
            {
                return new Command(KontragentSelect, _ => Document.PaymentDocs.Count == 0 &&
                                                          Document.Facts.Count == 0);
            }
        }

        private void KontragentSelect(object obj)
        {
            if (Document.PaySumma != 0)
            {
                WindowManager.ShowMessage("По счету есть Оплата. Изменить контрагента нельзя.",
                    "Предупреждение", MessageBoxImage.Information);
                return;
            }

            var kontr = StandartDialogs.SelectKontragent(Document.Currency);
            if (kontr == null) return;
            Document.Kontragent = kontr;
            Document.Currency = kontr.Currency as Currency;
            RaisePropertyChanged(nameof(IsCurrencyEnabled));
        }

        public ICommand ReceiverSelectCommand
        {
            get { return new Command(ReceiverSelect, _ => true); }
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void ReceiverSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent();
            if (kontr == null) return;
            Document.KontrReceiver = kontr;
        }

        public ICommand AddStoreLinkCommand
        {
            get { return new Command(AddStoreLink, _ => true); }
        }

        private void AddStoreLink(object obj)
        {
            var ctx = new AddNomenklFromOrderInViewModel(Document.Kontragent);
            var dlg = new SelectDialogView { DataContext = ctx };
            ctx.Form = dlg;
            if (dlg.ShowDialog() == false) return;
            var defaultNDS = Convert.ToDecimal(GenericProviderRepository.Context.PROFILE
                .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
            string docName = null;
            foreach (var item in ctx.Nomenkls.Where(_ => _.IsChecked))
            {
                var old = Document.Facts.FirstOrDefault(_ => _.DocCode == item.DocCode
                                                             && _.Code == item.Code);
                if (old != null)
                {
                    old.DDT_KOL_PRIHOD += item.Quantity;
                    var srow = Document.Rows.FirstOrDefault(_ => _.Nomenkl.DocCode == old.Nomenkl.DocCode);
                    if (srow != null && old.DDT_KOL_PRIHOD > srow.Quantity) srow.Quantity = old.DDT_KOL_PRIHOD;
                }
                else
                {
                    using (var dtx = GlobalOptions.GetEntities())
                    {
                        var d = dtx.SD_24.FirstOrDefault(_ => _.DOC_CODE == item.DocCode);
                        if (d != null)
                            docName =
                                $"Приходный складской ордер {d.DD_IN_NUM}/{d.DD_EXT_NUM} от {d.DD_DATE.ToShortDateString()} {d.DD_NOTES}";
                    }

                    var srow = Document.Rows.FirstOrDefault(_ => _.Nomenkl.DocCode == item.Nomenkl.DocCode);
                    if (srow == null)
                    {
                        var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                        var r = new InvoiceProviderRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Id = Guid.NewGuid(),
                            DocId = Document.Id,
                            Nomenkl = item.Nomenkl,
                            Quantity = item.Quantity,
                            Price = 0,
                            NDSPercent = item.Nomenkl.DefaultNDSPercent ?? defaultNDS,
                            PostUnit = (Unit)item.Nomenkl.Unit,
                            UchUnit = (Unit)item.Nomenkl.Unit,
                            State = RowStatus.NewRow
                        };
                        r.Entity.SFT_POST_ED_IZM_DC = ((IDocCode)item.Nomenkl.Unit).DocCode;
                        Document.Rows.Add(r);
                        var oldOrdRow = GenericProviderRepository.Context.TD_24.Include(_ => _.SD_24).FirstOrDefault(
                            _ =>
                                _.DOC_CODE == item.DocCode
                                && _.CODE == item.Code);
                        Document.Facts.Add(new WarehouseOrderInRow(oldOrdRow)
                        {
                            DDT_SPOST_DC = Document.DocCode,
                            DDT_SPOST_ROW_CODE = newCode,
                            DDT_TAX_EXECUTED = 1,
                            DDT_FACT_EXECUTED = 1,
                            myName = docName
                        });
                    }
                    else
                    {
                        var oldOrdRow = GenericProviderRepository.Context.TD_24.Include(_ => _.SD_24).FirstOrDefault(
                            _ =>
                                _.DOC_CODE == item.DocCode
                                && _.CODE == item.Code);
                        Document.Facts.Add(new WarehouseOrderInRow(oldOrdRow)
                        {
                            State = RowStatus.NewRow,
                            DDT_TAX_EXECUTED = 1,
                            DDT_FACT_EXECUTED = 1,
                            myName = docName
                        });
                        if (oldOrdRow != null)
                        {
                            oldOrdRow.DDT_SPOST_DC = srow.DocCode;
                            oldOrdRow.DDT_SPOST_ROW_CODE = srow.Code;
                            //genericProviderRepository.Context.SaveChanges();
                        }

                        if (srow.Quantity < item.Quantity)
                            srow.Quantity = item.Quantity;
                    }
                }
            }

            UpdateVisualData();
        }

        public ICommand DeleteStoreLinkCommand
        {
            get { return new Command(DeleteStoreLink, _ => CurrentFact != null); }
        }

        private void DeleteStoreLink(object obj)
        {
            var WinManager = new WindowManager();
            var rowForRemove = new List<Tuple<decimal, int>>();
            var res = WinManager.ShowWinUIMessageBox("Действительно хотите удалить связь с приходными ордерами?",
                "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    foreach (var item in SelectedFacts)
                    {

                        // ReSharper disable PossibleInvalidOperationException
                        DeletedStoreLink.Add(new Tuple<decimal, int>(item.DOC_CODE, item.Code));
                        // ReSharper restore PossibleInvalidOperationException
                        rowForRemove.Add(new Tuple<decimal, int>(item.DOC_CODE, item.Code));

                    }

                    foreach (var ritem in rowForRemove
                                 .Select(r =>
                                     Document.Facts.FirstOrDefault(_ => _.DocCode == r.Item1 && _.Code == r.Item2))
                                 .Where(ritem => ritem != null))
                    {
                        Document.Facts.Remove(ritem);
                    }
                    UpdateVisualData();
                    return;
                case MessageBoxResult.No:
                    return;
            }
        }

        public ICommand OpenStoreLinkDocumentCommand
        {
            get { return new Command(OpenStoreLinkDocument, _ => CurrentFact != null); }
        }

        private void OpenStoreLinkDocument(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentFact.DocCode);
        }

        public ICommand OpenPayDocumentCommand
        {
            get { return new Command(OpenPayDocument, _ => CurrentPaymentDoc != null); }
        }

        private void OpenPayDocument(object obj)
        {
            if (CurrentPaymentDoc.CashDC != null)
                DocumentsOpenManager.Open(DocumentType.CashOut, (decimal)CurrentPaymentDoc.CashDC);
            if (CurrentPaymentDoc.BankCode != null)
                DocumentsOpenManager.Open(DocumentType.Bank, (decimal)CurrentPaymentDoc.BankCode);
            if (CurrentPaymentDoc.VZDC != null)
                DocumentsOpenManager.Open(DocumentType.MutualAccounting, (decimal)CurrentPaymentDoc.VZDC);
        }

        public override void RefreshData(object obj)
        {
            var WinManager = new WindowManager();
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
                        Document.DeletedRows.Clear();
                        return;
                    case MessageBoxResult.No:
                        foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        Document.LoadReferences();
                        RaiseAll();

                        foreach (var r in Document.Rows.Cast<InvoiceProviderRow>())
                        {
                            r.myState = RowStatus.NotEdited;
                            AddUsedNomenkl(r.Nomenkl.DocCode);
                        }

                        Document.myState = RowStatus.NotEdited;
                        Document.DeletedRows.Clear();
                        return;
                }
            }

            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            var crsrates = new CurrencyRates(DateTime.Today, DateTime.Today);
            var rate = Math.Round(crsrates.GetRate(Document.Currency.DocCode,
                GlobalOptions.SystemProfile.NationalCurrency.DocCode, DateTime.Today), 4);
            Document.PaymentDocs.Clear();
            foreach (var p in UnitOfWork.Context.ProviderInvoicePay
                         .Include(_ => _.TD_101)
                         .Include(_ => _.TD_101.SD_101)
                         .Include(_ => _.TD_110)
                         .Include(_ => _.TD_110.SD_110)
                         .Include(_ => _.SD_34)
                         .Where(_ => _.DocDC == Document.DocCode))
                Document.PaymentDocs.Add(new ProviderInvoicePayViewModel(p));
            if (Document.PaymentDocs.Count > 0)
                rate = Document.PaymentDocs.Sum(_ => _.Summa * _.Rate) / Document.PaymentDocs.Sum(_ => _.Summa);

            Document.Facts.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var facts = ctx.TD_24.Include(_ => _.TD_26)
                    .Include(_ => _.SD_24)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(_ => _.DDT_SPOST_DC == Document.DocCode)
                    .AsNoTracking().ToList();
                foreach (var fact in facts)
                    Document.Facts.Add(new WarehouseOrderInRow(fact)
                    {
                        State = RowStatus.NotEdited
                    });
            }

            Document.SummaFact = 0;
            foreach (var f in Document.Facts)
            {
                var r = Document.Rows.First(_ => _.Nomenkl.DocCode == f.Nomenkl.DocCode);
                Document.SummaFact += Math.Round(r.Summa * r.Quantity / f.DDT_KOL_PRIHOD, 2);
            }

            foreach (var r in Document.Rows.Cast<InvoiceProviderRow>())
            {
                foreach (var cr in r.CurrencyConvertRows)
                {
                    cr.Rate = rate;
                    cr.myState = RowStatus.NotEdited;
                }

                r.myState = RowStatus.NotEdited;
                AddUsedNomenkl(r.Nomenkl.DocCode);
                RaiseAll();
            }

            Document.DeletedRows.Clear();
            Document.myState = RowStatus.NotEdited;
        }

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

        public ICommand UslugaAddCommand
        {
            get { return new Command(UslugaAdd, _ => true); }
        }

        private void UslugaAdd(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(null, true);
            if (k != null)
                foreach (var item in k)
                {
                    if (Document.Rows.Cast<InvoiceProviderRow>()
                        .Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.DefaultNDSPercent == null)
                        nds = 0;
                    else
                        nds = (decimal)item.DefaultNDSPercent;
                    var r = new InvoiceProviderRow
                    {
                        DocCode = -1,
                        NDSPercent = nds,
                        Quantity = 1,
                        Price = 0,
                        Entity = { SFT_NEMENKL_DC = item.DocCode }
                    };
                    Document.Rows.Add(r);
                }

            UpdateVisualData();
        }

        private void UpdateVisualData()
        {
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("SF_CRS_SUMMA");
            // ReSharper disable once PossibleInvalidOperationException
            Document.SummaFact = 0;
            foreach (var r in Document.Rows)
            {
                var q = Document.Facts.Where(_ => _.DDT_SPOST_DC == r.DocCode
                                                  && _.DDT_SPOST_ROW_CODE == r.Code).Sum(_ => _.DDT_KOL_PRIHOD);
                // ReSharper disable once PossibleInvalidOperationException
                Document.SummaFact += r.Summa / r.Quantity * q;
            }

            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("Summa");
            // ReSharper disable once InvertIf
            if (Form is InvoiceClientView frm)
            {
                frm.gridRows.RefreshData();
                RaisePropertyChanged(nameof(Document));
            }
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                return new Command(DeleteRow,
                    _ => CurrentRow != null && (CurrentRow.CurrencyConvertRows == null ||
                                                CurrentRow.CurrencyConvertRows.Count == 0));
            }
        }

        private void DeleteRow(object obj)
        {
            if (CurrentRow != null && CurrentRow.State != RowStatus.NewRow) Document.DeletedRows.Add(CurrentRow);
            var facts = Document.Facts.Where(_ => _.DDT_SPOST_DC == CurrentRow.DocCode
                                                  && _.DDT_SPOST_ROW_CODE == CurrentRow.Code).ToList();
            if (facts.Any())
                foreach (var f in facts)
                    Document.Facts.Remove(f);

            if (CurrentRow != null)
            {
                var distrLink = UnitOfWork.Context.DistributeNakladRow.Where(_ => _.TovarInvoiceRowId == CurrentRow.Id)
                    .ToList();
                foreach (var r in distrLink) UnitOfWork.Context.DistributeNakladRow.Remove(r);
                Document.Entity.TD_26.Remove(CurrentRow.Entity);
                Document.Rows.Remove(CurrentRow);
            }

            UpdateVisualData();
        }

        public ICommand AddNomenklCrsConvertCommand
        {
            get
            {
                return new Command(AddNomenklCrsConvert, _ => CurrentRow != null &&
                                                              CurrentRow?.State != RowStatus.NewRow
                                                              && CurrentRow?.Nomenkl?.IsCurrencyTransfer == true);
            }
        }

        private void AddNomenklCrsConvert(object obj)
        {
            //if (CurrentRow.CurrencyConvertRows.Any(_ => _.State == RowStatus.Edited))
            //{
            //    myWManager.ShowWinUIMessageBox(
            //        "В валютную таксировку внесены изменения. Небходимо сохранение.",
            //        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            var factnom = Document.Facts
                .FirstOrDefault(_ => _.DDT_NOMENKL_DC == CurrentRow.Nomenkl.DocCode);
            if (factnom == null)
            {
                myWManager.ShowWinUIMessageBox(
                    "Товар по этому счету не принят на склад, валютная таксировка не возможна.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //GlobalOptions.ReferencesCache.UpdateNomenklForMain(CurrentRow.Nomenkl.MainId);
            var noms = GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>().Where(_ =>
                    _.MainId == CurrentRow.Nomenkl.MainId
                    && ((IDocCode)_.Currency).DocCode !=
                    ((IDocCode)CurrentRow.Nomenkl.Currency).DocCode)
                .ToList();
            if (noms.Count == 0) return;
            Nomenkl n;
            if (noms.Count > 1)
            {
                var ctx = new NomenklSlectForCurrencyConvertViewModel
                {
                    ItemsCollection = new ObservableCollection<Nomenkl>(noms)
                };
                var dlg = new SelectDialogView { DataContext = ctx };
                ctx.Form = dlg;
                dlg.ShowDialog();
                n = !ctx.DialogResult ? null : ctx.CurrentItem;
            }
            else
            {
                n = noms[0];
            }

            if (n == null) return;
            AddUsedNomenkl(n.DocCode);
            DateTime dt;
            //factnom = Document.Facts
            //    .FirstOrDefault(_ => _.DDT_NOMENKL_DC == CurrentRow.Nomenkl.DocCode);
            //if (factnom == null)
            //{
            //    myWManager.ShowWinUIMessageBox(
            //        "Товар по этому счету не принят на склад, валютная таксировка не возможна.",
            //        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            var crsrates = new CurrencyRates(Document.DocDate <= factnom.SD_24.DD_DATE
                ? Document.DocDate.AddDays(-5)
                : factnom.SD_24.DD_DATE.AddDays(-5), DateTime.Today);

            var store = UnitOfWork.Context.SD_24.FirstOrDefault(_ => _.DOC_CODE == factnom.DOC_CODE);
            if (store == null) return;
            var oldQuan = CurrentRow.CurrencyConvertRows.Count == 0
                ? 0
                : CurrentRow.CurrencyConvertRows.Sum(_ => _.Quantity);
            dt = UnitOfWork.Context.SD_24.Where(_ => _.DOC_CODE == factnom.DOC_CODE).OrderBy(_ => _.DD_DATE).First()
                .DD_DATE;
            var rate = Math.Round(crsrates.GetRate(((IDocCode)CurrentRow.Nomenkl.Currency).DocCode,
                ((IDocCode)n.Currency).DocCode, dt), 4);
            if (Document.PaymentDocs.Count > 0)
                rate = Document.PaymentDocs.Sum(_ => _.Summa * _.Rate) / Document.PaymentDocs.Sum(_ => _.Summa);

            // Новый алгоритм
            var prihod = (from t in UnitOfWork.Context.TD_24.Where(_ => _.DDT_SPOST_DC == Document.DocCode 
                                                                        && _.DDT_NOMENKL_DC == CurrentRow.Nomenkl.DocCode)
                join s in UnitOfWork.Context.SD_24 on t.DOC_CODE equals s.DOC_CODE
                select new
                {
                    Date = s.DD_DATE,
                    SkladDC = s.DD_SKLAD_POL_DC,
                    Quantity = t.DDT_KOL_PRIHOD
                }).ToList();

            var currConv = (from c in CurrentRow.CurrencyConvertRows
                select new
                {
                    c.Id,
                    SkladDC = c.StoreDC,
                    c.Quantity
                }).ToList();

            var conv = from t in UnitOfWork.Context.TD_26_CurrencyConvert.Where(_ => _.DOC_CODE == Document.DocCode)
                select new
                {
                    t.Id,
                    SkladDC = t.StoreDC,
                    t.Quantity
                };

            var isAllOut = true;
            foreach (decimal sklDC in prihod.Select(_ => _.SkladDC).Distinct())
            {
                var q = prihod.Where(_ => _.SkladDC == sklDC).Sum(_ => _.Quantity) -
                        currConv.Where(_ => _.SkladDC == sklDC).Sum(_ => _.Quantity);
                if (q > 0)
                {
                    var newItem = new InvoiceProviderRowCurrencyConvertViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocCode = CurrentRow.DocCode,
                        Code = CurrentRow.Code,
                        NomenklId = n.Id,
                        Date = dt,
                        // ReSharper disable once PossibleInvalidOperationException
                        OLdPrice = CurrentRow.Price,
                        OLdNakladPrice = Math.Round(CurrentRow.Price +
                                                    // ReSharper disable once PossibleInvalidOperationException
                                                    (CurrentRow.SummaNaklad ?? 0) / CurrentRow.Quantity,
                            2),
                        Quantity = q,
                        Rate = rate,
                        // ReSharper disable once PossibleInvalidOperationException
                        StoreDC = sklDC
                    };
                    CurrentRow.Entity.TD_26_CurrencyConvert.Add(newItem.Entity);
                    newItem.CalcRow(DirectCalc.Rate);
                    CurrentRow.CurrencyConvertRows.Add(newItem);
                    CurrentRow.State = RowStatus.Edited;
                    isAllOut = false;
                }
            }

            if (isAllOut)
                myWManager.ShowWinUIMessageBox(
                    "Все поступившие товары сконвертированы.",
                    "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public InvoiceProviderRowCurrencyConvertViewModel CurrentCrsConvertItem
        {
            get => myCurrentCrsConvertItem;
            set
            {
                if (myCurrentCrsConvertItem == value) return;
                myCurrentCrsConvertItem = value;
                RaisePropertyChanged();
            }
        }

        public ICommand DeleteNomenklCrsConvertCommand
        {
            get { return new Command(DeleteNomenklCrsConvert, _ => CurrentCrsConvertItem != null); }
        }

        private void DeleteNomenklCrsConvert(object obj)
        {
            var row = Document.Rows.Cast<InvoiceProviderRow>().FirstOrDefault(_ =>
                _.DocCode == CurrentCrsConvertItem.DocCode
                && _.Code == CurrentCrsConvertItem.Code);
            if (CurrentCrsConvertItem == null || row == null) return;
            DeletedCrsConvertItems.Add(CurrentCrsConvertItem);
            if (CurrentRow == null)
            {
                row.Entity.TD_26_CurrencyConvert.Remove(CurrentCrsConvertItem.Entity);
                row.CurrencyConvertRows.Remove(CurrentCrsConvertItem);
                row.State = RowStatus.Edited;
            }
            else
            {
                CurrentRow.Entity.TD_26_CurrencyConvert.Remove(CurrentCrsConvertItem.Entity);
                CurrentRow.CurrencyConvertRows.Remove(CurrentCrsConvertItem);
                CurrentRow.State = RowStatus.Edited;
            }
        }

        public ICommand AddNomenklSimpleCommand
        {
            get { return new Command(AddNomenklSimple, _ => Document?.Currency != null); }
        }

        private IEnumerable<Nomenkl> LoadNomenkl(string srchText)
        {
            return GlobalOptions.ReferencesCache.GetNomenklsAll()
                .Where(_ => ((IDocCode)_.Currency).DocCode == Document.Currency.DocCode).Cast<Nomenkl>().Where(_ =>
                    (_.Name + _.NomenklNumber + _.FullName).ToUpper().Contains(srchText.ToUpper()));
        }


        private void AddNomenklSimple(object obj)
        {
            decimal defaultNDS;
            var dtx = new TableSearchWindowViewMove<Nomenkl>(LoadNomenkl, "Выбор номенклатур", "NomenklSipleListView");
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Выбор номенклатур", dtx) == MessageResult.OK
                || dtx.DialogResult)
            {
                using (var entctx = GlobalOptions.GetEntities())
                {
                    defaultNDS = Convert.ToDecimal(entctx.PROFILE
                        .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
                }

                var cr = new CurrencyRates(Document.DocDate, Document.DocDate);
                var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var n in dtx.SelectedItems)
                {
                    if (Document.Rows.Any(_ => _.Nomenkl.DocCode == n.DocCode)) continue;
                    AddUsedNomenkl(n.DocCode);
                    var newRow = new InvoiceProviderRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n,
                        Quantity = 1,
                        Price = 0,
                        NDSPercent = n.DefaultNDSPercent ?? defaultNDS,
                        PostUnit = (Unit)n.Unit,
                        UchUnit = (Unit)n.Unit,
                        Note = " ",
                        State = RowStatus.NewRow,
                        IsIncludeInPrice = Document.IsNDSInPrice,
                        Parent = Document
                    };
                    newRow.Entity.SFT_POST_ED_IZM_DC = ((IDocCode)n.Unit).DocCode;
                    switch (Document.Currency.DocCode)
                    {
                        case CurrencyCode.EUR:
                            newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.EUR,
                                Document.DocDate);
                            newRow.EURSumma = newRow.EURRate;
                            break;
                        case CurrencyCode.USD:
                            newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.USD,
                                Document.DocDate);
                            newRow.EURSumma = newRow.USDRate;
                            break;
                        case CurrencyCode.RUB:
                            newRow.RUBRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.RUB,
                                Document.DocDate);
                            newRow.RUBSumma = newRow.RUBRate;
                            break;
                        case CurrencyCode.GBP:
                            newRow.GBPRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.GBP,
                                Document.DocDate);
                            newRow.GBPSumma = newRow.GBPRate;
                            break;
                    }

                    Document.Entity.TD_26.Add(newRow.Entity);
                    Document.Rows.Add(newRow);
                    newCode++;
                }
            }
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document?.Currency != null); }
        }

        private void AddNomenkl(object obj)
        {
            decimal defaultNDS;
            var nomenkls = StandartDialogs.SelectNomenkls(Document.Kontragent?.Currency as Currency, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            using (var entctx = GlobalOptions.GetEntities())
            {
                defaultNDS = Convert.ToDecimal(entctx.PROFILE
                    .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
            }

            var cr = new CurrencyRates(Document.DocDate, Document.DocDate);
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var n in nomenkls.Where(_ => Document.Rows.All(t => t.DocCode != _.DocCode)))
            {
                AddUsedNomenkl(n.DocCode);
                var newRow = new InvoiceProviderRow
                {
                    DocCode = Document.DocCode,
                    Code = newCode,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Nomenkl = n,
                    Quantity = 1,
                    Price = 0,
                    NDSPercent = n.DefaultNDSPercent ?? defaultNDS,
                    PostUnit = (Unit)n.Unit,
                    UchUnit = (Unit)n.Unit,
                    Note = " ",
                    State = RowStatus.NewRow,
                    IsIncludeInPrice = Document.IsNDSInPrice,
                    Parent = Document
                };
                newRow.Entity.SFT_POST_ED_IZM_DC = ((IDocCode)n.Unit).DocCode;
                switch (Document.Currency.DocCode)
                {
                    case CurrencyCode.EUR:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.EUR,
                            Document.DocDate);
                        newRow.EURSumma = newRow.EURRate;
                        break;
                    case CurrencyCode.USD:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.USD,
                            Document.DocDate);
                        newRow.EURSumma = newRow.USDRate;
                        break;
                    case CurrencyCode.RUB:
                        newRow.RUBRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.RUB,
                            Document.DocDate);
                        newRow.RUBSumma = newRow.RUBRate;
                        break;
                    case CurrencyCode.GBP:
                        newRow.GBPRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.GBP,
                            Document.DocDate);
                        newRow.GBPSumma = newRow.GBPRate;
                        break;
                }

                Document.Entity.TD_26.Add(newRow.Entity);
                Document.Rows.Add(newRow);
                newCode++;
            }

            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        public Command AddUslugaCommand
        {
            get { return new Command(AddUsluga, _ => Document?.Currency != null); }
        }

        private void AddUsluga(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(Document.Currency);
            if (k != null)
            {
                var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var item in k)
                {
                    if (Document.Rows.Cast<InvoiceProviderRow>()
                        .Any(_ => _.Entity.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.DefaultNDSPercent == null)
                        nds = 0;
                    else
                        nds = (decimal)item.DefaultNDSPercent;
                    var newRow = new InvoiceProviderRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.DocCode) as Nomenkl,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        NDSPercent = nds,
                        Quantity = 1,
                        PostUnit = (Unit)item.Unit,
                        UchUnit = (Unit)item.Unit,
                        State = RowStatus.NewRow,
                        Note = " ",
                        IsIncludeInPrice = Document.IsNDSInPrice,
                        Parent = Document,
                        Shipped = 1
                        
                    };
                    if (Document.IsNDSInPrice)
                        newRow.SFT_SUMMA_K_OPLATE = 0;
                    else
                        newRow.Price = 0;
                    newRow.Entity.SFT_NEMENKL_DC = item.DocCode;
                    newRow.Entity.SFT_POST_ED_IZM_DC = ((IDocCode)item.Unit).DocCode;
                    Document.Rows.Add(newRow);
                    Document.Entity.TD_26.Add(newRow.Entity);
                    newCode++;
                }
            }

            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        public ICommand UpdateCalcRowSummaCommand
        {
            get { return new Command(UpdateCalcRowSumma, _ => true); }
        }

        private void UpdateCalcRowSumma(object obj)
        {
            //CurrentRow?.CalcRow();
            if (Form is InvoiceProviderView frm)
            {
                frm.gridRows.RefreshData();
                frm.gridRows.UpdateTotalSummary();
            }
        }

        public override void SaveData(object data)
        {
            var WinManager = new WindowManager();

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (InvoiceProviderRow r in Document.Rows)
            {
                var facts = Document.Facts.Where(_ => _.DDT_NOMENKL_DC == r.Nomenkl.DocCode).ToList();
                if (facts.Sum(_ => _.DDT_KOL_PRIHOD) < r.CurrencyConvertRows.Sum(_ => _.Quantity))
                {
                    myWManager.ShowWinUIMessageBox(
                        $"По номенклатуре {r.Nomenkl.Name} ({r.Nomenkl.NomenklNumber}) " +
                        $"приход {facts.Sum(_ => _.DDT_KOL_PRIHOD):n4} " +
                        $"меньше, чем валютной таксировки {r.CurrencyConvertRows.Sum(_ => _.Quantity):n4}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

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
                isOldExist = ctx.SD_26.Any(_ => _.DOC_CODE == Document.DocCode);
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
                isCreateNum = false;
            }

            foreach (var entry in UnitOfWork.Context.ChangeTracker.Entries())
            {
                if ((entry.Entity is SD_101 || entry.Entity is TD_101 || entry.Entity is SD_114) &&
                    entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Unchanged;
                }
            }

            UnitOfWork.CreateTransaction();
            try
            {
                if (Document.State == RowStatus.NewRow || Document.DocCode < 0)
                    if (isCreateNum)
                    {
                        Document.DocCode = UnitOfWork.Context.SD_26.Any()
                            ? UnitOfWork.Context.SD_26.Max(_ => _.DOC_CODE) + 1
                            : 10260000001;
                        foreach (var row in Document.Rows) row.DocCode = Document.DocCode;

                        Document.SF_IN_NUM = UnitOfWork.Context.SD_26.Any()
                            ? UnitOfWork.Context.SD_26.Max(_ => _.SF_IN_NUM) + 1
                            : 1;
                    }

                if (Document.SF_CRS_RATE == null) Document.SF_CRS_RATE = 1;

                if (Document.SF_KONTR_CRS_RATE == null) Document.SF_KONTR_CRS_RATE = 1;

                if (Document.SF_UCHET_VALUTA_RATE == null) Document.SF_UCHET_VALUTA_RATE = 1;
                if (Document.SF_SUMMA_S_NDS == null) Document.SF_SUMMA_S_NDS = (short)(Document.IsNDSInPrice ? 1 : 0);
                if (Document.SF_SCHET_FACT_FLAG == null) Document.SF_SCHET_FACT_FLAG = 1;
                if (Document.SF_KONTR_CRS_DC == null)
                    Document.SF_KONTR_CRS_DC = ((IDocCode)Document.Kontragent.Currency).DocCode;

                if (Document.SF_KONTR_CRS_SUMMA == null) Document.SF_KONTR_CRS_SUMMA = Document.Summa;

                foreach (var row in Document.Rows.Cast<InvoiceProviderRow>())
                {
                    if (row.SFT_SUMMA_V_UCHET_VALUTE == null) row.SFT_SUMMA_V_UCHET_VALUTE = row.SFT_SUMMA_K_OPLATE;

                    if (row.SFT_SUMMA_K_OPLATE_KONTR_CRS == null)
                        row.SFT_SUMMA_K_OPLATE_KONTR_CRS = row.SFT_SUMMA_K_OPLATE;

                    if (row.SFT_NOM_CRS_RATE == null) row.SFT_NOM_CRS_RATE = 1;
                }

                var DistributeDocs = new List<Guid>();
                foreach (var crsitem in DeletedCrsConvertItems)
                {
                    var olditems = UnitOfWork.Context.DistributeNakladRow
                        .Include(_ => _.DistributeNakladInfo)
                        .Where(_ => _.TransferRowId == crsitem.Id).ToList();
                    foreach (var old in olditems)
                    {
                        if (DistributeDocs.All(_ => _ != old.DocId))
                            DistributeDocs.Add(old.DocId);
                        UnitOfWork.Context.DistributeNakladInfo.RemoveRange(old.DistributeNakladInfo);
                        UnitOfWork.Context.DistributeNakladRow.Remove(old);
                    }
                }

                foreach (var row in Document.Rows.Cast<InvoiceProviderRow>())
                foreach (var crs in row.CurrencyConvertRows)
                {
                    var distr = UnitOfWork.Context.DistributeNakladRow
                        .Include(_ => _.DistributeNakladInfo)
                        .FirstOrDefault(_ => _.TransferRowId == crs.Id);
                    if (distr != null)
                        if (DistributeDocs.All(_ => _ != distr.DocId))
                            DistributeDocs.Add(distr.DocId);
                }

                foreach (var id in DistributeDocs)
                {
                    var doc = UnitOfWork.Context.DistributeNaklad.FirstOrDefault(_ => _.Id == id);
                    if (doc != null)
                    {
                        var vm = new DistributeNakladViewModel(doc);
                        vm.Load(doc.Id);
                        vm.RecalcAllResult();
                        vm.Save();
                    }
                }

                foreach (var row in DeletedStoreLink)
                {
                    var d = UnitOfWork.Context.TD_24.FirstOrDefault(_ =>
                        _.DOC_CODE == row.Item1 && _.CODE == row.Item2);
                    if (d != null )
                        UnitOfWork.Context.TD_24.Remove(d);
                }
                UnitOfWork.Save();
                UnitOfWork.Commit();
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceProvider), null,
                    Document.DocCode, null, (string)Document.ToJson());
                RecalcKontragentBalans.CalcBalans(Document.Entity.SF_POST_DC, Document.DocDate);
                nomenklManager.RecalcPrice(myUsedNomenklsDC);
                myUsedNomenklsDC.Clear();
                foreach (var r in Document.Rows.Cast<InvoiceProviderRow>())
                {
                    r.myState = RowStatus.NotEdited;
                    foreach (var rr in r.CurrencyConvertRows) rr.myState = RowStatus.NotEdited;
                }

                foreach (var f in Document.Facts) f.myState = RowStatus.NotEdited;

                foreach (var p in Document.PaymentDocs) p.State = RowStatus.NotEdited;
                Document.myState = RowStatus.NotEdited;
                // ReSharper disable once UseNameofExpression
                Document.RaisePropertyChanged("State");
                RaisePropertyChanged(nameof(WindowName));
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.InvoiceProvider, Document.Id, Document.DocCode,
                    Document.CREATOR,
                    "", Document.Description);
                DeletedStoreLink.Clear();
                Document.DeletedRows.Clear();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        private void RaiseAll()
        {
            Document.RaisePropertyAllChanged();
            foreach (var r in Document.Rows.Cast<InvoiceProviderRow>()) r.RaisePropertyAllChanged();

            foreach (var s in Document.Facts) s.RaisePropertyAllChanged();
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var ctx = new ProviderWindowViewModel(Document.DocCode);
            ctx.SetAsNewCopy(true);
            var frm = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var ctx = new ProviderWindowViewModel(Document.DocCode);
            ctx.SetAsNewCopy(false);
            var frm = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocDelete(object form)
        {
            var WinManager = new WindowManager();

            if (Document.State == RowStatus.NewRow)
            {
                var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        Form.Close();
                        return;
                    case MessageBoxResult.No:
                        return;
                }
            }
            else
            {
                var isDistr = UnitOfWork.Context.DistributeNakladInfo.Any(_ => _.InvoiceNakladId == Document.Id);
                var isRowDistr = Document.Rows.Any(r =>
                    UnitOfWork.Context.DistributeNakladRow.Any(_ => _.TovarInvoiceRowId == r.Id));
                var str = !isDistr && !isRowDistr
                    ? "Вы уверены, что хотите удалить данный документ?"
                    : "Документ участвует в распределении накладных.\nВы уверены, что хотите удалить данный документ?";
                var res = WinManager.ShowWinUIMessageBox(str, "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        var dc = Document.DocCode;
                        var docdate = Document.DocDate;
                        var DistributeDocs = new List<Guid>();
                        UnitOfWork.CreateTransaction();
                        try
                        {
                            if (isDistr)
                            {
                                foreach (var d in UnitOfWork.Context.DistributeNakladInfo
                                             .Where(_ => _.InvoiceNakladId == Document.Id).ToList())
                                    UnitOfWork.Context.DistributeNakladInfo.Remove(d);

                                foreach (var din in UnitOfWork.Context.DistributeNakladInvoices
                                             .Where(_ => _.InvoiceId == Document.Id))
                                    UnitOfWork.Context.DistributeNakladInvoices.Remove(din);
                            }


                            if (isRowDistr)
                                foreach (var row in Document.Rows.Cast<InvoiceProviderRow>())
                                {
                                    foreach (var d2 in row.Entity.TD_26_CurrencyConvert)
                                    {
                                        foreach (var d1 in d2.DistributeNakladRow)
                                        {
                                            foreach (var irow in d1.DistributeNakladInfo)
                                                UnitOfWork.Context.DistributeNakladInfo.Remove(irow);
                                            UnitOfWork.Context.DistributeNakladRow.Remove(d1);
                                        }

                                        UnitOfWork.Context.TD_26_CurrencyConvert.Remove(d2);
                                    }

                                    foreach (var d1 in UnitOfWork.Context.DistributeNakladRow.Where(_ =>
                                                 _.TovarInvoiceRowId == row.Id).ToList())
                                    {
                                        foreach (var irow in UnitOfWork.Context.DistributeNakladInfo.Where(_ =>
                                                     _.RowId == d1.Id))
                                            UnitOfWork.Context.DistributeNakladInfo.Remove(irow);

                                        UnitOfWork.Context.DistributeNakladRow.Remove(d1);
                                        DistributeDocs.Add(row.DocId);
                                    }
                                }

                            var pays = new List<ProviderInvoicePay>();
                            foreach (var pay in Document.Entity.ProviderInvoicePay)
                            {
                                if (pay.SD_34 != null)
                                {
                                    pay.SD_34.SPOST_DC = null;
                                    pay.SD_34.SPOST_CRS_DC = null;
                                    pay.SD_34.SPOST_CRS_RATE = null;
                                    pay.SD_34.SPOST_OPLACHENO = null;
                                }

                                if (pay.TD_101 != null) pay.TD_101.VVT_SFACT_POSTAV_DC = null;

                                if (pay.TD_110 != null) pay.TD_110.VZT_SPOST_DC = null;
                                pays.Add(pay);
                            }

                            foreach (var p in pays)
                                UnitOfWork.Context.ProviderInvoicePay.Remove(p);
                            GenericProviderRepository.Delete(Document.Entity);
                            UnitOfWork.Save();
                            foreach (var id in DistributeDocs)
                            {
                                var doc = UnitOfWork.Context.DistributeNaklad.FirstOrDefault(_ => _.Id == id);
                                if (doc == null) continue;
                                var vm = new DistributeNakladViewModel(doc);
                                vm.Load(doc.Id);
                                vm.RecalcAllResult();
                                vm.Save();
                            }

                            UnitOfWork.Commit();
                            DocumentsOpenManager.DeleteFromLastDocument(null, Document.DocCode);
                        }
                        catch (Exception ex)
                        {
                            UnitOfWork.Rollback();
                            WindowManager.ShowError(ex);
                            return;
                        }

                        Form.Close();
                        RecalcKontragentBalans.CalcBalans(dc, docdate);
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageBoxResult.No:
                        Form.Close();
                        return;
                }
            }
        }

        public override void DocNewEmpty(object form)
        {
            var ctx = new ProviderWindowViewModel(null);
            var view = new InvoiceProviderView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Document.IsNDSInPrice = true;
            ctx.Form = view;
            view.Show();
        }

        // ReSharper disable once UnusedMember.Global
        public ICommand DeletePaymentDocumentCommand
        {
            get
            {
                return new Command(DeletePayDocument,
                    _ => CurrentPaymentDoc != null);
            }
        }

        public void DeletePayDocument(object obj)
        {
            var WinManager = new WindowManager();
            var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить связь с оплатой?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            switch (CurrentPaymentDoc.DocName)
            {
                case "Банковская транзакция":
                    var old = UnitOfWork.Context.TD_101.SingleOrDefault(_ =>
                        _.CODE == CurrentPaymentDoc.BankCode);
                    if (old != null) old.VVT_SFACT_POSTAV_DC = null;
                    break;
                case "Расходный кассовый ордер":
                    var old1 = UnitOfWork.Context.SD_34.SingleOrDefault(_ =>
                        _.DOC_CODE == CurrentPaymentDoc.CashDC);
                    if (old1 != null)
                    {
                        old1.SPOST_DC = null;
                        old1.SPOST_CRS_DC = null;
                    }

                    break;
                case "Акт взаимозачета":
                    var old2 = UnitOfWork.Context.TD_110.SingleOrDefault(_ =>
                        _.DOC_CODE == CurrentPaymentDoc.VZDC
                        && _.CODE == CurrentPaymentDoc.VZCode);
                    if (old2 != null)
                        UnitOfWork.Context.TD_110.Remove(old2);
                    break;
            }

            UnitOfWork.Context.ProviderInvoicePay.Remove(CurrentPaymentDoc.Entity);
            Document.PaymentDocs.Remove(CurrentPaymentDoc);
            SaveData(null);
            UnitOfWork.Context.Database.ExecuteSqlCommand(
                $"EXEC dbo.GenerateSFProviderCash {CustomFormat.DecimalToSqlDecimal(Document.DocCode)}");
            // ReSharper disable once StringLiteralTypo
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("PaySumma");
        }

        public ICommand AddPaymentFromBankCommand
        {
            get
            {
                return new Command(AddPaymentFromBank,
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.Summa
                                                      && Document.State != RowStatus.NewRow);
            }
        }

        private void AddPaymentFromBank(object obj)
        {
            try
            {
                var oper = StandartDialogs.SelectBankOperationForProviderInvoice(Document.Kontragent.DocCode);
                if (oper == null) return;
                if (Document.PaymentDocs.Any(_ => _.BankCode == oper.Code)) return;

                using (var ctx = GlobalOptions.GetEntities())
                {
                    var old = ctx.TD_101
                        .Include(_ => _.SD_101)
                        .Include(_ => _.SD_101.SD_114)
                        .AsNoTracking()
                        .Single(_ => _.CODE == oper.Code);
                    if (old != null) old.VVT_SFACT_POSTAV_DC = Document.DocCode;
                    else return;

                    var newItem = new ProviderInvoicePayViewModel
                    {
                        BankCode = old.CODE,
                        myState = RowStatus.NewRow,
                        Summa = Document.Summa - Document.PaySumma >= oper.Remainder
                            ? oper.Remainder
                            : Document.Summa - Document.PaySumma,
                        // ReSharper disable once PossibleInvalidOperationException
                        Rate = old.CurrencyRateForReference ?? 1,
                        TD_101 = old
                    };
                    Document.PaymentDocs.Add(newItem);
                    Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                    ctx.SaveChanges();
                }

                // ReSharper disable once NotResolvedInText
                Document.RaisePropertyChanged("PaySumma");
                if (Document.State != RowStatus.NewRow)
                    Document.State = RowStatus.Edited;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public ICommand AddPaymentFromCashCommand
        {
            get
            {
                return new Command(AddPaymentFromCash,
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.Summa
                                                      && Document.State != RowStatus.NewRow);
            }
        }

        private void AddPaymentFromCash(object obj)
        {
            var oper = StandartDialogs.SelectCashOperationForProviderInvoice(Document.Kontragent.DocCode);
            if (oper == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var old = ctx.SD_34
                        .Include(_ => _.SD_22)
                        .Single(_ => _.DOC_CODE == oper.DocCode);
                    if (old != null)
                    {
                        old.SPOST_DC = Document.DocCode;
                        old.SPOST_CRS_DC = Document.Currency.DocCode;
                        old.SPOST_CRS_RATE = 1;
                    }

                    ctx.SaveChanges();

                    if (old != null)
                    {
                        var newItem = new ProviderInvoicePayViewModel
                        {
                            CashDC = old.DOC_CODE,
                            myState = RowStatus.NewRow,
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = Document.Summa - Document.PaySumma >= (decimal)old.SUMM_ORD
                                ? (decimal)old.SUMM_ORD
                                : Document.Summa - Document.PaySumma,
                            // ReSharper disable once PossibleInvalidOperationException
                            Rate = 1
                            // ReSharper disable once PossibleInvalidOperationException
                        };
                        Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                        Document.PaymentDocs.Add(newItem);
                    }

                    if (Document.State != RowStatus.NewRow)
                        Document.State = RowStatus.Edited;
                    // ReSharper disable once NotResolvedInText
                    Document.RaisePropertyChanged("PaySumma");
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
        }

        public ICommand AddPaymentFromVZCommand
        {
            get
            {
                return new Command(AddPaymentFromVZ,
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.Summa);
            }
        }

        private void AddPaymentFromVZ(object obj)
        {
            var oper = StandartDialogs.SelectVZOperationForProviderInvoice(Document.Kontragent.DocCode);
            if (oper == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var old = ctx.TD_110
                    .Include(_ => _.SD_110)
                    .Single(_ =>
                        _.DOC_CODE == oper.DocCode && _.CODE == oper.Code);
                if (old == null) return;
                old.VZT_SPOST_DC = Document.DocCode;
                ctx.SaveChanges();
                var newItem = new ProviderInvoicePayViewModel
                {
                    VZDC = old.DOC_CODE,
                    VZCode = old.CODE,
                    myState = RowStatus.NewRow,
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = Document.Summa - Document.PaySumma >= (decimal)old.VZT_CRS_SUMMA
                        ? (decimal)old.VZT_CRS_SUMMA
                        : Document.Summa - Document.PaySumma,
                    Rate = 1
                };
                Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                Document.PaymentDocs.Add(newItem);
                if (Document.State != RowStatus.NewRow)
                    Document.State = RowStatus.Edited;
                // ReSharper disable once NotResolvedInText
                Document.RaisePropertyChanged("PaySumma");
            }
        }

        /// <summary>
        ///     удаление зависших связей с платежами
        /// </summary>
        public override void CloseWindow(object form)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        DeletePayments();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            if (Form != null)
            {
                Form.Close();
                return;
            }

            var frm = form as Window;
            frm?.Close();
        }

        public ICommand RestColumnDataCommand
        {
            get { return new Command(RestColumnData, _ => true); }
        }


        private void RestColumnData(object obj)
        {
            if (obj is not UnboundColumnRowArgs args) return;
            if (!args.IsGetData) return;
            var item = (InvoiceProviderRow)args.Item;
            args.Value = item.IsUsluga ? item.Quantity : item.Quantity - (item.Entity.TD_24?.Sum(_ => _.DDT_KOL_PRIHOD) ?? 0);
        }


        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => null;

        public string Error { get; } = null;

        #endregion
    }
}

public class NomenklSlectForCurrencyConvertViewModel : RSWindowViewModelBase
{
    private Nomenkl myCurrentItem;
    private StandartDialogSelectUC myDataUserControl;

    public NomenklSlectForCurrencyConvertViewModel()
    {
        myDataUserControl = new StandartDialogSelectUC(GetType().Name);
        // ReSharper disable once VirtualMemberCallInConstructor
        RefreshData(null);
    }

    #region command

    public override void RefreshData(object o)
    {
        RaisePropertyChanged(nameof(ItemsCollection));
    }

    #endregion

    #region properties

    public ObservableCollection<Nomenkl> ItemsCollection { set; get; } =
        new ObservableCollection<Nomenkl>();

    public StandartDialogSelectUC DataUserControl
    {
        set
        {
            if (Equals(myDataUserControl, value)) return;
            myDataUserControl = value;
            RaisePropertyChanged();
        }
        get => myDataUserControl;
    }

    public Nomenkl CurrentItem
    {
        set
        {
            if (Equals(myCurrentItem, value)) return;
            myCurrentItem = value;
            RaisePropertyChanged();
        }
        get => myCurrentItem;
    }

    #endregion
}
