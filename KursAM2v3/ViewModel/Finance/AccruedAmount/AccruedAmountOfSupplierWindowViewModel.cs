using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.AccruedAmount;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Utils.Extensions;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    public sealed class AccruedAmountOfSupplierWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructors

        public AccruedAmountOfSupplierWindowViewModel(Guid? id)
        {
            GenericRepository = new GenericKursDBRepository<AccruedAmountOfSupplier>(UnitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new AccruedAmountOfSupplierViewModel { State = RowStatus.NewRow };
                UnitOfWork.Context.AccruedAmountOfSupplier.Add(Document.Entity);
            }
            else
            {
                Document = new AccruedAmountOfSupplierViewModel(doc, GenericRepository.Context)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                Document.Rows.ForEach(_ => _.State = RowStatus.NotEdited);
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
            UnitOfWork.Context.AccruedAmountOfSupplier.Add(Document.Entity);
            if (isCopy)
            {
                foreach (var row in Document.Rows)
                {
                    UnitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    Document.Entity.AccuredAmountOfSupplierRow.Add(row.Entity);
                    row.myState = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.AccuredAmountOfSupplierRow.Clear();
                }

                Document.Rows.Clear();
            }
        }

        #endregion

        #region Fields

        public readonly GenericKursDBRepository<AccruedAmountOfSupplier> GenericRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private AccruedAmountOfSupplierViewModel myDocument;
        private AccruedAmountOfSupplierRowViewModel myCurrentAccrual;
        private AccruedAmountOfSupplierCashDocViewModel myCurrentCash;

        #endregion

        #region Properties

        public override string LayoutName => "AccruedAmountOfSupplierWindowViewModel";
        public override string WindowName => Document.ToString();

        public ObservableCollection<AccruedAmountOfSupplierRowViewModel> SelectedRows { set; get; } =
            new ObservableCollection<AccruedAmountOfSupplierRowViewModel>();

        public ObservableCollection<AccruedAmountOfSupplierRowViewModel> DeletedRows { set; get; } =
            new ObservableCollection<AccruedAmountOfSupplierRowViewModel>();

        public AccruedAmountOfSupplierRowViewModel CurrentAccrual
        {
            get => myCurrentAccrual;
            set
            {
                if (myCurrentAccrual == value) return;
                myCurrentAccrual = value;
                RaisePropertyChanged();
            }
        }

        public AccruedAmountOfSupplierCashDocViewModel CurrentCash
        {
            get => myCurrentCash;
            set
            {
                if (myCurrentCash == value) return;
                myCurrentCash = value;
                RaisePropertyChanged();
            }
        }

        public AccruedAmountOfSupplierViewModel Document
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
            $"Прямой расход №{Document.DocInNum}/{Document.DocExtNum} " +
            $"от {Document.DocDate.ToShortDateString()} Контрагент: {Document.Kontragent} на сумму {Document.Summa} " +
            $"{Document.Currency}";

        #endregion

        #region Commands

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.AccruedAmountOfSupplier, Document.Id, 0, null );
        }

        public override bool IsCanSaveData =>
            (Document.State != RowStatus.NotEdited || DeletedRows.Count > 0
                                                   || Document.Rows.Any(_ => _.State != RowStatus.NotEdited)) &&
            Document.Error == null;

        public override bool IsDocDeleteAllow => Document.Rows.All(_ => _.CashDocs.Count == 0);

        public override void DocNewCopy(object form)
        {
            var ctx = new AccruedAmountOfSupplierWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AccruedAmountOfSupplierWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var ctx = new AccruedAmountOfSupplierWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        [Display(AutoGenerateField = false)]
        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => true); }
        }

        private void KontragentSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent();
            if (kontr == null) return;
            Document.Kontragent = kontr;
            RaisePropertyChanged(nameof(WindowName));
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddAccrualCommand
        {
            get { return new Command(AddAccrual, _ => Document.Kontragent != null); }
        }

        private void AddAccrual(object obj)
        {
            var k = StandartDialogs.SelectNomenkls();
            if (k != null)
                foreach (var item in k)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var newItem = new AccruedAmountOfSupplierRowViewModel(null, Document)
                    {
                        State = RowStatus.NewRow,
                        Nomenkl = item
                    };
                    // ReSharper disable once PossibleNullReferenceException
                    if (Document.State != RowStatus.NewRow)
                    {
                        Document.myState = RowStatus.Edited;
                        Document.RaisePropertyChanged("State");
                    }

                    Document.Entity.AccuredAmountOfSupplierRow.Add(newItem.Entity);
                    Document.Rows.Add(newItem);
                    CurrentAccrual = newItem;
                }
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteAccrualCommand
        {
            get
            {
                return new Command(DeleteAccrual, _ => CurrentAccrual != null && CurrentAccrual.CashDocs.Count == 0);
            }
        }

        private void DeleteAccrual(object obj)
        {
            if (CurrentAccrual.State != RowStatus.NewRow) 
                DeletedRows.Add(CurrentAccrual);
            UnitOfWork.Context.AccuredAmountOfSupplierRow.Remove(CurrentAccrual.Entity);
            Document.Rows.Remove(CurrentAccrual);
            if (Form is AccruedAmountOfSupplierView frm)
            {
                frm.gridRows.UpdateTotalSummary();
                frm.gridCashRows.UpdateTotalSummary();
            }
            Document.RaisePropertyChanged("Summa");
        }

        public override void RefreshData(object obj)
        {
            bool isMustCheckState = true;
            base.RefreshData(obj);
            if (obj is bool p)
                isMustCheckState = p;
            if (IsCanSaveData && isMustCheckState)
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
            foreach (var r in Document.Rows)
            {
                r.CashDocs.Clear();
                if (r.Entity.SD_34 != null && r.Entity.SD_34.Count > 0)
                    foreach (var d in UnitOfWork.Context.SD_34.Where(_ => _.AccuredId == r.Id).ToList())
                        r.CashDocs.Add(new AccruedAmountOfSupplierCashDocViewModel
                        {
                            Creator = d.CREATOR,
                            DocCode = d.DOC_CODE,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocDate = (DateTime)d.DATE_ORD,
                            DocNumber = d.NUM_ORD.ToString(),
                            DocumentType = "Расходный кассовый ордер",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal)d.CRS_SUMMA,
                            Note = d.NAME_ORD + " " + d.NOTES_ORD
                        });
                if (r.Entity.TD_101 != null && r.Entity.TD_101.Count > 0)
                    foreach (var d in UnitOfWork.Context.TD_101.Where(_ => _.AccuredId == r.Id).ToList())
                        r.CashDocs.Add(new AccruedAmountOfSupplierCashDocViewModel
                        {
                            Creator = null,
                            DocCode = d.DOC_CODE,
                            Code = d.CODE,
                            DocDate = d.SD_101.VV_START_DATE,
                            DocNumber = d.VVT_DOC_NUM,
                            DocumentType = "Банковская транзакция",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal)d.VVT_VAL_RASHOD,
                            Note = null
                        });
                if (Form is AccruedAmountOfSupplierView frm)
                {
                    frm.gridRows.UpdateTotalSummary();
                    frm.gridCashRows.UpdateTotalSummary();
                }

                r.myState = RowStatus.NotEdited;
            }

            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.NewRow)
            {
                var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?",
                    "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        var dc = Document.Kontragent.DocCode;
                        var docdate = Document.DocDate;
                        //InvoicesManager.DeleteProvider(Document.DocCode);
                        UnitOfWork.CreateTransaction();
                        try
                        {
                            //GenericRepository.Delete(Document.Entity);
                            foreach (var r in Document.Rows.Where(_ => _.State != RowStatus.NewRow))
                                UnitOfWork.Context.AccuredAmountOfSupplierRow.Remove(r.Entity);

                            UnitOfWork.Context.AccruedAmountOfSupplier.Remove(Document.Entity);
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
                Form.Close();
            }
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
                Document.DocInNum = UnitOfWork.Context.AccruedAmountOfSupplier.Any()
                    ? UnitOfWork.Context.AccruedAmountOfSupplier.Max(_ => _.DocInNum) + 1
                    : 1;
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.AccruedAmountOfSupplier), Document.Id,
                0, null, (string)Document.ToJson());
            DeletedRows.Clear();
            RecalcKontragentBalans.CalcBalans(Document.Kontragent.DOC_CODE, Document.DocDate);
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
            ParentFormViewModel?.RefreshData(null);
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddCashDocCommand
        {
            get
            {
                return new Command(AddCashDoc, _ => CurrentAccrual != null && CurrentAccrual.SDRSchet != null
                                                                           && CurrentAccrual.State != RowStatus.NewRow
                                                                           && CurrentAccrual.Summa -
                                                                           CurrentAccrual.PaySumma > 0);
            }
        }

        private void AddCashDoc(object obj)
        { 
            var ctx = new SelectCashBankDialogViewModel(true, Document.Currency);
            var service = GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Выбрать кассу", ctx) == MessageResult.OK
            || ctx.DialogResult == MessageResult.OK)
            {
                var winManager = new WindowManager();
                if (ctx.CurrentObject == null) return;
                var cash = ctx.CurrentObject;
                var maxsumma =
                    CashManager.GetCashCurrencyRemains(cash.DocCode, Document.Currency.DocCode, DateTime.Today);
                if (maxsumma <= 0)
                {
                    winManager.ShowWinUIMessageBox(
                        $"Остаток по кассе 0 {Document.Currency}.Ордер не будет создан.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                if (CurrentAccrual.Summa - CurrentAccrual.PaySumma > maxsumma)
                {
                   
                    var res = winManager.ShowWinUIMessageBox(
                        $"Сумма платежа {CurrentAccrual.Summa - CurrentAccrual.PaySumma:n2} " +
                        $"больше остатка по кассе {maxsumma:n2}! " +
                        $"Будет установлена сумма {maxsumma}.",
                        "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                    if (res == MessageBoxResult.No) return;
                }

                var s = CurrentAccrual.Summa - CurrentAccrual.PaySumma > maxsumma
                    ? maxsumma
                    : CurrentAccrual.Summa - CurrentAccrual.PaySumma;
                var snum = string.IsNullOrWhiteSpace(Document.DocExtNum)
                    ? Document.DocInNum.ToString()
                    : $"{Document.DocInNum}/{Document.DocExtNum}";
                var vm = new CashOutWindowViewModel
                {
                    Document = CashManager.NewCashOut()
                };
                vm.Document.Cash = MainReferences.Cashs[cash.DocCode];
                vm.Document.KontragentType = CashKontragentType.Kontragent;
                vm.Document.KONTRAGENT_DC = Document.Kontragent.DocCode;
                vm.Document.Currency = Document.Kontragent.BalansCurrency;
                vm.Document.SUMM_ORD = s;
                vm.Document.AccuredId = CurrentAccrual.Id;
                vm.Document.AccuredInfo = $"Прямой расход №{snum} от {Document.DocDate.ToShortDateString()} " +
                                          $"{CurrentAccrual.Nomenkl}({CurrentAccrual.Nomenkl.NomenklNumber})";
                vm.Document.SDRSchet = CurrentAccrual.SDRSchet;
                vm.SaveData(null);
                CurrentAccrual.CashDocs.Add(new AccruedAmountOfSupplierCashDocViewModel
                {
                    Creator = vm.Document.CREATOR,
                    DocCode = vm.Document.DocCode,
                    // ReSharper disable once PossibleInvalidOperationException
                    DocDate = (DateTime)vm.Document.DATE_ORD,
                    DocNumber = vm.Document.NUM_ORD.ToString(),
                    DocumentType = "Расходный кассовый ордер",
                    // ReSharper disable once PossibleInvalidOperationException
                    Summa = (decimal)vm.Document.SUMM_ORD,
                    Note = vm.Document.NAME_ORD + " " + vm.Document.NOTES_ORD
                });
                DocumentsOpenManager.Open(DocumentType.CashOut, vm, Form);
                if (Form is AccruedAmountOfSupplierView frm)
                {
                    frm.gridRows.UpdateTotalSummary();
                    frm.gridCashRows.UpdateTotalSummary();
                    CurrentAccrual.RaisePropertyChanged("PaySumma");
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand OpenRashodDocCommand
        {
            get
            {
                return new Command(OpenRashodDoc,
                    _ => CurrentCash != null);
            }
        }

        private void OpenRashodDoc(object obj)
        {
            switch (CurrentCash.DocumentType)
            {
                case "Расходный кассовый ордер":
                    DocumentsOpenManager.Open(DocumentType.CashOut, CurrentCash.DocCode, null, Form);
                    break;
                case "Банковская транзакция":
                    DocumentsOpenManager.Open(DocumentType.Bank, CurrentCash.Code, null, Form);
                    break;
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteLinkRashodDocCommand
        {
            get
            {
                return new Command(DeleteLinkRashodDoc,
                    _ => CurrentCash != null);
            }
        }

        private void DeleteLinkRashodDoc(object obj)
        {
            var service = GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Вы действительно хотите удалить связь с платежным документом?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.Yes)
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (CurrentCash.DocumentType == "Расходный кассовый ордер")
                    {
                        var old = ctx.SD_34.FirstOrDefault(_ => _.DOC_CODE == CurrentCash.DocCode);
                        if (old != null)
                            old.AccuredId = null;
                    }

                    if (CurrentCash.DocumentType == "Банковская транзакция")
                    {
                        var old = ctx.TD_101.FirstOrDefault(_ => _.CODE == CurrentCash.Code);
                        if (old != null)
                            old.AccuredId = null;
                    }

                    ctx.SaveChanges();
                    CurrentAccrual.CashDocs.Remove(CurrentCash);
                    if (Form is AccruedAmountOfSupplierView frm)
                    {
                        frm.gridRows.UpdateTotalSummary();
                        frm.gridCashRows.UpdateTotalSummary();
                        CurrentAccrual.RaisePropertyChanged("PaySumma");
                    }
                }
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddBankDocCommand
        {
            get
            {
                return new Command(AddBankDoc, _ => CurrentAccrual != null && CurrentAccrual.SDRSchet != null
                                                                           && CurrentAccrual.State != RowStatus.NewRow
                                                                           && CurrentAccrual.Summa -
                                                                           CurrentAccrual.PaySumma > 0);
            }
        }

        private void AddBankDoc(object obj)
        {
            var ctx = new SelectCashBankDialogViewModel(false, Document.Currency);
            var service = GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.OK
            || ctx.DialogResult == MessageResult.OK)
            {
                var winManager = new WindowManager();
                if (ctx.CurrentObject == null) return;
                var CurrentBankAccount = ctx.CurrentObject;
                if (CurrentBankAccount != null)
                {
                    var manager = new BankOperationsManager();
                    var maxsumma = manager.GetRemains2(CurrentBankAccount.DocCode, DateTime.Today, DateTime.Today)
                        .SummaEnd;
                    if (maxsumma <= 0)
                    {
                        winManager.ShowWinUIMessageBox(
                            $"Остаток по банковскому счету = 0 {Document.Currency}.Транзакция не будет создана.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                    if (CurrentAccrual.Summa - CurrentAccrual.PaySumma > maxsumma)
                    {
                        var res = winManager.ShowWinUIMessageBox(
                            $"Сумма платежа {CurrentAccrual.Summa - CurrentAccrual.PaySumma:n2} " +
                            $"большу остатка по счету {maxsumma:n2}! " +
                            $"Будет установлена сумма {maxsumma}.",
                            "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                        if (res == MessageBoxResult.No) return;
                    }

                    var s = CurrentAccrual.Summa - CurrentAccrual.PaySumma > maxsumma
                        ? maxsumma
                        : CurrentAccrual.Summa - CurrentAccrual.PaySumma;

                    var snum = string.IsNullOrWhiteSpace(Document.DocExtNum)
                        ? Document.DocInNum.ToString()
                        : $"{Document.DocInNum}/{Document.DocExtNum}";
                    var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode,
                        new BankOperationsViewModel
                        {
                            DocCode = UnitOfWork.Context.TD_101.Any()
                                ? UnitOfWork.Context.TD_101.Max(_ => _.DOC_CODE) + 1
                                : 11010000001,
                            Code = UnitOfWork.Context.TD_101.Any() ? UnitOfWork.Context.TD_101.Max(_ => _.CODE) + 1 : 1,
                            Date = DateTime.Today,
                            Currency = Document.Currency,
                            Kontragent = Document.Kontragent,
                            BankOperationType = BankOperationType.Kontragent,
                            Payment = Document.Kontragent,
                            VVT_VAL_RASHOD = s,
                            VVT_VAL_PRIHOD = 0,
                            SHPZ = CurrentAccrual.SDRSchet,
                            VVT_SFACT_CLIENT_DC = null,
                            VVT_SFACT_POSTAV_DC = null,
                            SFName = null,
                            VVT_DOC_NUM = CurrentAccrual.Note,
                            State = RowStatus.NewRow,
                            AccuredId = CurrentAccrual.Id,
                            AccuredInfo = $"Прямой расход №{snum} от {Document.DocDate.ToShortDateString()} " +
                                          $"{CurrentAccrual.Nomenkl}({CurrentAccrual.Nomenkl.NomenklNumber})"
                        },
                        MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
                    if (k != null)
                    {
                        k.State = RowStatus.NewRow;
                        manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                        CurrentAccrual.CashDocs.Add(new AccruedAmountOfSupplierCashDocViewModel
                        {
                            Creator = null,
                            DocCode = k.DocCode,
                            Code = k.Code,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocDate = k.Entity.SD_101.VV_START_DATE,
                            DocNumber = k.VVT_DOC_NUM,
                            DocumentType = "Банковская транзакция",
                            // ReSharper disable once PossibleInvalidOperationException
                            Summa = (decimal)k.VVT_VAL_RASHOD,
                            Note = null
                        });
                    }
                }

                if (Form is AccruedAmountOfSupplierView frm)
                {
                    frm.gridRows.UpdateTotalSummary();
                    frm.gridCashRows.UpdateTotalSummary();
                    CurrentAccrual.RaisePropertyChanged("PaySumma");
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand OpenBankDocCommand
        {
            get { return new Command(OpenBankDoc, _ => true); }
        }

        private void OpenBankDoc(object obj)
        {
            //var CurrentBankAccount = CurrentAccrual.BankDoc.BankAccount;
            //var k = StandartDialogs.OpenBankOperation(CurrentBankAccount.DocCode,
            //    CurrentAccrual.BankDoc,
            //    MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
            //if (k != null)
            //{
            //    var manager = new BankOperationsManager();
            //    k.State = RowStatus.Edited;
            //    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
            //    CurrentAccrual.BankDoc = k;
            //}
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteBankDocCommand
        {
            get { return new Command(DeleteBankDoc, _ => true); }
        }

        private void DeleteBankDoc(object obj)
        {
            //var service = GetService<IDialogService>("WinUIDialogService");
            //if (service.ShowDialog(MessageButton.YesNo, "Запрос", null) == MessageResult.Yes)
            //    CurrentAccrual.BankDoc = null;
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => null;

        public string Error { get; } = null;

        #endregion
    }
}