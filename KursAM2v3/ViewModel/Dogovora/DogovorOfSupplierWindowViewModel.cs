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
using Core.EntityViewModel.Dogovora;
using Core.EntityViewModel.Employee;
using Core.EntityViewModel.Invoices;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using Helper;
using KursAM2.Auxiliary;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Dogovors;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorOfSupplierWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructors

        public DogovorOfSupplierWindowViewModel(Guid? id)
        {
            GenericRepository = new GenericKursDBRepository<DogovorOfSupplier>(UnitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            //DialogService = GetService<IDialogService>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new DogovorOfSupplierViewModel { State = RowStatus.NewRow };
                UnitOfWork.Context.DogovorOfSupplier.Add(Document.Entity);
            }
            else
            {
                Document = new DogovorOfSupplierViewModel(doc)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                foreach (var r in  Document.Rows)
                {
                     r.State = RowStatus.NotEdited;
                }
                LoadLinkDocuments();
                Document.myState = RowStatus.NotEdited;
            }
        }

        #endregion

        #region Methods

        private void RaiseAll()
        {
            Document.RaisePropertyAllChanged();
            foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
        }

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.Id = newId;
            Document.DocDate = DateTime.Today;
            Document.Creator = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.Rows.Clear();
            UnitOfWork.Context.DogovorOfSupplier.Add(Document.Entity);
            if (isCopy)
            {
                foreach (var row in Document.Rows)
                {
                    UnitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    Document.Entity.DogovorOfSupplierRow.Add(row.Entity);
                    row.myState = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.DogovorOfSupplierRow.Clear();
                }

                Document.Rows.Clear();
            }
        }

        private void LoadLinkDocuments()
        {
            Documents.Clear();
            FactsAll.Clear();
            PaymentList.Clear();
            var sflist = UnitOfWork.Context.SD_26
                .Include(_ => _.TD_26)
                .Include(_ => _.SD_24)
                .Include(_ => _.SD_24.Select(x => x.TD_24))
                .Include(_ => _.ProviderInvoicePay)
                .Include(_ => _.ProviderInvoicePay.Select(x => x.SD_34))
                .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_101))
                .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_101).Select(y => y.SD_101))
                .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_101)
                    .Select(y => y.SD_101).Select(z => z.SD_114))
                .Include(_ => _.ProviderInvoicePay.Select(x => x.TD_110))
                .Where(_ => _.DogovorOfSupplierId == Document.Id);
            foreach (var sf in sflist)
            {
                var newItem = new LinkDocumentInfo
                {
                    Id = sf.Id,
                    DocCode = sf.DOC_CODE,
                    DocumentType = DocumentType.InvoiceProvider,
                    DocDate = sf.SF_POSTAV_DATE,
                    Summa = sf.SF_CRS_SUMMA,
                    DocNumber = string.IsNullOrWhiteSpace(sf.SF_POSTAV_NUM)
                        ? sf.SF_IN_NUM.ToString()
                        : $"{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM}",
                    DocInfo =
                        $"С/ф поставщика №{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM} от {sf.SF_POSTAV_DATE.ToShortDateString()} " +
                        $"на {sf.SF_KONTR_CRS_SUMMA} {MainReferences.GetCurrency(sf.SF_CRS_DC)} " +
                        $"{Note}"
                };
                Documents.Add(newItem);
                foreach (var p in sf.SD_24)
                {
                    var newPrih = new LinkDocumentInfo
                    {
                        Id = p.Id,
                        DocCode = p.DOC_CODE,
                        DocumentType = DocumentType.StoreOrderIn,
                        DocDate = p.DD_DATE,
                        Summa = 0,
                        DocInfo = $"Приходный складской ордер №{p.DD_IN_NUM} от {p.DD_DATE.ToShortDateString()} " +
                                  $"{p.DD_NOTES}",
                        DocNumber = p.DD_IN_NUM.ToString()
                    };
                    foreach (var pp in p.TD_24)
                    {
                        var newFact = new DogovorOfSupplierFactViewModel
                        {
                            Id = pp.Id,
                            DocCode = pp.DOC_CODE,
                            Code = pp.CODE,
                            Quantity = pp.DDT_KOL_PRIHOD,
                            Nomenkl = MainReferences.GetNomenkl(pp.DDT_NOMENKL_DC),
                            OrderInDC = pp.DOC_CODE,
                            OrderInInfo =
                                $"Приходный складской ордер №{p.DD_IN_NUM} от {p.DD_DATE.ToShortDateString()} " +
                                $"{p.DD_NOTES}",
                            OrderInNote = p.DD_NOTES,
                            SPostDC = pp.DDT_SPOST_DC ?? 0,
                            SPostInfo =
                                $"С/ф поставщика №{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM} от {sf.SF_POSTAV_DATE.ToShortDateString()} " +
                                $"на {sf.SF_KONTR_CRS_SUMMA} {MainReferences.GetCurrency(sf.SF_CRS_DC)} " +
                                $"{Note}"
                        };
                        FactsAll.Add(newFact);
                    }

                    Documents.Add(newPrih);
                }

                if (sf.ProviderInvoicePay != null && sf.ProviderInvoicePay.Count > 0)
                    foreach (var pay in sf.ProviderInvoicePay)
                    {
                        var newPay = new ProviderInvoicePayViewModel(pay);
                        if (pay.TD_101 != null)
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            newPay.DocSumma = (decimal)pay.TD_101.VVT_VAL_RASHOD;
                            newPay.DocDate = pay.TD_101.SD_101.VV_START_DATE;
                            newPay.DocName = "Банковский платеж";
                            newPay.DocExtName = $"{pay.TD_101.SD_101.SD_114.BA_BANK_NAME} " +
                                                $"р/с {pay.TD_101.SD_101.SD_114.BA_RASH_ACC}";
                        }

                        if (pay.SD_34 != null)
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            newPay.DocSumma = (decimal)pay.SD_34.SUMM_ORD;
                            newPay.DocName = "Расходный кассовый ордер";
                            newPay.DocNum = pay.SD_34.NUM_ORD.ToString();
                            // ReSharper disable once PossibleInvalidOperationException
                            newPay.DocDate = (DateTime)pay.SD_34.DATE_ORD;
                            if (pay.SD_34.SD_22 != null)
                                newPay.DocExtName = $"Касса {pay.SD_34.SD_22.CA_NAME}";
                            else
                                // ReSharper disable once PossibleInvalidOperationException
                                newPay.DocExtName = $"Касса {MainReferences.CashsAll[(decimal)pay.SD_34.CA_DC].Name}";
                        }

                        if (pay.TD_110 != null)
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            newPay.DocSumma = (decimal)pay.TD_110.VZT_CRS_SUMMA;
                            newPay.DocName = "Акт взаимозачета";
                            newPay.DocNum = pay.TD_110.SD_110.VZ_NUM.ToString();
                            newPay.DocDate = pay.TD_110.SD_110.VZ_DATE;
                            newPay.DocExtName = $"{pay.TD_110.VZT_DOC_NOTES}";
                        }

                        PaymentList.Add(newPay);
                    }
            }
        }

        #endregion

        #region Fields

        public readonly GenericKursDBRepository<DogovorOfSupplier> GenericRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private DogovorOfSupplierViewModel myDocument;
        private DogovorOfSupplierRowViewModel myCurrentAccrual;
        private LinkDocumentInfo myCurrentLinkDocument;
        private DogovorOfSupplierFactViewModel myCurrentFact;
        private ProviderInvoicePayViewModel myCurrentPayment;

        #endregion

        #region Properties

        public override string LayoutName => "DogovorOfSupplierWindowViewModel";

        public override string WindowName =>
            Document.State == RowStatus.NewRow ? "Новый договор от поставщика" : Document.ToString();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DogovorOfSupplierRowViewModel> SelectedRows { set; get; } =
            new ObservableCollection<DogovorOfSupplierRowViewModel>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DogovorOfSupplierRowViewModel> DeletedRows { set; get; } =
            new ObservableCollection<DogovorOfSupplierRowViewModel>();

        public ObservableCollection<DogovorOfSupplierFactViewModel> FactsAll { set; get; } =
            new ObservableCollection<DogovorOfSupplierFactViewModel>();

        public ObservableCollection<LinkDocumentInfo> Documents { set; get; } =
            new ObservableCollection<LinkDocumentInfo>();

        public ObservableCollection<ProviderInvoicePayViewModel> PaymentList { set; get; } =
            new ObservableCollection<ProviderInvoicePayViewModel>();

        public List<ContractType> ContractTypeList =>
            MainReferences.ContractTypes.Values.Where(_ => !_.IsSale).ToList();

        public List<Employee> EmployeeList => MainReferences.Employees.Values.ToList();

        public ProviderInvoicePayViewModel CurrentPayment
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentPayment == value) return;
                myCurrentPayment = value;
                RaisePropertiesChanged();
            }
            get => myCurrentPayment;
        }

        public LinkDocumentInfo CurrentLinkDocument
        {
            get => myCurrentLinkDocument;
            set
            {
                if (myCurrentLinkDocument == value) return;
                myCurrentLinkDocument = value;
                RaisePropertyChanged();
            }
        }

        public DogovorOfSupplierFactViewModel CurrentFact
        {
            get => myCurrentFact;
            set
            {
                if (myCurrentFact == value) return;
                myCurrentFact = value;
                RaisePropertyChanged();
            }
        }

        public DogovorOfSupplierRowViewModel CurrentAccrual
        {
            get => myCurrentAccrual;
            set
            {
                if (myCurrentAccrual == value) return;
                myCurrentAccrual = value;
                RaisePropertyChanged();
            }
        }

        public DogovorOfSupplierViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public override string Description =>
            $"Договор от поставщика №{Document.InNum}/{Document.OutNum} " +
            $"от {Document.DocDate.ToShortDateString()} Контрагент: {Document.Kontragent} на сумму {Document.Summa} " +
            $"{Document.Currency}";

        #endregion

        #region Commands

        public override bool IsCanSaveData =>
            (Document.State != RowStatus.NotEdited || DeletedRows.Count > 0
                                                   || Document.Rows.Any(_ => _.State != RowStatus.NotEdited)) &&
            Document.Error == null;


        public override void DocNewCopy(object form)
        {
            var ctx = new DogovorOfSupplierWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(true);
            var frm = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorOfSupplierWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var ctx = new DogovorOfSupplierWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(false);
            var frm = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public ICommand OpenPayDocumentCommand
        {
            get { return new Command(OpenPayDocument, _ => CurrentPayment != null); }
        }

        private void OpenPayDocument(object obj)
        {
            if (CurrentPayment.BankCode != null)
            {
                DocumentsOpenManager.Open(DocumentType.Bank, (decimal)CurrentPayment.BankCode);
                return;
            }

            if (CurrentPayment.CashDC != null)
            {
                DocumentsOpenManager.Open(DocumentType.CashOut, CurrentPayment.CashDC.Value);
                return;
            }

            if (CurrentPayment.VZDC != null)
            {
                DocumentsOpenManager.Open(DocumentType.MutualAccounting, CurrentPayment.VZDC.Value);
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }


        public ICommand OpenPrihOrderCommand
        {
            get { return new Command(OpenPrihOrder, _ => CurrentFact != null && CurrentFact.OrderInDC > 0); }
        }

        private void OpenPrihOrder(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentFact.OrderInDC);
        }

        public ICommand OpenSFactCommand
        {
            get { return new Command(OpenSFact, _ => CurrentFact != null && CurrentFact.SPostDC > 0); }
        }

        private void OpenSFact(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.InvoiceProvider, CurrentFact.SPostDC);
        }

        public ICommand OpenLinkDocumentCommand
        {
            get { return new Command(OpenLinkDocument, _ => CurrentLinkDocument != null); }
        }

        private void OpenLinkDocument(object obj)
        {
            DocumentsOpenManager.Open(CurrentLinkDocument.DocumentType, CurrentLinkDocument.DocCode);
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document?.Currency != null); }
        }

        private void AddNomenkl(object obj)
        {
            decimal defaultNDS;
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            var nomenkls = StandartDialogs.SelectNomenkls(Document.Currency, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            using (var entctx = GlobalOptions.GetEntities())
            {
                defaultNDS = Convert.ToDecimal(entctx.PROFILE
                    .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
            }

            foreach (var newRow in from n in nomenkls
                where Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode)
                select new DogovorOfSupplierRowViewModel
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Nomenkl = n,
                    Quantity = 1,
                    Price = 0,
                    NDSPercent = n.NDSPercent ?? defaultNDS,
                    Parent = Document,
                    State = RowStatus.NewRow
                })
            {
                Document.Entity.DogovorOfSupplierRow.Add(newRow.Entity);
                Document.Rows.Add(newRow);
            }

            RaisePropertyChanged(nameof(State));
        }

        public ICommand AddUslugaCommand
        {
            get { return new Command(AddUsluga, _ => true); }
        }

        private void AddUsluga(object obj)
        {
            var k = Document.Currency == null
                ? StandartDialogs.SelectNomenkls()
                : StandartDialogs.SelectNomenkls(Document.Currency);
            if (k != null)
            {
                decimal defaultNDS;
                using (var entctx = GlobalOptions.GetEntities())
                {
                    defaultNDS = Convert.ToDecimal(entctx.PROFILE
                        .FirstOrDefault(_ => _.SECTION == "НОМЕНКЛАТУРА" && _.ITEM == "НДС")?.ITEM_VALUE);
                }

                foreach (var newRow in from n in k
                    where Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode)
                    select new DogovorOfSupplierRowViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n,
                        Quantity = 1,
                        Price = 0,
                        NDSPercent = n.NDSPercent ?? defaultNDS,
                        Parent = Document,
                        State = RowStatus.NewRow
                    })
                {
                    Document.Entity.DogovorOfSupplierRow.Add(newRow.Entity);
                    Document.Rows.Add(newRow);
                }
            }

            if (Document.State != RowStatus.NewRow)
                Document.myState = RowStatus.Edited;
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, _ => SelectedRows.Count > 0); }
        }

        private void DeleteRow(object obj)
        {
            if (WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить строки", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var list = SelectedRows.Select(_ => _.Id).ToList();
                foreach (var id in list)
                {
                    var row = Document.Rows.Single(_ => _.Id == id);
                    if (FactsAll.All(_ => _.Nomenkl.DocCode != row.Nomenkl.DocCode))
                    {
                        Document.Rows.Remove(row);
                        UnitOfWork.Context.Entry(row.Entity).State =
                            UnitOfWork.Context.Entry(row.Entity).State == EntityState.Added
                                ? EntityState.Detached
                                : EntityState.Deleted;
                    }

                    Document.Rows.Remove(row);
                }
            }

            Document.State = RowStatus.Edited;
        }

        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => Document != null); }
        }

        private void KontragentSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent(null, true);
            if (kontr == null) return;
            Document.Kontragent = kontr;
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            foreach (var id in Document.Rows.Where(_ => _.State == RowStatus.NewRow).Select(_ => _.Id)
                .ToList())
                Document.Rows.Remove(Document.Rows.Single(_ => _.Id == id));
            EntityManager.EntityReload(UnitOfWork.Context);
            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            RaiseAll();
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
            Document.myState = RowStatus.NotEdited;
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.NewRow)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "Вы уверены, что хотите удалить данный документ?";
                var res = service.ShowDialog(MessageButton.YesNo, "Запрос", this);
                if (res != MessageResult.Yes) return;
                switch (res)
                {
                    case MessageResult.Yes:
                        var dc = Document.Kontragent.DocCode;
                        var docdate = Document.DocDate;
                        UnitOfWork.CreateTransaction();
                        try
                        {
                            foreach (var r in Document.Rows.Where(_ => _.State != RowStatus.NewRow))
                                UnitOfWork.Context.DogovorOfSupplierRow.Remove(r.Entity);

                            UnitOfWork.Context.DogovorOfSupplier.Remove(Document.Entity);
                            UnitOfWork.Save();
                            UnitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            UnitOfWork.Rollback();
                            WindowManager.ShowError(ex);
                        }

                        Form.Close();
                        ParentFormViewModel?.RefreshData(null);
                        RecalcKontragentBalans.CalcBalans(dc, docdate);
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageResult.No:
                        Form.Close();
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageResult.Cancel:
                        return;
                }
            }
            else
            {
                Form.Close();
            }
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
                Document.InNum = UnitOfWork.Context.DogovorOfSupplier.Any()
                    ? UnitOfWork.Context.DogovorOfSupplier.Max(_ => _.InNum) + 1
                    : 1;
            try
            {
                UnitOfWork.CreateTransaction();
                DocumentHistoryHelper.SaveHistory(Document.DogType.ToString(), Document.Id,
                    null, null, (string)Document.ToJson());
                UnitOfWork.Save();
                UnitOfWork.Commit();
                RecalcKontragentBalans.CalcBalans(Document.Kontragent.DOC_CODE, Document.DocDate);
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
            }
            catch (Exception ex)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                MessageManager.ErrorShow(service, ex);
            }
        }

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.DogovorOfSupplier, Document.Id, null);
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => null;

        public string Error { get; } = null;

        #endregion
    }
}