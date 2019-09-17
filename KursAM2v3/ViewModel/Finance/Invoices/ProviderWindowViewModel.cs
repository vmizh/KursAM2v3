using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Finance;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance.Invoices;
using KursAM2.ViewModel.Management.Calculations;
using Reports.Base;

namespace KursAM2.ViewModel.Finance.Invoices
{
    /// <summary>
    ///     Сфчет-фактура поставщика
    /// </summary>
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

        #endregion

        #region Fields  

        private InvoiceProvider myDocument;
        private InvoiceProviderRow myCurrentRow;

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
            Document = dc != null ? InvoicesManager.GetInvoiceProvider(dc.Value) : InvoicesManager.NewProvider();
            if (Document == null || Document.Rows == null) return;
            foreach (var row in Document.Rows)
            foreach (var t in row.Entity.TD_24)
                Facts.Add(new InvoiceProviderWarehouseReceipt(t));
            if (Document != null)
                WindowName = Document.ToString();
        }

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
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<InvoiceProviderRow> SelectedRows { set; get; }

        public ObservableCollection<InvoiceProviderWarehouseReceipt> Facts { set; get; } =
            new ObservableCollection<InvoiceProviderWarehouseReceipt>();

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

                Document.RaisePropertyChanged("State");
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
                          Document.DeletedRows.Count > 0 || Document.Rows.Any(_ => _.State != RowStatus.NotEdited);
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
                        InvoicesManager.SaveProvider(Document);
                        Document = InvoicesManager.GetInvoiceProvider(Document.DocCode);
                        Document.WindowViewModel = this;
                        return;
                    case MessageBoxResult.No:
                        Document = InvoicesManager.GetInvoiceProvider(Document.DocCode);
                        Document.WindowViewModel = this;
                        RaisePropertiesChanged(nameof(Document));
                        return;
                }
            }
            else
            {
                Document = InvoicesManager.GetInvoiceProvider(Document.DocCode);
                Document.WindowViewModel = this;
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
                    decimal NDS;
                    if (item.NOM_NDS_PERCENT == null)
                        NDS = 0;
                    else
                        NDS = (decimal) item.NOM_NDS_PERCENT;
                    Document.Rows.Add(new InvoiceProviderRow
                    {
                        DOC_CODE = -1,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = NDS,
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
                //if (Document.IsNDSIncludeInPrice)
                //{
                //    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                //    if (colPrice != null) colPrice.ReadOnly = true;
                //    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
                //    if (colSumma != null) colSumma.ReadOnly = false;
                //}
                //else
                //{
                //    var colPrice = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_ED_CENA");
                //    if (colPrice != null) colPrice.ReadOnly = false;
                //    var colSumma = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "SFT_SUMMA_K_OPLATE");
                //    if (colSumma != null) colSumma.ReadOnly = true;
                //}
                frm.gridRows.RefreshData();
                RaisePropertiesChanged(nameof(Document));
            }
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, param => true); }
        }

        private void DeleteRow(object obj)
        {
            if (CurrentRow != null && CurrentRow.State != RowStatus.NewRow)
                Document.DeletedRows.Add(CurrentRow);
            Document.Rows.Remove(CurrentRow);
            UpdateVisualData();
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

            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var n in nomenkls.Where(_ => Document.Rows.All(t => t.DocCode != _.DocCode)))
            {
                Document.Rows.Add(new InvoiceProviderRow
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
                });
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
                    decimal NDS;
                    if (item.NOM_NDS_PERCENT == null)
                        NDS = 0;
                    else
                        NDS = (decimal) item.NOM_NDS_PERCENT;
                    Document.Rows.Add(new InvoiceProviderRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        SFT_NEMENKL_DC = item.DOC_CODE,
                        SFT_NDS_PERCENT = NDS,
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
            var dc = InvoicesManager.SaveProvider(Document);
            if (dc > 0)
            {
                if (Document.DocCode < 0)
                    Document.DocCode = dc;
                RecalcKontragentBalans.CalcBalans(Document.SF_POST_DC, Document.SF_POSTAV_DATE);
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
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

        #endregion
    }
}