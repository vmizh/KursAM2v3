using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.AccruedAmount;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Utils.Extensions;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    public sealed class AccruedAmountForClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Constructors

        public AccruedAmountForClientWindowViewModel(Guid? id)
        {
            GenericRepository = new GenericKursDBRepository<AccruedAmountForClient>(UnitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new AccruedAmountForClientViewModel {State = RowStatus.NewRow};
                UnitOfWork.Context.AccruedAmountForClient.Add(Document.Entity);
            }
            else
            {
                Document = new AccruedAmountForClientViewModel(doc)
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
            UnitOfWork.Context.AccruedAmountForClient.Add(Document.Entity);
            if (isCopy)
            {
                foreach (var row in Document.Rows)
                {
                    UnitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    Document.Entity.AccuredAmountForClientRow.Add(row.Entity);
                    row.myState = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    UnitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.AccuredAmountForClientRow.Clear();
                }

                Document.Rows.Clear();
            }
        }

        #endregion

        #region Fields

        public readonly GenericKursDBRepository<AccruedAmountForClient> GenericRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private AccruedAmountForClientViewModel myDocument;
        private AccruedAmountForClientRowViewModel myCurrentAccrual;


        #endregion

        #region Properties

        public override string LayoutName => "AccruedAmountForClientWindowViewModel";
        public override string WindowName => Document.ToString();

        public ObservableCollection<AccruedAmountForClientRowViewModel> SelectedRows { set; get; } =
            new ObservableCollection<AccruedAmountForClientRowViewModel>();

        public ObservableCollection<AccruedAmountForClientRowViewModel> DeletedRows { set; get; } =
            new ObservableCollection<AccruedAmountForClientRowViewModel>();

        public AccruedAmountForClientRowViewModel CurrentAccrual
        {
            get => myCurrentAccrual;
            set
            {
                if (myCurrentAccrual == value) return;
                myCurrentAccrual = value;
                RaisePropertyChanged();
            }
        }

        public AccruedAmountForClientViewModel Document
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
            $"Внебалансовые начисления для клиентов №{Document.DocInNum}/{Document.DocExtNum} " +
            $"от {Document.DocDate.ToShortDateString()} Контрагент: {Document.Kontragent} на сумму {Document.Summa} " +
            $"{Document.Currency}";

        #endregion

        #region Commands

        public override bool IsCanSaveData =>
            (Document.State != RowStatus.NotEdited || DeletedRows.Count > 0
                                                   || Document.Rows.Any(_ => _.State != RowStatus.NotEdited)) &&
            Document.Error == null;

        [Display(AutoGenerateField = false)]
        public ICommand AddCashDocCommand
        {
            get
            {
                return new Command(AddCashDoc, _ => CurrentAccrual != null && CurrentAccrual.CashDoc == null
                                                                           && CurrentAccrual.BankDoc == null
                                                                           && CurrentAccrual.SDRSchet != null
                                                                           && CurrentAccrual.State != RowStatus.NewRow
                                                                           && CurrentAccrual.Summa > 0);
            }
        }

        private void AddCashDoc(object obj)
        {
            var ctx = new SelectCashBankDialogViewModel(true);
            var dlg = new DialogSelectCashBankView
            {
                DataContext = ctx
            };
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!(dlg.DialogResult ?? false)) return;
            var cash = ctx.CurrentObject;

            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCashIn()
            };
            vm.Document.Cash = MainReferences.Cashs[cash.DocCode];


            vm.Document.Cash = MainReferences.Cashs[cash.DocCode];
            vm.Document.KontragentType = CashKontragentType.Kontragent;
            vm.Document.KONTRAGENT_DC = Document.Kontragent.DocCode;
            vm.Document.Currency = Document.Kontragent.BalansCurrency;
            vm.Document.SUMM_ORD = CurrentAccrual.Summa;
            vm.Document.AcrruedAmountRow = CurrentAccrual.Entity;
            vm.Document.SDRSchet = CurrentAccrual.SDRSchet;
            vm.SaveData(null);
            var ord = UnitOfWork.Context.SD_33.FirstOrDefault(_ => _.DOC_CODE == vm.Document.DocCode);
            if (ord != null)
                CurrentAccrual.CashDoc = new CashIn(ord);
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
        }

        [Display(AutoGenerateField = false)]
        public ICommand OpenCashDocCommand
        {
            get { return new Command(OpenCashDoc, _ => CurrentAccrual?.CashDoc != null); }
        }

        private void OpenCashDoc(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.CashIn, CurrentAccrual.CashDoc.DocCode, null, Form);
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteCashDocCommand
        {
            get { return new Command(DeleteCashDoc, _ => CurrentAccrual?.CashDoc != null); }
        }

        private void DeleteCashDoc(object obj)
        {
            IDialogService service = GetService<IDialogService>("WinUIDialogService");
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", null)== MessageResult.Yes)
            {
                CurrentAccrual.CashDoc = null;
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddBankDocCommand
        {
            get
            {
                return new Command(AddBankDoc, _ => CurrentAccrual != null && CurrentAccrual.CashDoc == null
                                                                           && CurrentAccrual.BankDoc == null
                                                                           && CurrentAccrual.SDRSchet != null
                                                                           && CurrentAccrual.State != RowStatus.NewRow
                                                                           && CurrentAccrual.Summa > 0);
            }
        }

        private void AddBankDoc(object obj)
        {
            var ctx = new SelectCashBankDialogViewModel(false);
            var dlg = new DialogSelectCashBankView
            {
                DataContext = ctx
            };
            ctx.Form = dlg;
            dlg.ShowDialog();
            if (!(dlg.DialogResult ?? false)) return;
            var CurrentBankAccount = ctx.CurrentObject;
            if (CurrentBankAccount != null)
            {
                BankOperationsManager manager = new BankOperationsManager();
                var k = StandartDialogs.AddNewBankOperation(CurrentBankAccount.DocCode, 
                    new BankOperationsViewModel
                    {
                        DocCode = -1,
                        Code = -1,
                        Date = DateTime.Today,
                        Currency = Document.Currency,
                        Kontragent = Document.Kontragent,
                        BankOperationType = BankOperationType.Kontragent,
                        Payment = Document.Kontragent,
                        VVT_VAL_RASHOD = 0,
                        VVT_VAL_PRIHOD = CurrentAccrual.Summa,
                        SHPZ = CurrentAccrual.SDRSchet,
                        VVT_SFACT_CLIENT_DC = null,
                        VVT_SFACT_POSTAV_DC = null,
                        SFName = null,
                        VVT_DOC_NUM = CurrentAccrual.Note,
                        State = RowStatus.NewRow 
                    },
                    MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    CurrentAccrual.BankDoc = k;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand OpenBankDocCommand
        {
            get { return new Command(OpenBankDoc, _ => CurrentAccrual?.BankDoc != null); }
        }

        private void OpenBankDoc(object obj)
        {
            var CurrentBankAccount = CurrentAccrual.BankDoc.BankAccount;
            var k = StandartDialogs.OpenBankOperation(CurrentBankAccount.DocCode,
                CurrentAccrual.BankDoc,
                MainReferences.BankAccounts[CurrentBankAccount.DocCode]);
            if (k != null)
            {
                BankOperationsManager manager = new BankOperationsManager();
                k.State = RowStatus.Edited;
                manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                CurrentAccrual.BankDoc = k;
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteBankDocCommand
        {
            get { return new Command(DeleteBankDoc, _ => CurrentAccrual?.BankDoc != null); }
        }

        private void DeleteBankDoc(object obj)
        {
            IDialogService service = GetService<IDialogService>("WinUIDialogService");
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", null)== MessageResult.Yes)
            {
                CurrentAccrual.BankDoc = null;
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => true); }
        }

        private void KontragentSelect(object obj)
        {
            var kontr = StandartDialogs.SelectKontragent(null, false);
            if (kontr == null) return;
            Document.Kontragent = kontr;
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddAccrualCommand
        {
            get { return new Command(AddAccrual, _ => true); }
        }

        private void AddAccrual(object obj)
        {
            var newItem = new AccruedAmountForClientRowViewModel(null, Document)
            {
                State = RowStatus.NewRow
            };
            if (Document.State != RowStatus.NewRow)
            {
                Document.myState = RowStatus.Edited;
                Document.RaisePropertyChanged("State");
            }

            Document.Entity.AccuredAmountForClientRow.Add(newItem.Entity);
            Document.Rows.Add(newItem);
            CurrentAccrual = newItem;
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteAccrualCommand
        {
            get { return new Command(DeleteAccrual, _ => CurrentAccrual != null); }
        }

        private void DeleteAccrual(object obj)
        {
            if (CurrentAccrual.State != RowStatus.NewRow)
                DeletedRows.Add(CurrentAccrual);
            UnitOfWork.Context.AccuredAmountForClientRow.Remove(CurrentAccrual.Entity);
            Document.Rows.Remove(CurrentAccrual);
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
                        SaveData(null);
                        return;
                    case MessageBoxResult.No:
                        foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        RaiseAll();
                        Document.myState = RowStatus.NotEdited;
                        return;
                }
            }

            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();

            RaiseAll();

            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

            Document.myState = RowStatus.NotEdited;
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
                                UnitOfWork.Context.AccuredAmountForClientRow.Remove(r.Entity);

                            UnitOfWork.Context.AccruedAmountForClient.Remove(Document.Entity);
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
                Document.DocInNum = UnitOfWork.Context.AccruedAmountForClient.Any()
                    ? UnitOfWork.Context.AccruedAmountForClient.Max(_ => _.DocInNum) + 1
                    : 1;
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            RecalcKontragentBalans.CalcBalans(Document.Kontragent.DOC_CODE, Document.DocDate);
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
            ParentFormViewModel?.RefreshData(null);
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => null;

        public string Error { get; } = null;

        #endregion
    }
}