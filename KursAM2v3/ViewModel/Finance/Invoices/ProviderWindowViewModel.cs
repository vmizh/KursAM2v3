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
using Core.Finance;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Logistiks.UC;
using KursAM2.View.Logistiks.Warehouse;
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

        private void UpdatePayDocuments(ALFAMEDIAEntities ctx)
        {
            Document.PaymentDocs.Clear();
            foreach (var c in ctx.SD_34.Where(_ => _.SPOST_DC == Document.DocCode).ToList())
                Document.PaymentDocs.Add(new InvoicePaymentDocument
                {
                    DocCode = c.DOC_CODE,
                    Code = 0,
                    DocumentType = DocumentType.CashOut,
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentName =
                        $"{c.NUM_ORD} от {c.DATE_ORD} на {c.SUMM_ORD} {MainReferences.Currencies[(decimal) c.CRS_DC]} ({c.CREATOR})",
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = (decimal) c.SUMM_ORD,
                    Currency = MainReferences.Currencies[(decimal) c.CRS_DC],
                    Note = c.NOTES_ORD
                });
            foreach (var c in ctx.TD_101.Include(_ => _.SD_101).Where(_ => _.VVT_SFACT_POSTAV_DC == Document.DocCode)
                .ToList())
                Document.PaymentDocs.Add(new InvoicePaymentDocument
                {
                    DocCode = c.DOC_CODE,
                    Code = c.CODE,
                    DocumentType = DocumentType.Bank,
                    DocumentName =
                        // ReSharper disable once PossibleInvalidOperationException
                        $"{c.SD_101.VV_START_DATE} на {(decimal) c.VVT_VAL_PRIHOD} {MainReferences.BankAccounts[c.SD_101.VV_ACC_DC]}",
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = (decimal) c.VVT_VAL_RASHOD,
                    Currency = MainReferences.Currencies[c.VVT_CRS_DC],
                    Note = c.VVT_DOC_NUM
                });
            foreach (var c in ctx.TD_110.Include(_ => _.SD_110).Where(_ => _.VZT_SPOST_DC == Document.DocCode).ToList())
                Document.PaymentDocs.Add(new InvoicePaymentDocument
                {
                    DocCode = c.DOC_CODE,
                    Code = c.CODE,
                    DocumentType = DocumentType.MutualAccounting,
                    DocumentName =
                        // ReSharper disable once PossibleInvalidOperationException
                        $"Взаимозачет №{c.SD_110.VZ_NUM} от {c.SD_110.VZ_DATE} на {c.VZT_CRS_SUMMA}",
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = (decimal) c.VZT_CRS_SUMMA,
                    Currency = MainReferences.Currencies[c.SD_110.CurrencyToDC],
                    Note = c.VZT_DOC_NOTES
                });
        }


        #endregion

        #region Fields

        private InvoiceProvider myDocument;
        private InvoiceProviderRow myCurrentRow;
        private readonly InvoicesManager invoiceManager = new InvoicesManager();

        #endregion

        #region Constructors

        public ProviderWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Счет-фактура поставщика";
            CreateReportsMenu();
        }
        
        public ProviderWindowViewModel(decimal? dc) : this()
        {
            //Document = dc != null ? InvoicesManager.GetInvoiceProvider(dc.Value) : InvoicesManager.NewProvider();
            Document = dc != null ? invoiceManager.GetInvoiceProvider2(dc.Value) : InvoicesManager.NewProvider();
            if (Document != null)
                WindowName = Document.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Local
        public ProviderWindowViewModel(Guid? id) : this()
        {
        }

        #endregion

        #region Properties

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

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow
                                                                  && Document.PaymentDocs?.Count == 0 &&
                                                                  Document?.Facts.Count == 0;

        public override bool IsCanSaveData
        {
            get
            {
                FooterText = string.Empty;
                if (Document.State != RowStatus.NotEdited)
                {
                    Document.RaisePropertyChanged("SF_CRS_SUMMA");
                    Document.RaisePropertyChanged("SF_KONTR_CRS_SUMMA");
                    Document.RaisePropertyChanged("PaySumma");
                    UpdateCalcRowSumma(null);
                }

                Document.RaisePropertyChanged(nameof(State));
                if (Document.State == RowStatus.NotEdited) return false;
                if (Document.PaySumma > Document.SF_CRS_SUMMA)
                {
                    FooterText =
                        $"Сумма оплаты {Document.PaySumma:n2} больше суммы счета {Document.SF_CRS_SUMMA:n2}";
                    return false;
                }

                var res = Document.SF_POST_DC > 0 //&& Document.SF_POLUCH_KONTR_DC != null
                          && Document.SF_CRS_DC != null && Document.SF_CENTR_OTV_DC != null
                          && Document.PayCondition != null &&
                          Document.SF_VZAIMOR_TYPE_DC != null
                          && Document.SF_FORM_RASCH_DC != null &&
                          Document.State != RowStatus.NotEdited ||
                          Document.DeletedRows.Count > 0 || 
                          Document.Rows.Any(_ => _.State != RowStatus.NotEdited);
                if (!res)
                {
                    if (Document.SF_POST_DC == 0)
                        FooterText += "Не выбран контрагент. ";
                    //if (Document.SF_POLUCH_KONTR_DC == null)
                    //    FooterText += "Не выбран получатель. ";
                    if (Document.SF_CRS_DC == 0)
                        FooterText += "Не выбрана валюта документа. ";
                    if (Document.SF_CENTR_OTV_DC == null)
                        FooterText += "Не выбран Центр ответственности. ";
                    if (Document.PayCondition == null)
                        FooterText += "Не выбрано условие оплаты. ";
                    if (Document.SF_VZAIMOR_TYPE_DC == null)
                        FooterText += "Не выбран тип продукции. ";
                    if (Document.SF_FORM_RASCH_DC == null)
                        FooterText += "Не выбрана форма расчетов. ";
                }

                return res;
            }
        }

        private InvoicePaymentDocument myCurrentPaymentDoc;
        private InvoiceProviderRowCurrencyConvertViewModel myCurrentCrsConvertItem;

        public InvoicePaymentDocument CurrentPaymentDoc
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
            get { return new Command(AddStoreLink, _ => true); }
        }

        private void AddStoreLink(object obj)
        {
            var ctx = new AddNomenklFromOrderInViewModel(Document.Kontragent);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            if (dlg.ShowDialog() == false) return;
            using (var dbctx = GlobalOptions.GetEntities())
            {
                var defaultNDS = Convert.ToDecimal(dbctx.PROFILE
                    .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
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
                            var oldOrdRow = dbctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == item.DocCode
                                                                            && _.CODE == item.Code);
                            Document.Facts.Add(new WarehouseOrderInRow(oldOrdRow)
                            {
                                DDT_SPOST_DC = Document.DocCode,
                                DDT_SPOST_ROW_CODE = newCode,
                                State = RowStatus.NewRow
                            });
                        }
                        else
                        {
                            var oldOrdRow = dbctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == item.DocCode
                                                                            && _.CODE == item.Code);
                            Document.Facts.Add(new WarehouseOrderInRow(oldOrdRow)
                            {
                                State = RowStatus.NewRow
                            });
                            if (oldOrdRow != null)
                            {
                                oldOrdRow.DDT_SPOST_DC = srow.DocCode;
                                oldOrdRow.DDT_SPOST_ROW_CODE = srow.Code;
                                dbctx.SaveChanges();
                            }

                            if (srow.SFT_KOL < item.Quantity)
                                srow.SFT_KOL = item.Quantity;
                        }
                    }
                }
            }
        }

        public ICommand DeleteStoreLinkCommand
        {
            get { return new Command(DeleteStoreLink, _ => CurrentFact != null); }
        }

        private void DeleteStoreLink(object obj)
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
            get { return new Command(OpenPayDocument, _ => CurrentPaymentDoc != null); }
        }

        private void OpenPayDocument(object obj)
        {
            switch (CurrentPaymentDoc.DocumentType)
            {
                case DocumentType.CashOut:
                    DocumentsOpenManager.Open(DocumentType.CashOut, CurrentPaymentDoc.DocCode);
                    break;
                case DocumentType.Bank:
                    DocumentsOpenManager.Open(DocumentType.Bank, CurrentPaymentDoc.Code);
                    break;
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var res = WinManager.ShowWinUIMessageBox("В документ внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        invoiceManager.SaveProvider2(Document);
                        return;
                    case MessageBoxResult.No:
                        invoiceManager.RefreshProvider(Document);
                        //Document.Entity.
                        //context.Entry(entity).State = EntityState.Detached;
                        //entity = context.Find(entity.ID);
                        RaisePropertiesChanged(nameof(Document));
                        return;
                }
            }

            Document.myState = RowStatus.NotEdited;
            foreach (var r in Document.Rows)
            {
                r.myState = RowStatus.NotEdited;
                foreach (var rr in r.CurrencyConvertRows)
                {
                    rr.State = RowStatus.NotEdited;
                }
            }
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
            get { return new Command(UslugaAdd, _ => true); }
        }

        private void UslugaAdd(object obj)
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

            UpdateVisualData();
        }

        private void UpdateVisualData()
        {
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("SF_CRS_SUMMA");
            // ReSharper disable once NotResolvedInText
            Document.RaisePropertyChanged("SF_KONTR_CRS_SUMMA");
            if (Form is InvoiceClientView frm)
            {
                frm.gridRows.RefreshData();
                RaisePropertiesChanged(nameof(Document));
            }
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, _ => CurrentRow != null); }
        }

        private void DeleteRow(object obj)
        {
            if (CurrentRow != null && CurrentRow.State != RowStatus.NewRow) Document.DeletedRows.Add(CurrentRow);
            var f = Document.Facts.FirstOrDefault(_ => _.DDT_SPOST_DC == CurrentRow.DocCode
                                                       && _.DDT_SPOST_ROW_CODE == CurrentRow.Code);
            if (f != null) Document.Facts.Remove(f);
            Document.Rows.Remove(CurrentRow);
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
            var crsrates = new CurrencyRates(Document.SF_POSTAV_DATE.AddDays(-5), DateTime.Today);

            var noms = MainReferences.ALLNomenkls.Values.Where(_ => _.MainId == CurrentRow.Nomenkl.MainId
                                                                    && _.Currency.DocCode !=
                                                                    CurrentRow.Nomenkl.Currency.DocCode).ToList();
            if (noms.Count == 0) return;
            Nomenkl n;
            if (noms.Count > 1)
            {
                var ctx = new NomenklSlectForCurrencyConvertViewModel()
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
            var oldQuan = CurrentRow.CurrencyConvertRows.Count == 0
                ? 0
                : CurrentRow.CurrencyConvertRows.Sum(_ => _.Quantity);
            var newItem = new InvoiceProviderRowCurrencyConvertViewModel
            {
                Id = Guid.NewGuid(),
                DocCode = CurrentRow.DocCode,
                Code = CurrentRow.Code,
                NomenklId = n.Id,
                Date = DateTime.Today,
                // ReSharper disable once PossibleInvalidOperationException
                OLdPrice = (decimal) CurrentRow.SFT_ED_CENA,
                OLdNakladPrice = Math.Round((decimal) CurrentRow.SFT_ED_CENA +
                                 // ReSharper disable once PossibleInvalidOperationException
                                 (decimal) CurrentRow.SFT_ED_CENA / (decimal) CurrentRow.SFT_SUMMA_NAKLAD,2),
                Quantity = CurrentRow.SFT_KOL - oldQuan,
                Rate = Math.Round(crsrates.GetRate(CurrentRow.Nomenkl.Currency.DocCode,
                    n.Currency.DocCode, DateTime.Today),4)
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
            get { return new Command(DeleteNomenklCrsConvert, _ => CurrentCrsConvertItem != null); }
        }

        private void DeleteNomenklCrsConvert(object obj)
        {
            if (CurrentCrsConvertItem == null) return;
            if(CurrentCrsConvertItem.State != RowStatus.NewRow)
                Document.DeletedCurrencyRows.Add(CurrentCrsConvertItem);
            if (CurrentRow == null)
            {
                var row = Document.Rows.FirstOrDefault(_ => _.Id == CurrentCrsConvertItem.Id);
                if (row != null)
                {
                    row.CurrencyConvertRows.Remove(CurrentCrsConvertItem);
                    row.State = RowStatus.Edited;
                }
            }
            else
            {
                CurrentRow.CurrencyConvertRows.Remove(CurrentCrsConvertItem);
                CurrentRow.Entity.TD_26_CurrencyConvert.Remove(CurrentCrsConvertItem.Entity);
                CurrentRow.State = RowStatus.Edited;
            }
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        private void AddNomenkl(object obj)
        {
            decimal defaultNDS;
            var nomenkls = StandartDialogs.SelectNomenkls();
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
                    State = RowStatus.NewRow

                };
                switch (Document.Currency.DocCode)
                {
                    case CurrencyCode.EUR:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.EUR,Document.SF_POSTAV_DATE);
                        newRow.EURSumma = newRow.EURRate;
                        break;
                    case CurrencyCode.USD:
                        newRow.EURRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.USD, Document.SF_POSTAV_DATE);
                        newRow.EURSumma = newRow.USDRate;
                        break;
                    case CurrencyCode.RUB:
                        newRow.RUBRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.RUB, Document.SF_POSTAV_DATE);
                        newRow.RUBSumma = newRow.RUBRate;
                        break;
                    case CurrencyCode.GBP:
                        newRow.GBPRate = cr.GetRate(Document.Currency.DocCode, CurrencyCode.GBP, Document.SF_POSTAV_DATE);
                        newRow.GBPSumma = newRow.GBPRate;
                        break;

                }
                Document.Rows.Add(newRow);
                newCode++;
            }
        }

        public Command AddUslugaCommand
        {
            get { return new Command(AddUsluga, _ => true); }
        }

        private void AddUsluga(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(null, true);
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
                    Document.Rows.Add(new InvoiceProviderRow
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
                        State = RowStatus.NewRow
                    });
                    newCode++;
                }

                UpdateVisualData();
            }
        }

        public ICommand UpdateCalcRowSummaCommand
        {
            get { return new Command(UpdateCalcRowSumma, _ => true); }
        }

        private void UpdateCalcRowSumma(object obj)
        {
            CurrentRow?.Calc();
        }

        public override void SaveData(object data)
        {
            //var dc = InvoicesManager.SaveProvider(Document);
            var dc = invoiceManager.SaveProvider2(Document);
            if (dc > 0)
            {
                if (Document.DocCode < 0)
                    Document.DocCode = dc;
                RecalcKontragentBalans.CalcBalans(Document.SF_POST_DC, Document.SF_POSTAV_DATE);
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged(nameof(State));
                RefreshData(null);
            }
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewProviderCopy(Document.DocCode);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            //var d = form as InvoiceProvider;
            //if (d == null) return;
            if (Document == null) return;
            var frm = new InvoiceProviderView {Owner = Application.Current.MainWindow};
            var ctx = new ProviderWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewProviderRequisite(Document.DocCode);
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
                        InvoicesManager.DeleteProvider(Document.DocCode);
                        Form.Close();
                        return;
                    case MessageBoxResult.No:
                        Form.Close();
                        return;
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
            var ctx = new ProviderWindowViewModel
            {
                Form = view,
                Document = InvoicesManager.NewProvider()
            };
            view.Show();
            view.DataContext = ctx;
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
            var oper = StandartDialogs.SelectBankOperationForProviderInvoice(Document.Kontragent.DocCode);
            if (oper == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var old = ctx.TD_101.Single(_ => _.CODE == oper.Code);
                if (old != null) old.VVT_SFACT_POSTAV_DC = Document.DocCode;

                ctx.SaveChanges();
                UpdatePayDocuments(ctx);
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
                var old = ctx.SD_34.Single(_ => _.DOC_CODE == oper.DocCode);
                if (old != null)
                {
                    old.SPOST_DC = Document.DocCode;
                    old.SPOST_CRS_DC = Document.Currency.DocCode;
                    old.SPOST_CRS_RATE = 1;
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
                    _ => Document?.Kontragent != null && Document.PaySumma < Document.SF_CRS_SUMMA);
            }
        }

        private void AddPaymentFromVZ(object obj)
        {
            var oper = StandartDialogs.SelectVZOperationForProviderInvoice(Document.Kontragent.DocCode);
            if (oper == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var old = ctx.TD_110.Single(_ => _.DOC_CODE == oper.DocCode && _.CODE == oper.Code);
                if (old != null) old.VZT_SPOST_DC = Document.DocCode;
                ctx.SaveChanges();
                UpdatePayDocuments(ctx);
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
}