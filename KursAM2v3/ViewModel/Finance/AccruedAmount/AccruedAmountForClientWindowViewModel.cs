using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.AccruedAmount;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Utils.Extensions;
using KursAM2.Dialogs;
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
                            {
                                UnitOfWork.Context.AccuredAmountForClientRow.Remove(r.Entity);
                            }

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