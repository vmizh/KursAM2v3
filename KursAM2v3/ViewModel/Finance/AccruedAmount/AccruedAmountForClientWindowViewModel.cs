using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursDomain.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils.Extensions;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.ViewModel.Finance.Cash;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.AccruedAmount;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

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
            //DialogService = GetService<IDialogService>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new AccruedAmountForClientViewModel { State = RowStatus.NewRow };
                UnitOfWork.Context.AccruedAmountForClient.Add(Document.Entity);
            }
            else
            {
                Document = new AccruedAmountForClientViewModel(doc)
                {
                    myState = RowStatus.NotEdited
                };

                if (Document != null)
                    WindowName = Document.ToString();
                Document.Rows.ForEach(_ => _.myState = RowStatus.NotEdited);
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

        public List<CentrResponsibility> COList => GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll()
            .Cast<CentrResponsibility>().OrderBy(_ => _.Name).ToList();

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
                var frm = Form as AccruedAmountForClientView;
                if (frm == null || myCurrentAccrual == null) return;
                var col = frm.gridRows.Columns.FirstOrDefault(_ => _.FieldName == "Summa");
                if (col == null) return;
                col.ReadOnly = myCurrentAccrual.CashDoc != null || myCurrentAccrual.BankDoc != null;
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
            $"Прямой расход для клиентов №{Document.DocInNum}/{Document.DocExtNum} " +
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
            var ctx = new SelectCashBankDialogViewModel(true, Document.Currency);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            if (ctx.CurrentObject == null) return;
            var cash = ctx.CurrentObject;

            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCashIn()
            };
            vm.Document.Cash = GlobalOptions.ReferencesCache.GetCashBox(cash.DocCode) as CashBox;


            vm.Document.Cash = GlobalOptions.ReferencesCache.GetCashBox(cash.DocCode) as CashBox;
            vm.Document.KontragentType = CashKontragentType.Kontragent;
            vm.Document.KONTRAGENT_DC = Document.Kontragent.DocCode;
            vm.Document.Currency = Document.Kontragent.Currency as Currency;
            vm.Document.SUMM_ORD = CurrentAccrual.Summa;
            vm.Document.AcrruedAmountRow = CurrentAccrual.Entity;
            vm.Document.SDRSchet = CurrentAccrual.SDRSchet;
            vm.SaveData(null);
            var ord = UnitOfWork.Context.SD_33.FirstOrDefault(_ => _.DOC_CODE == vm.Document.DocCode);
            if (ord != null)
                CurrentAccrual.CashDoc = new CashInViewModel(ord);
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
            if (Form is AccruedAmountForClientView frm)
                frm.gridRows.UpdateTotalSummary();
        }

        public ICommand OpenPrihodDocCommand
        {
            get
            {
                return new Command(OpenPrihodDoc,
                    _ => CurrentAccrual?.BankDoc != null || CurrentAccrual?.CashDoc != null);
            }
        }

        [Display(AutoGenerateField = false)]
        private void OpenPrihodDoc(object obj)
        {
            if (CurrentAccrual.BankDoc != null)
            {
                var CurrentBankAccount = CurrentAccrual.BankDoc.BankAccount;
                var k = StandartDialogs.OpenBankOperation(CurrentBankAccount.DocCode,
                    CurrentAccrual.BankDoc,
                    GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount);
                if (k != null)
                {
                    var manager = new BankOperationsManager();
                    k.State = RowStatus.Edited;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    CurrentAccrual.BankDoc = k;
                }
            }

            if (CurrentAccrual.CashDoc != null)
                DocumentsOpenManager.Open(DocumentType.CashIn, CurrentAccrual.CashDoc.DocCode, null, Form);
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteLinkPrihodDocCommand
        {
            get
            {
                return new Command(DeleteLinkPrihodDoc,
                    _ => CurrentAccrual?.BankDoc != null || CurrentAccrual?.CashDoc != null);
            }
        }

        private void DeleteLinkPrihodDoc(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Вы действительно хотите удалить связь с платежным документом?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.Yes)
            {
                if (CurrentAccrual.BankDoc != null)
                    CurrentAccrual.BankDoc = null;
                if (CurrentAccrual.CashDoc != null)
                    CurrentAccrual.CashDoc = null;
            }

            if (Form is AccruedAmountForClientView frm)
                frm.gridRows.UpdateTotalSummary();
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
            var ctx = new SelectCashBankDialogViewModel(false, Document.Currency);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            if (ctx.CurrentObject == null) return;
            var CurrentBankAccount = ctx.CurrentObject;
            if (CurrentBankAccount != null)
            {
                var manager = new BankOperationsManager();
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
                        VVT_VAL_RASHOD = 0,
                        VVT_VAL_PRIHOD = CurrentAccrual.Summa,
                        SHPZ = CurrentAccrual.SDRSchet,
                        VVT_SFACT_CLIENT_DC = null,
                        VVT_SFACT_POSTAV_DC = null,
                        SFName = null,
                        VVT_DOC_NUM = CurrentAccrual.Note,
                        State = RowStatus.NewRow
                    },
                    GlobalOptions.ReferencesCache.GetBankAccount(CurrentBankAccount.DocCode) as BankAccount);

                if (k != null)
                {
                    k.State = RowStatus.NewRow;
                    manager.SaveBankOperations(k, CurrentBankAccount.DocCode, 0);
                    CurrentAccrual.BankDoc = k;
                }

                if (Form is AccruedAmountForClientView frm)
                    frm.gridRows.UpdateTotalSummary();
            }
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
            updateKontragent();
            RaisePropertyChanged(nameof(WindowName));
        }

        private void updateKontragent()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var row in Document.Rows)
                {
                    if (row.BankDoc is not null)
                    {
                        var bank = ctx.TD_101.FirstOrDefault(_ => _.CODE == row.BankDoc.Code);
                        if (bank is not null)
                        {
                            bank.VVT_KONTRAGENT = Document.Kontragent.DocCode;
                        }
                    }

                    if (row.CashDoc is not null)
                    {
                        var cash = ctx.SD_33.FirstOrDefault(_ => _.DOC_CODE == row.CashDoc.DocCode);
                        if (cash is not null)
                        {
                            cash.KONTRAGENT_DC = Document.Kontragent.DocCode;
                        }
                    }
                }

                ctx.SaveChanges();
            }
        }

        [Display(AutoGenerateField = false)]
        public ICommand AddAccrualCommand
        {
            get { return new Command(AddAccrual, _ => Document.Kontragent != null); }
        }

        private void AddAccrual(object obj)
        {
            var k = StandartDialogs.SelectNomenkls(Document.Currency);
            if (k != null)
                foreach (var item in k)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var newItem = new AccruedAmountForClientRowViewModel(null, Document)
                    {
                        Nomenkl = item,
                        State = RowStatus.NewRow
                    };
                    // ReSharper disable once PossibleNullReferenceException
                    if (Document.State != RowStatus.NewRow)
                    {
                        Document.myState = RowStatus.Edited;
                        Document.RaisePropertyChanged("State");
                    }

                    Document.Entity.AccuredAmountForClientRow.Add(newItem.Entity);
                    Document.Rows.Add(newItem);
                    CurrentAccrual = newItem;
                }
        }

        [Display(AutoGenerateField = false)]
        public ICommand DeleteAccrualCommand
        {
            get { return new Command(DeleteAccrual, _ => CurrentAccrual != null); }
        }

        private void DeleteAccrual(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Хотите удалить выделенные строки?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.No)
                return;
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
                var service = this.GetService<IDialogService>("WinUIDialogService");
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
            Document.RaisePropertyChanged("State");
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.NewRow)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "Вы уверены, что хотите удалить данный документ?";
                var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
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
                Document.DocInNum = UnitOfWork.Context.AccruedAmountForClient.Any()
                    ? UnitOfWork.Context.AccruedAmountForClient.Max(_ => _.DocInNum) + 1
                    : 1;
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            RecalcKontragentBalans.CalcBalans(Document.Kontragent.DocCode, Document.DocDate);
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
            ParentFormViewModel?.RefreshData(null);
        }

        public override void DocNewCopy(object form)
        {
            var ctx = new AccruedAmountForClientWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AccruedAmountForClientWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var ctx = new AccruedAmountForClientWindowViewModel(Document.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => null;

        public string Error { get; } = null;

        #endregion
    }
}
