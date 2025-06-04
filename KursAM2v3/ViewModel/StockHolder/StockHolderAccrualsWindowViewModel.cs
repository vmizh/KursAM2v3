using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.Helper;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using KursAM2.Dialogs;
using KursAM2.View.StockHolder;
using KursDomain;
using KursDomain.Documents.StockHolder;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHolderAccrualWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public StockHolderAccrualWindowViewModel()
        {
            repository =
                new StockHolderAccrualsRepository(unitOfWork);
            foreach (var t in unitOfWork.Context.StockHolderAccrualType.ToList())
                StockHolderAccruelTypes.Add(new StockHolderAccrualTypeViewModel(t));
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }


        public StockHolderAccrualWindowViewModel(StockHolderAccrual entity, bool isAddToRepository = false) : this()
        {
            Document = new StockHolderAccrualViewModel(entity);
            if (!isAddToRepository) return;
            unitOfWork.Context.StockHolderAccrual.Add(entity);
        }

        #endregion

        #region Methods

        public void SetAsNewCopy(bool isCopy)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.StockHolderAccrual
                    .Include(_ => _.StockHolderAccrualRows)
                    .Include(_ => _.StockHolderAccrualRows.Select(s1 => s1.StockHolders))
                    .Include(_ => _.StockHolderAccrualRows.Select(s1 => s1.StockHolderAccrualType))
                    .AsNoTracking()
                    .FirstOrDefault(_ => _.Id == Document.Id);
                if (data == null)
                {
                    WinManager.ShowWinUIMessageBox(
                        $"Документ с id \"{Document.Id}\" не найден. Создание копиии не возможно.", "Сообщение",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dtx = new StockHolderAccrualWindowViewModel();
                var newItem = new StockHolderAccrual
                {
                    Id = Guid.NewGuid(),
                    StockHolderAccrualRows = new List<StockHolderAccrualRows>(),
                    Creator = GlobalOptions.UserInfo.NickName,
                    Date = DateTime.Today,
                    Num = -1,
                    Note = data.Note
                };
                dtx.unitOfWork.Context.StockHolderAccrual.Add(newItem);
                if (isCopy)
                    foreach (var r in data.StockHolderAccrualRows)
                    {
                        var newRow = new StockHolderAccrualRows
                        {
                            Id = Guid.NewGuid(),
                            StockHolders = dtx.unitOfWork.Context
                                .StockHolders.FirstOrDefault(_ => _.Id == r.StockHolderId),
                            StockHolderAccrual = newItem,
                            StockHolderAccrualType = dtx.unitOfWork.Context
                                .StockHolderAccrualType.FirstOrDefault(_ => _.Id == r.AcrrualTypeId),
                            AcrrualTypeId = r.AcrrualTypeId,
                            CurrencyDC = r.CurrencyDC,
                            DocId = newItem.Id,
                            Note = r.Note,
                            StockHolderId = r.StockHolderId,
                            Summa = r.Summa
                        };
                        newItem.StockHolderAccrualRows.Add(newRow);
                    }

                var newDoc = new StockHolderAccrualViewModel(newItem)
                {
                    State = RowStatus.NewRow
                };
                dtx.Document = newDoc;

                foreach (var r in dtx.Document.Rows) r.State = RowStatus.NewRow;

                if (isCopy)
                {
                    dtx.StockHolders.Clear();
                    foreach (var r in dtx.Document.Rows.Select(_ => _.StockHolder).Distinct())
                    {
                        if (dtx.StockHolders.Any(_ => _.StockHolder.Id == r.Id)) continue;
                        var newSh = new StockHolderItem
                        {
                            Id = Guid.NewGuid(),
                            StockHolder = r,
                            Accruals = new ObservableCollection<StockHolderAccrualRowViewModel>(
                                dtx.Document.Rows.Where(_ => _.StockHolder.Id == r.Id)),
                            myState = RowStatus.NewRow
                        };
                        dtx.StockHolders.Add(newSh);
                    }
                }

                var view = new StockHolderAccrualsView
                {
                    Owner = Application.Current.MainWindow,
                    DataContext = dtx
                };
                dtx.Form = view;
                view.Show();
            }
        }

        public void SetVisibleColums()
        {
            if (Form is StockHolderAccrualsView frm)
                foreach (var c in frm.GridControlStockHolder.Columns)
                    switch (c.FieldName)
                    {
                        case "SummaRUB":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.RUB));
                            break;
                        case "SummaCHF":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.CHF));
                            break;
                        case "SummaEUR":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.EUR));
                            break;
                        case "SummaGBP":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.GBP));
                            break;
                        case "SummaSEK":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.SEK));
                            break;
                        case "SummaUSD":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.USD));
                            break;
                        case "SummaCNY":
                            c.Visible = StockHolders.Any(_ =>
                                _.Accruals.Any(x => x.Currency.DocCode == CurrencyCode.CNY));
                            break;
                    }
        }

        #endregion

        #region Fields

        private readonly WindowManager WinManager = new WindowManager();

        private readonly IStockHolderAccrualsRepository repository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private StockHolderAccrualViewModel myDocument;
        private StockHolderItem myCurrentcStockHolder;
        private StockHolderAccrualRowViewModel myCurrentAccrual;

        #endregion

        #region Properties

        public override string WindowName => "Ведомость начисления для акционеров";
        public override string LayoutName => "StockHolderAccrualWindowViewModel";

        public override bool IsCanSaveData => Document != null &&
                                              Document.State != RowStatus.NotEdited && Document.Rows.All(_ =>
                                                  _.AccrualType != null
                                                  && _.Currency != null && _.Summa >= 0);

        public ObservableCollection<StockHolderItem> StockHolders { set; get; } =
            new ObservableCollection<StockHolderItem>();

        public ObservableCollection<StockHolderItem> SelectedStockHolders { set; get; } =
            new ObservableCollection<StockHolderItem>();

        public List<StockHolderAccrualTypeViewModel> StockHolderAccruelTypes { set; get; } =
            new List<StockHolderAccrualTypeViewModel>();

        //public List<StockHolderAccrualRowViewModel> DeletedRows = new List<StockHolderAccrualRowViewModel>();

        public StockHolderAccrualViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public StockHolderItem CurrentStockHolder
        {
            get => myCurrentcStockHolder;
            set
            {
                if (myCurrentcStockHolder == value) return;
                myCurrentcStockHolder = value;
                RaisePropertyChanged();
            }
        }


        public StockHolderAccrualRowViewModel CurrentAccrual
        {
            get => myCurrentAccrual;
            set
            {
                if (myCurrentAccrual == value) return;
                myCurrentAccrual = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsDocDeleteAllow => Document?.State != RowStatus.NewRow;

        public override bool IsDocNewCopyAllow => Document.State != RowStatus.NewRow;
        public override bool IsDocNewCopyRequisiteAllow => Document.State != RowStatus.NewRow;

        public override void DocDelete(object form)
        {
            var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    if (Document.State == RowStatus.NewRow)
                    {
                        Form.Close();
                    }
                    else
                    {
                        unitOfWork.CreateTransaction();
                        try
                        {
                            foreach (var r in Document.Rows) unitOfWork.Context.StockHolderAccrualRows.Remove(r.Entity);
                            unitOfWork.Context.StockHolderAccrual.Remove(Document.Entity);
                            unitOfWork.Context.SaveChanges();
                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            WindowManager.ShowError(ex);
                        }
                    }

                    Form.Close();
                    return;
                case MessageBoxResult.No:
                    break;
            }
        }

        public ICommand DeleteStockHolderCommand
        {
            get { return new Command(DeleteStockHolder, _ => CurrentStockHolder != null); }
        }

        private void DeleteStockHolder(object obj)
        {
            foreach (var accr in CurrentStockHolder.Accruals) Document.Rows.Remove(accr);
            CurrentStockHolder.Accruals.Remove(CurrentAccrual);
            if (CurrentStockHolder.State == RowStatus.NewRow)
                foreach (var acc in CurrentStockHolder.Accruals)
                    unitOfWork.Context.Entry(acc.Entity).State = EntityState.Detached;
            else
                foreach (var acc in CurrentStockHolder.Accruals)
                    repository.GenericRepository.Context.StockHolderAccrualRows.Remove(acc.Entity);
            StockHolders.Remove(CurrentStockHolder);
            Document.State = RowStatus.Edited;
        }

        public ICommand AddStockHolderCommand
        {
            get { return new Command(AddStockHolder, _ => true); }
        }

        private void AddStockHolder(object obj)
        {
            var service = this.GetService<IDialogService>("DialogServiceUI");
            var sh = StandartDialogs.SelectStockHolder(service);
            if (sh != null)
            {
                var newStockHolder = new StockHolderItem
                {
                    Id = Guid.NewGuid(),
                    StockHolder = new StockHolderViewModel(sh.Entity),
                    myState = RowStatus.NewRow,
                    Parent = Document
                };
                StockHolders.Add(newStockHolder);
                var newAccrual = new StockHolderAccrualRows
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Summa = 0,
                    CurrencyDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    StockHolderId = sh.Id
                };
                unitOfWork.Context.StockHolderAccrualRows.Add(newAccrual);
                var newAccViewModel = new StockHolderAccrualRowViewModel(newAccrual)
                {
                    State = RowStatus.NewRow
                };
                Document.Rows.Add(newAccViewModel);
                newStockHolder.Accruals.Add(newAccViewModel);
            }

            SetVisibleColums();
        }

        public ICommand AddAllStockHolderCommand
        {
            get { return new Command(AddAllStockHolder, _ => true); }
        }

        private void AddAllStockHolder(object obj)
        {
            foreach (var sh in unitOfWork.Context.StockHolders.ToList())
            {
                if (StockHolders.Any(_ => _.StockHolder.Id == sh.Id)) continue;
                var newStockHolder = new StockHolderItem
                {
                    Id = Guid.NewGuid(),
                    StockHolder = new StockHolderViewModel(sh),
                    myState = RowStatus.NewRow,
                    Parent = Document
                };
                StockHolders.Add(newStockHolder);
                var newAccrual = new StockHolderAccrualRows
                {
                    Id = Guid.NewGuid(),
                    DocId = Document.Id,
                    Summa = 0,
                    CurrencyDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    StockHolderId = sh.Id
                };
                unitOfWork.Context.StockHolderAccrualRows.Add(newAccrual);
                var newAccViewModel = new StockHolderAccrualRowViewModel(newAccrual)
                {
                    State = RowStatus.NewRow
                };
                Document.Rows.Add(newAccViewModel);
                newStockHolder.Accruals.Add(newAccViewModel);
            }

            SetVisibleColums();
        }

        public ICommand DeleteStockHolderAccrualCommand
        {
            get { return new Command(DeleteStockHolderAccrual, _ => CurrentAccrual != null); }
        }

        private void DeleteStockHolderAccrual(object obj)
        {
            if (CurrentAccrual.State == RowStatus.NewRow)
                unitOfWork.Context.Entry(CurrentAccrual.Entity).State = EntityState.Detached;
            else
                repository.GenericRepository.Context.StockHolderAccrualRows.Remove(CurrentAccrual.Entity);
            Document.Rows.Remove(CurrentAccrual);
            CurrentStockHolder.Accruals.Remove(CurrentAccrual);

            if (CurrentStockHolder.Accruals.Count != 0) return;
            if (WinManager.ShowWinUIMessageBox($"Для акционера {CurrentStockHolder} удалены все начисления," +
                                               "удалить акционера из ведомости?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                StockHolders.Remove(CurrentStockHolder);
            Document.State = RowStatus.Edited;
        }

        public ICommand AddStockHolderAccrualCommand
        {
            get { return new Command(AddStockHolderAccrual, _ => CurrentStockHolder != null); }
        }

        private void AddStockHolderAccrual(object obj)
        {
            var newItem = new StockHolderAccrualRows
            {
                Id = Guid.NewGuid(),
                DocId = Document.Id,
                StockHolderId = CurrentStockHolder.StockHolder.Id
            };
            Document.Entity.StockHolderAccrualRows.Add(newItem);
            var newAcc = new StockHolderAccrualRowViewModel(newItem)
                {
                    StockHolder = CurrentStockHolder.StockHolder,
                    Currency = GlobalOptions.SystemProfile.NationalCurrency,
                    Parent = Document,
                    State = RowStatus.NewRow
                }
                ;
            CurrentStockHolder.Accruals.Add(newAcc);
            Document.Rows.Add(newAcc);
            SetVisibleColums();
        }

        public override void DocNewCopy(object form)
        {
            SetAsNewCopy(true);
        }

        public override void DocNewCopyRequisite(object form)
        {
            SetAsNewCopy(false);
        }

        public override void SaveData(object data)
        {
            try
            {
                if (Document.State == RowStatus.NewRow)
                    Document.Num = unitOfWork.Context.StockHolderAccrual.Any()
                        ? unitOfWork.Context.StockHolderAccrual.Max(_ => _.Num) + 1
                        : 1;
                unitOfWork.CreateTransaction();
                unitOfWork.Save();
                unitOfWork.Commit();
                Document.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var res = WinManager.ShowWinUIMessageBox("В документ внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        foreach (var entity in unitOfWork.Context.ChangeTracker.Entries()
                                     .Where(_ => _.State == EntityState.Modified || _.State == EntityState.Detached ||
                                                 _.State == EntityState.Deleted)
                                ) entity.Reload();
                        StockHolders.Clear();
                        foreach (var r in Document.Rows.Select(_ => _.StockHolder).Distinct())
                        {
                            if (StockHolders.Any(_ => _.StockHolder.Id == r.Id)) continue;
                            var newItem = new StockHolderItem
                            {
                                Id = Guid.NewGuid(),
                                StockHolder = r,
                                Accruals = new ObservableCollection<StockHolderAccrualRowViewModel>(
                                    Document.Rows.Where(_ => _.StockHolder.Id == r.Id)),
                                myState = RowStatus.NotEdited
                            };
                            StockHolders.Add(newItem);
                        }

                        foreach (var r in Document.Rows)
                        {
                            r.Parent = Document;
                            r.myState = RowStatus.NotEdited;
                        }

                        Document.State = RowStatus.NotEdited;
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            try
            {
                if (obj == null && Document == null)
                {
                    var newEntity = new StockHolderAccrual
                    {
                        Id = Guid.NewGuid(),
                        Date = DateTime.Today,
                        Num = -1,
                        Creator = GlobalOptions.UserInfo.NickName
                    };
                    unitOfWork.Context.StockHolderAccrual.Add(newEntity);
                    Document = new StockHolderAccrualViewModel(newEntity)
                    {
                        State = RowStatus.NewRow
                    };
                }
                else
                {
                    StockHolders.Clear();
                    //DeletedRows.Clear();
                    if (obj is Guid guid)
                        // ReSharper disable once PossibleNullReferenceException
                        Document = new StockHolderAccrualViewModel(
                            repository.GenericRepository.GetById(guid));

                    if (obj is StockHolderAccrualsView frm)
                        if (frm.DataContext is StockHolderAccrualWindowViewModel dtx)
                            Document = new StockHolderAccrualViewModel(
                                repository.GenericRepository.GetById(dtx.Document.Id));

                    foreach (var r in Document.Rows.Select(_ => _.StockHolder).Distinct())
                    {
                        if (StockHolders.Any(_ => _.StockHolder.Id == r.Id)) continue;
                        var newItem = new StockHolderItem
                        {
                            Id = Guid.NewGuid(),
                            StockHolder = r,
                            Accruals = new ObservableCollection<StockHolderAccrualRowViewModel>(
                                Document.Rows.Where(_ => _.StockHolder.Id == r.Id)),
                            myState = RowStatus.NotEdited
                        };
                        StockHolders.Add(newItem);
                    }

                    foreach (var r in Document.Rows)
                    {
                        r.Parent = Document;
                        r.myState = RowStatus.NotEdited;
                    }

                    Document.State = RowStatus.NotEdited;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return;
            }


            SetVisibleColums();
        }

        #endregion

        #region IDataErrorInfo

        #endregion
    }

    public class StockHolderItem_FluentAPI : DataAnnotationForFluentApiBase,
        IMetadataProvider<StockHolderItem>
    {
        void IMetadataProvider<StockHolderItem>.BuildMetadata(MetadataBuilder<StockHolderItem> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.StockHolder).AutoGenerated().DisplayName("Акционер").ReadOnly();
            builder.Property(_ => _.SummaEUR).AutoGenerated().DisplayName("EUR").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaGBP).AutoGenerated().DisplayName("GBP").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaRUB).AutoGenerated().DisplayName("RUB").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaSEK).AutoGenerated().DisplayName("SEK").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaUSD).AutoGenerated().DisplayName("USD").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaCNY).AutoGenerated().DisplayName("CNY").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.SummaCHF).AutoGenerated().DisplayName("CHF").DisplayFormatString("n2").ReadOnly();
        }
    }

    [MetadataType(typeof(StockHolderItem_FluentAPI))]
    public class StockHolderItem : RSViewModelBase
    {
        public StockHolderViewModel StockHolder { set; get; }

        public ObservableCollection<StockHolderAccrualRowViewModel> Accruals { set; get; } =
            new ObservableCollection<StockHolderAccrualRowViewModel>();

        public decimal SummaCHF => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.CHF).Sum(_ => _.Summa);
        public decimal SummaEUR => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.EUR).Sum(_ => _.Summa);
        public decimal SummaGBP => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.GBP).Sum(_ => _.Summa);
        public decimal SummaRUB => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.RUB).Sum(_ => _.Summa);
        public decimal SummaSEK => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.SEK).Sum(_ => _.Summa);
        public decimal SummaUSD => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.USD).Sum(_ => _.Summa);
        public decimal SummaCNY => Accruals.Where(_ => _.Currency?.DocCode == CurrencyCode.CNY).Sum(_ => _.Summa);
    }
}
