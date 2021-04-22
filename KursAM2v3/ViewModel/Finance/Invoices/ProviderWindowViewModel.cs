using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Managers.Nomenkl;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Logistiks.UC;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursAM2.ViewModel.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;
using Reports.Base;

namespace KursAM2.ViewModel.Finance.Invoices
{
    /// <summary>
    ///     Сфчет-фактура поставщика
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public sealed class ProviderWindowViewModel : RSWindowViewModelBase
    {
        #region Methods

        private void createReportsMenu()
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

        private void addUsedNomenkl(decimal nomdc)
        {
            if (myUsedNomenklsDC.All(_ => _ != nomdc))
            {
                myUsedNomenklsDC.Add(nomdc);
            }
        }

        #endregion

        #region Fields

        private InvoiceProvider myDocument;
        private InvoiceProviderRow myCurrentRow;
        public readonly GenericKursDBRepository<SD_26> GenericProviderRepository;
        private readonly WindowManager myWManager = new WindowManager();
        private readonly List<decimal> myUsedNomenklsDC = new List<decimal>();

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
            WindowName = "Счет-фактура поставщика";
            createReportsMenu();
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
                    TD_26 = new List<TD_26>()
                };
                UnitOfWork.Context.SD_26.Add(doc);
            }

            Document = new InvoiceProvider(doc, UnitOfWork)
            {
                State = dc == null ? RowStatus.NewRow : RowStatus.NotEdited
            };
            if (Document != null)
                WindowName = Document.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Local
        public ProviderWindowViewModel(Guid id) : this()
        {
        }

        #endregion

        #region Properties

        public List<InvoiceProviderRowCurrencyConvertViewModel> DeletedCrsConvertItems { set; get; } =
            new List<InvoiceProviderRowCurrencyConvertViewModel>();

        public InvoiceProvider Document
        {
            get => myDocument;
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
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
                                              && Document.State != RowStatus.NotEdited;

        private ProviderInvoicePayViewModel myCurrentPaymentDoc;
        private InvoiceProviderRowCurrencyConvertViewModel myCurrentCrsConvertItem;

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

        public ICommand AddStoreLinkCommand
        {
            get { return new Command(addStoreLink, _ => true); }
        }

        private void addStoreLink(object obj)
        {
            var ctx = new AddNomenklFromOrderInViewModel(Document.Kontragent);
            var dlg = new SelectDialogView {DataContext = ctx};
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
                    if (srow != null && old.DDT_KOL_PRIHOD > srow.SFT_KOL) srow.SFT_KOL = old.DDT_KOL_PRIHOD;
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
                        Document.Rows.Add(new InvoiceProviderRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Id = Guid.NewGuid(),
                            DocId = Document.Id,
                            Nomenkl = item.Nomenkl,
                            SFT_KOL = item.Quantity,
                            SFT_ED_CENA = 0,
                            SFT_NDS_PERCENT = item.Nomenkl.NDSPercent ?? defaultNDS,
                            SFT_POST_ED_IZM_DC = item.Nomenkl.Unit.DocCode,
                            PostUnit = item.Nomenkl.Unit,
                            UchUnit = item.Nomenkl.Unit,
                            State = RowStatus.NewRow
                        });
                        var oldOrdRow = GenericProviderRepository.Context.TD_24.Include(_ => _.SD_24).FirstOrDefault(_ =>
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
                        var oldOrdRow = GenericProviderRepository.Context.TD_24.Include(_ => _.SD_24).FirstOrDefault(_ =>
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

                        if (srow.SFT_KOL < item.Quantity)
                            srow.SFT_KOL = item.Quantity;
                    }
                }
            }
            updateVisualData();
        }

        public ICommand DeleteStoreLinkCommand
        {
            get { return new Command(deleteStoreLink, _ => CurrentFact != null); }
        }

        private void deleteStoreLink(object obj)
        {
            var rowForRemove = new List<Tuple<decimal, int>>();
            var res = WinManager.ShowWinUIMessageBox("Действительно хотите удалить связь с приходными ордерами?",
                "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var tran = ctx.Database.BeginTransaction();
                        try
                        {
                            foreach (var item in SelectedFacts)
                            {
                                var ordRows = ctx.TD_24.Where(_ => _.DDT_SPOST_DC == item.DDT_SPOST_DC
                                                                   && _.DDT_SPOST_ROW_CODE == item.DDT_SPOST_ROW_CODE);
                                foreach (var row in ordRows)
                                {
                                    row.DDT_SPOST_DC = null;
                                    row.DDT_SPOST_ROW_CODE = null;
                                    row.DDT_TAX_IN_SFACT = 0;
                                    row.DDT_TAX_EXECUTED = 0;
                                }

                                rowForRemove.Add(new Tuple<decimal, int>(item.DOC_CODE, item.Code));
                            }

                            ctx.SaveChanges();
                            tran.Commit();
                            foreach (var rem in rowForRemove)
                            {
                                var old = Document.Facts.FirstOrDefault(_ => _.DOC_CODE == rem.Item1
                                                                             && _.Code == rem.Item2);
                                if (old != null)
                                    Document.Facts.Remove(old);
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            WindowManager.ShowError(ex);
                        }
                    }

                    updateVisualData();
                    return;
                case MessageBoxResult.No:
                    return;
            }
        }

        public ICommand OpenStoreLinkDocumentCommand
        {
            get { return new Command(openStoreLinkDocument, _ => CurrentFact != null); }
        }

        private void openStoreLinkDocument(object obj)
        {
            var ctx = new OrderInWindowViewModel(
                new StandartErrorManager(GlobalOptions.GetEntities(), "WarehouseOrderIn", true)
                , CurrentFact.DocCode);
            var frm = new OrderInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public ICommand OpenPayDocumentCommand
        {
            get { return new Command(openPayDocument, _ => CurrentPaymentDoc != null); }
        }

        private void openPayDocument(object obj)
        {
            switch (CurrentPaymentDoc.DocName)
            {
                case "Расходный кассовый ордер":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.CashOut, (decimal) CurrentPaymentDoc.CashDC);
                    break;
                case "Банковский платеж":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.Bank, (decimal) CurrentPaymentDoc.BankCode);
                    break;
                case "Акт взаимозачета":
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.MutualAccounting, (decimal) CurrentPaymentDoc.VZDC);
                    break;
            }
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
                        var dc = Document.DocCode;
                        UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
                        var d = GenericProviderRepository.GetById(dc);
                        Document = new InvoiceProvider(d, UnitOfWork);
                        Document.myState = RowStatus.NotEdited;
                        foreach (var r in Document.Rows)
                        {
                            r.myState = RowStatus.NotEdited;
                            foreach (var rr in r.CurrencyConvertRows) 
                                rr.State = RowStatus.NotEdited;
                        }

                        //genericProviderRepository.Refresh(Document.Entity);
                        RaisePropertiesChanged(nameof(Document));
                        RaisePropertiesChanged(nameof(Document.Rows));
                        RaisePropertyChanged(nameof(Document.Facts));
                        RaisePropertyChanged(nameof(Document.PaymentDocs));
                        return;
                }
            }

            Document.myState = RowStatus.NotEdited;
            foreach (var r in Document.Rows)
            {
                r.myState = RowStatus.NotEdited;
                addUsedNomenkl(r.Nomenkl.DocCode);
                foreach (var rr in r.CurrencyConvertRows)
                {
                    rr.State = RowStatus.NotEdited;
                    addUsedNomenkl(rr.Nomenkl.DocCode);
                }
            }

            RaisePropertiesChanged(nameof(Document));
            RaisePropertiesChanged(nameof(Document.Rows));
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

        public ICommand UslugaAddCommand
        {
            get { return new Command(uslugaAdd, _ => true); }
        }

        private void uslugaAdd(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(null, true);
            if (k != null)
                foreach (var item in k)
                {
                    if (Document.Rows.Any(_ => _.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    Document.Rows.Add(new InvoiceProviderRow
                    {
                        DOC_CODE = -1,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = nds,
                        SFT_KOL = 1,
                        SFT_ED_CENA = 0
                    });
                }

            updateVisualData();
        }

        private void updateVisualData()
        {
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("SF_CRS_SUMMA");
            // ReSharper disable once PossibleInvalidOperationException
            Document.SF_CRS_SUMMA = (decimal) (Document.Rows.Any() ? Document.Rows.Sum(_ => _.SFT_SUMMA_K_OPLATE) : 0);
            Document.SummaFact = 0;
            foreach (var r in Document.Rows)
            {
                var q = Document.Facts.Where(_ => _.DDT_SPOST_DC == r.DocCode
                                                  && _.DDT_SPOST_ROW_CODE == r.Code).Sum(_ => _.DDT_KOL_PRIHOD);
                // ReSharper disable once PossibleInvalidOperationException
                Document.SummaFact += (decimal) r.SFT_SUMMA_K_OPLATE / r.SFT_KOL * q;
            }

            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("SF_KONTR_CRS_SUMMA");
            // ReSharper disable once InvertIf
            if (Form is InvoiceClientView frm)
            {
                frm.gridRows.RefreshData();
                RaisePropertiesChanged(nameof(Document));
            }
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                return new Command(deleteRow,
                    _ => CurrentRow != null && (CurrentRow.CurrencyConvertRows == null ||
                                                CurrentRow.CurrencyConvertRows.Count == 0));
            }
        }

        private void deleteRow(object obj)
        {
            if (CurrentRow != null && CurrentRow.State != RowStatus.NewRow) Document.DeletedRows.Add(CurrentRow);
            var facts = Document.Facts.Where(_ => _.DDT_SPOST_DC == CurrentRow.DocCode
                                                  && _.DDT_SPOST_ROW_CODE == CurrentRow.Code).ToList();
            if (facts.Any())
            {
                foreach (var f in facts)
                {
                    Document.Facts.Remove(f);
                }
            }

            if (CurrentRow != null)
            {
                Document.Entity.TD_26.Remove(CurrentRow.Entity);
                Document.Rows.Remove(CurrentRow);
            }

            updateVisualData();
        }

        public ICommand AddNomenklCrsConvertCommand
        {
            get
            {
                return new Command(addNomenklCrsConvert, _ => CurrentRow != null &&
                                                              CurrentRow?.State != RowStatus.NewRow
                                                              && CurrentRow?.Nomenkl?.IsCurrencyTransfer == true);
            }
        }

        private void addNomenklCrsConvert(object obj)
        {
            MainReferences.UpdateNomenklForMain(CurrentRow.Nomenkl.MainId);
            var noms = MainReferences.ALLNomenkls.Values.Where(_ => _.MainId == CurrentRow.Nomenkl.MainId
                                                                    && _.Currency.DocCode !=
                                                                    CurrentRow.Nomenkl.Currency.DocCode).ToList();
            if (noms.Count == 0) return;
            Nomenkl n;
            if (noms.Count > 1)
            {
                var ctx = new NomenklSlectForCurrencyConvertViewModel
                {
                    ItemsCollection = new ObservableCollection<Nomenkl>(noms)
                };
                var dlg = new SelectDialogView {DataContext = ctx};
                ctx.Form = dlg;
                dlg.ShowDialog();
                n = !ctx.DialogResult ? null : ctx.CurrentItem;
            }
            else
            {
                n = noms[0];
            }

            if (n == null) return;
            addUsedNomenkl(n.DocCode);
            DateTime dt;
            var factnom = Document.Facts
                .FirstOrDefault(_ => _.DDT_NOMENKL_DC == CurrentRow.Nomenkl.DocCode);
            if (factnom == null)
            {
                myWManager.ShowWinUIMessageBox(
                    "Товар по этому счету не принят на склад, валютная таксировка не возможна.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var crsrates = new CurrencyRates(Document.SF_POSTAV_DATE <= factnom.SD_24.DD_DATE
                ? Document.SF_POSTAV_DATE.AddDays(-5)
                : factnom.SD_24.DD_DATE.AddDays(-5), DateTime.Today);

            var store = UnitOfWork.Context.SD_24.FirstOrDefault(_ => _.DOC_CODE == factnom.DOC_CODE);
            if (store == null) return;
            var oldQuan = CurrentRow.CurrencyConvertRows.Count == 0
                ? 0
                : CurrentRow.CurrencyConvertRows.Sum(_ => _.Quantity);
            dt = UnitOfWork.Context.SD_24.Where(_ => _.DOC_CODE == factnom.DOC_CODE).OrderBy(_ => _.DD_DATE).First()
                .DD_DATE;
            var newItem = new InvoiceProviderRowCurrencyConvertViewModel
            {
                Id = Guid.NewGuid(),
                DocCode = CurrentRow.DocCode,
                Code = CurrentRow.Code,
                NomenklId = n.Id,
                Date = dt,
                // ReSharper disable once PossibleInvalidOperationException
                OLdPrice = (decimal) CurrentRow.SFT_ED_CENA,
                OLdNakladPrice = Math.Round((decimal) (CurrentRow.SFT_ED_CENA +
                                                       // ReSharper disable once PossibleInvalidOperationException
                                                       (CurrentRow.SFT_SUMMA_NAKLAD ?? 0)/CurrentRow.SFT_KOL),
                    2),
                Quantity = CurrentRow.SFT_KOL - oldQuan,
                Rate = Math.Round(crsrates.GetRate(CurrentRow.Nomenkl.Currency.DocCode,
                    n.Currency.DocCode, dt), 4),
                // ReSharper disable once PossibleInvalidOperationException
                StoreDC = (decimal) store.DD_SKLAD_POL_DC
            };
            CurrentRow.Entity.TD_26_CurrencyConvert.Add(newItem.Entity);
            newItem.CalcRow(DirectCalc.Rate);
            CurrentRow.CurrencyConvertRows.Add(newItem);
            CurrentRow.State = RowStatus.Edited;
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
            get { return new Command(deleteNomenklCrsConvert, _ => CurrentCrsConvertItem != null); }
        }

        private void deleteNomenklCrsConvert(object obj)
        {
            var row = Document.Rows.FirstOrDefault(_ => _.DocCode == CurrentCrsConvertItem.DocCode
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

        public ICommand AddNomenklCommand
        {
            get { return new Command(addNomenkl, _ => Document.Kontragent != null); }
        }

        private void addNomenkl(object obj)
        {
            decimal defaultNDS;
            var nomenkls = StandartDialogs.SelectNomenkls(Document.Kontragent.BalansCurrency, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            using (var entctx = GlobalOptions.GetEntities())
            {
                defaultNDS = Convert.ToDecimal(entctx.PROFILE
                    .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
            }

            var cr = new CurrencyRates(Document.SF_POSTAV_DATE, Document.SF_POSTAV_DATE);
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var n in nomenkls.Where(_ => Document.Rows.All(t => t.DocCode != _.DocCode)))
            {
                addUsedNomenkl(n.DocCode);
                var newRow = new InvoiceProviderRow
                {
                    DocCode = Document.DocCode,
                    Code = newCode,
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Nomenkl = n,
                    SFT_KOL = 1,
                    SFT_ED_CENA = 0,
                    SFT_NDS_PERCENT = n.NDSPercent ?? defaultNDS,
                    SFT_POST_ED_IZM_DC = n.Unit.DocCode,
                    PostUnit = n.Unit,
                    UchUnit = n.Unit,
                    SFT_TEXT = " ",
                    State = RowStatus.NewRow,
                    IsIncludeInPrice = Document.IsNDSInPrice,
                    Parent = Document

                };
                switch (Document.Currency.DocCode)
                {
                    case CurrencyCode.EUR:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.EUR,
                            Document.SF_POSTAV_DATE);
                        newRow.EURSumma = newRow.EURRate;
                        break;
                    case CurrencyCode.USD:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.USD,
                            Document.SF_POSTAV_DATE);
                        newRow.EURSumma = newRow.USDRate;
                        break;
                    case CurrencyCode.RUB:
                        newRow.RUBRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.RUB,
                            Document.SF_POSTAV_DATE);
                        newRow.RUBSumma = newRow.RUBRate;
                        break;
                    case CurrencyCode.GBP:
                        newRow.GBPRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.GBP,
                            Document.SF_POSTAV_DATE);
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
            get { return new Command(addUsluga, _ => true); }
        }

        private void addUsluga(object obj)
        {
            var k = StandartDialogs.SelectNomenkls();
            if (k != null)
            {
                var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                foreach (var item in k)
                {
                    if (Document.Rows.Any(_ => _.SFT_NEMENKL_DC == item.DocCode)) continue;
                    decimal nds;
                    if (item.NOM_NDS_PERCENT == null)
                        nds = 0;
                    else
                        nds = (decimal) item.NOM_NDS_PERCENT;
                    var newRow = new InvoiceProviderRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = nds,
                        SFT_KOL = 1,
                        SFT_ED_CENA = 0,
                        SFT_POST_ED_IZM_DC = item.Unit.DocCode,
                        PostUnit = item.Unit,
                        UchUnit = item.Unit,
                        State = RowStatus.NewRow, 
                        SFT_TEXT = " ",
                        IsIncludeInPrice = Document.IsNDSInPrice,
                        Parent = Document
                    };
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
            get { return new Command(updateCalcRowSumma, _ => true); }
        }

        private void updateCalcRowSumma(object obj)
        {
            CurrentRow?.Calc();
        }

        public override void SaveData(object data)
        {
            var closePeriod = UnitOfWork.Context.PERIOD_CLOSED
                .SingleOrDefault(_ => _.CLOSED_DOC_TYPE.ID.ToString() == "b57d269e-e17f-4dc2-86da-821db51bcc9e");
            if (closePeriod != null && Document.SF_POSTAV_DATE < closePeriod.DateClosed)
            {
                WinManager.ShowWinUIMessageBox(
                    $"Документ находится в закрытом периоде.Дата закрытия {closePeriod.DateClosed.ToShortDateString()}"
                    , "Ограничение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            UnitOfWork.CreateTransaction();
            try
            {
                if (Document.State == RowStatus.NewRow)
                {
                    Document.SF_IN_NUM = UnitOfWork.Context.SD_26.Any()
                        ? UnitOfWork.Context.SD_26.Max(_ => _.SF_IN_NUM) + 1
                        : 1;
                    Document.DOC_CODE = UnitOfWork.Context.SD_26.Any()
                        ? UnitOfWork.Context.SD_26.Max(_ => _.DOC_CODE) + 1
                        : 10260000001;
                    foreach (var row in Document.Rows)
                    {
                        row.DocCode = Document.DOC_CODE;
                    }
                    //genericProviderRepository.Insert(Document.Entity);
                }
                else
                {
                    //genericProviderRepository.Update(Document.Entity);
                }

                if (Document.SF_CRS_RATE == null)
                {
                    Document.SF_CRS_RATE = 1;
                }

                if (Document.SF_KONTR_CRS_RATE == null)
                {
                    Document.SF_KONTR_CRS_RATE = 1;
                }

                if (Document.SF_UCHET_VALUTA_RATE == null)
                {
                    Document.SF_UCHET_VALUTA_RATE = 1;
                }

                if (Document.SF_KONTR_CRS_DC == null)
                {
                    Document.SF_KONTR_CRS_DC = Document.Kontragent.BalansCurrency.DocCode;
                }

                if (Document.SF_KONTR_CRS_SUMMA == null)
                {
                    Document.SF_KONTR_CRS_SUMMA = Document.SF_CRS_SUMMA;
                }

                foreach (var row in Document.Rows)
                {
                    if (row.SFT_SUMMA_V_UCHET_VALUTE == null)
                    {
                        row.SFT_SUMMA_V_UCHET_VALUTE = row.SFT_SUMMA_K_OPLATE;
                    }

                    if (row.SFT_SUMMA_K_OPLATE_KONTR_CRS == null)
                    {
                        row.SFT_SUMMA_K_OPLATE_KONTR_CRS = row.SFT_SUMMA_K_OPLATE;
                    }

                    if (row.SFT_NOM_CRS_RATE == null)
                    {
                        row.SFT_NOM_CRS_RATE = 1;
                    }
                }
                List<Guid> DistributeDocs = new List<Guid>();
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

                foreach (var row in Document.Rows)
                {
                    foreach (var crs in row.CurrencyConvertRows)
                    {
                        var distr = UnitOfWork.Context.DistributeNakladRow
                            .Include(_ => _.DistributeNakladInfo)
                            .FirstOrDefault(_ => _.TransferRowId == crs.Id);
                        if (distr != null)
                        {
                            if (DistributeDocs.All(_ => _ != distr.DocId))
                                DistributeDocs.Add(distr.DocId);
                        }
                    }
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
                UnitOfWork.Save();
                UnitOfWork.Commit();
                RecalcKontragentBalans.CalcBalans(Document.SF_POST_DC, Document.SF_POSTAV_DATE);
                NomenklManager.RecalcPrice(myUsedNomenklsDC);
                myUsedNomenklsDC.Clear();
                Document.myState = RowStatus.NotEdited;
                foreach (var r in Document.Rows)
                {
                    r.myState = RowStatus.NotEdited;
                    foreach (var rr in r.CurrencyConvertRows)
                    {
                        rr.myState = RowStatus.NotEdited;
                    }
                }

                foreach (var f in Document.Facts)
                {
                    f.myState = RowStatus.NotEdited;
                }

                foreach (var p in Document.PaymentDocs)
                {
                    p.State = RowStatus.NotEdited;
                }

                RefreshData(null);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = new InvoiceProvider(ctx.GenericProviderRepository.GetById(Document.DocCode),
                ctx.UnitOfWork);
            ctx.GenericProviderRepository.DetachObjects();
            var id = Guid.NewGuid();
            ctx.Document.Id = id;
            ctx.Document.DocCode = -1;
            ctx.Document.SF_POSTAV_NUM = null;
            ctx.Document.SF_POSTAV_DATE = DateTime.Today;
            ctx.Document.SF_REGISTR_DATE = DateTime.Today;
            ctx.Document.CREATOR = GlobalOptions.UserInfo.Name;
            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.IsAccepted = false;
            var code = 1;
            foreach (var row in ctx.Document.Rows)
            {
                row.DocCode = -1;
                row.Id = Guid.NewGuid();
                row.DocId = id;
                row.Code = code;
                row.myState = RowStatus.NewRow;
                code++;
                ctx.Document.Entity.TD_26.Add(row.Entity);
            }
            ctx.Document.DeletedRows.Clear();
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = new InvoiceProvider(ctx.GenericProviderRepository.GetById(Document.DocCode),
                ctx.UnitOfWork);
            ctx.GenericProviderRepository.DetachObjects();
            ctx.Document.DocCode = -1;
            ctx.Document.Entity.SF_CRS_SUMMA = 0;
            ctx.Document.Id = Guid.NewGuid();
            ctx.Document.SF_POSTAV_NUM = null;
            ctx.Document.SF_POSTAV_DATE = DateTime.Today;
            ctx.Document.SF_REGISTR_DATE = DateTime.Today;
            ctx.Document.CREATOR = GlobalOptions.UserInfo.Name;
            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.IsAccepted = false;
            ctx.Document.DeletedRows.Clear();
            ctx.Document.PaymentDocs.Clear();
            ctx.Document.Facts.Clear();
            ctx.Document.Rows.Clear();
            ctx.UnitOfWork.Context.SD_26.Add(ctx.Document.Entity);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.Edited)
            {
                var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        //InvoicesManager.DeleteProvider(Document.DocCode);
                        UnitOfWork.CreateTransaction();
                        try
                        {
                            GenericProviderRepository.Delete(Document.Entity);
                            UnitOfWork.Save();
                            UnitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            UnitOfWork.Rollback();
                            WindowManager.ShowError(ex);
                        }

                        Form.Close();
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageBoxResult.No:
                        Form.Close();
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            else
            {
                InvoicesManager.DeleteProvider(Document.DocCode);
                Form.Close();
            }

            InvoicesManager.DeleteProvider(Document.DocCode);
            Form.Close();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel(null)
            {
                Form = view,
            };
            ctx.Document.IsNDSInPrice = true;
            view.Show();
            view.DataContext = ctx;
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
            var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить связь с оплатой?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            switch (CurrentPaymentDoc.DocName)
            {
                case "Банковский платеж":
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
                        old2.VZT_SPOST_DC = null;
                    break;
            }

            UnitOfWork.Context.ProviderInvoicePay.Remove(CurrentPaymentDoc.Entity);
            Document.PaymentDocs.Remove(CurrentPaymentDoc);
            SaveData(null);
            Document.RaisePropertyChanged("PaySumma");
        }

        public ICommand AddPaymentFromBankCommand
        {
            get
            {
                return new Command(AddPaymentFromBank,
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.SF_CRS_SUMMA);
            }
        }

        private void AddPaymentFromBank(object obj)
        {
            try
            {
                var oper = StandartDialogs.SelectBankOperationForProviderInvoice(Document.Kontragent.DocCode);
                if (oper == null) return;
                if (Document.PaymentDocs.Any(_ => _.BankCode == oper.Code))
                {
                    return;
                }

                using (var ctx = GlobalOptions.GetEntities())
                {
                    var old = ctx.TD_101
                        .Include(_ => _.SD_101)
                        .Include(_ => _.SD_101.SD_114)
                        .Single(_ => _.CODE == oper.Code);
                    if (old != null) old.VVT_SFACT_POSTAV_DC = Document.DocCode;
                    else return;
                   
                    var newItem = new ProviderInvoicePayViewModel()
                    {
                        BankCode = old.CODE,
                        myState = RowStatus.NewRow,
                        Summa = Document.SF_CRS_SUMMA - Document.PaySumma >= oper.Remainder
                            ? oper.Remainder
                            : Document.SF_CRS_SUMMA - Document.PaySumma,
                        // ReSharper disable once PossibleInvalidOperationException
                        DocSumma = (decimal) old.VVT_VAL_RASHOD,
                        DocDate = old.SD_101.VV_START_DATE,
                        Rate = 1,
                        DocName = "Банковский платеж",
                        DocExtName = $"{old.SD_101.SD_114.BA_BANK_NAME} " +
                                     $"р/с {old.SD_101.SD_114.BA_RASH_ACC}"
                    };
                    Document.PaymentDocs.Add(newItem);
                    Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                    ctx.SaveChanges();
                }

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
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.SF_CRS_SUMMA);
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
                            Summa = Document.SF_CRS_SUMMA - Document.PaySumma >= (decimal) old.SUMM_ORD
                                ? (decimal) old.SUMM_ORD
                                : Document.SF_CRS_SUMMA - Document.PaySumma,
                            DocSumma = (decimal) old.SUMM_ORD,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocDate = (DateTime) old.DATE_ORD,
                            DocNum = old.NUM_ORD.ToString(),
                            Rate = 1,
                            DocName = "Расходный кассовый ордер",
                            // ReSharper disable once PossibleInvalidOperationException
                            DocExtName = $"Касса: {MainReferences.Cashs[(decimal) old.CA_DC].Name} "
                        };
                        Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                        Document.PaymentDocs.Add(newItem);
                    }

                    if (Document.State != RowStatus.NewRow)
                        Document.State = RowStatus.Edited;
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
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.SF_CRS_SUMMA);
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
                    Summa = Document.SF_CRS_SUMMA - Document.PaySumma >= (decimal) old.VZT_CRS_SUMMA
                        ? (decimal) old.VZT_CRS_SUMMA
                        : Document.SF_CRS_SUMMA - Document.PaySumma,
                    DocSumma = (decimal) old.VZT_CRS_SUMMA,
                    DocDate = old.SD_110.VZ_DATE,
                    Rate = 1,
                    DocName = "Акт взаимозачета",
                    DocNum = old.SD_110.VZ_NUM.ToString(),
                    DocExtName = old.VZT_DOC_NOTES
                };
                Document.Entity.ProviderInvoicePay.Add(newItem.Entity);
                Document.PaymentDocs.Add(newItem);
                if (Document.State != RowStatus.NewRow)
                    Document.State = RowStatus.Edited;
                Document.RaisePropertyChanged("PaySumma");
            }
        }

        /// <summary>
        /// удаление зависших связей с платежами
        /// </summary>
        private void deletePayments()
        {
            //TODO Добавить для кассы и акта взаимозачета
            if (Document.PaymentDocs.All(_ => _.State != RowStatus.NewRow)) return;
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var bankListCodes = ctx.Database.SqlQuery<int>(
                        "SELECT code FROM td_101 WHERE VVT_SFACT_POSTAV_DC IS NOT NULL" +
                        " and CODE NOT IN (SELECT BankCode FROM ProviderInvoicePay WHERE BankCode IS NOT null)").ToList();
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
                        deletePayments();
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
    }

    #endregion
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
            if (myCurrentItem != null && myCurrentItem.Equals(value)) return;
            myCurrentItem = value;
            RaisePropertyChanged();
        }
        get => myCurrentItem;
    }

    #endregion
}