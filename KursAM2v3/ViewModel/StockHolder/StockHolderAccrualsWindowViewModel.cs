using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.StockHolder;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using KursAM2.Dialogs;
using KursAM2.View.StockHolder;

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

        #endregion

        #region Methods

        //private bool IsAccrualsChanged()
        //{
        //    return StockHolders.Any(sh => sh.Accruals.Any(acr => acr.State != RowStatus.NotEdited));
        //}

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            unitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.Id = newId;
            Document.Date = DateTime.Today;
            Document.Creator = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.Rows.Clear();
            Document.Note = string.Empty;
            Document.Num = -1;
            unitOfWork.Context.StockHolderAccrual.Add(Document.Entity);
            if (isCopy)
            {
                foreach (var row in Document.Rows)
                {
                    unitOfWork.Context.Entry(row.Entity).State = EntityState.Detached;
                    row.Id = Guid.NewGuid();
                    row.DocId = newId;
                    Document.Entity.StockHolderAccrualRows.Add(row.Entity);
                    row.myState = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.StockHolderAccrualRows.Clear();
                }

                Document.Rows.Clear();
            }
        }

        public void SetVisibleColums()
        {
            if (Form is StockHolderAccrualsView frm)
                foreach (var c in frm.GridControlStockHolder.Columns)
                    switch (c.FieldName)
                    {
                        case "SummaRUB":
                            c.Visible = StockHolders.Sum(_ => _.SummaRUB) != 0;
                            break;
                        case "SummaCHF":
                            c.Visible = StockHolders.Sum(_ => _.SummaCHF) != 0;
                            break;
                        case "SummaEUR":
                            c.Visible = StockHolders.Sum(_ => _.SummaEUR) != 0;
                            break;
                        case "SummaGBP":
                            c.Visible = StockHolders.Sum(_ => _.SummaGBP) != 0;
                            break;
                        case "SummaSEK":
                            c.Visible = StockHolders.Sum(_ => _.SummaSEK) != 0;
                            break;
                        case "SummaUSD":
                            c.Visible = StockHolders.Sum(_ => _.SummaUSD) != 0;
                            break;
                        case "SummaCNY":
                            c.Visible = StockHolders.Sum(_ => _.SummaCNY) != 0;
                            break;
                    }
        }

        #endregion

        #region Fields

        private readonly IStockHolderAccrualsRepository repository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private bool isHolderAccrualsDeleted;
        private StockHolderAccrualViewModel myDocument;
        private StockHolderItem myCurrentcStockHolder;
        private StockHolderAccrualRowViewModel myCurrentAccrual;

        #endregion

        #region Properties

        public override string WindowName => "Ведомость начисления для акционеров";
        public override string LayoutName => "StockHolderAccrualWindowViewModel";

        public override bool IsCanSaveData => Document != null &&
                                              (Document.State != RowStatus.NotEdited 
                                              || isHolderAccrualsDeleted);

        public ObservableCollection<StockHolderItem> StockHolders { set; get; } =
            new ObservableCollection<StockHolderItem>();

        public ObservableCollection<StockHolderItem> SelectedStockHolders { set; get; } =
            new ObservableCollection<StockHolderItem>();

        public List<StockHolderAccrualTypeViewModel> StockHolderAccruelTypes { set; get; } =
            new List<StockHolderAccrualTypeViewModel>();

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
            CurrentStockHolder.Accruals.Remove(CurrentAccrual);
            StockHolders.Remove(CurrentStockHolder);
            if (CurrentStockHolder.State == RowStatus.NewRow)
            {
                foreach (var acc in CurrentStockHolder.Accruals)
                    unitOfWork.Context.Entry(acc.Entity).State = EntityState.Detached;
            }
            else
            {
                isHolderAccrualsDeleted = true;
                foreach (var acc in CurrentStockHolder.Accruals)
                    repository.GenericRepository.Context.StockHolderAccrualRows.Remove(acc.Entity);
            }
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
                StockHolders.Add(new StockHolderItem
                {
                    Id = Guid.NewGuid(),
                    StockHolder = new StockHolderViewModel(sh.Entity),
                    myState = RowStatus.NewRow,
                    Parent = Document
                });
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
                StockHolders.Add(new StockHolderItem
                {
                    Id = Guid.NewGuid(),
                    StockHolder = new StockHolderViewModel(sh),
                    myState = RowStatus.NewRow,
                    Parent = Document
                });
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
            {
                unitOfWork.Context.Entry(CurrentAccrual.Entity).State = EntityState.Detached;
            }
            else
            {
                isHolderAccrualsDeleted = true;
                repository.GenericRepository.Context.StockHolderAccrualRows.Remove(CurrentAccrual.Entity);
            }

            CurrentStockHolder.Accruals.Remove(CurrentAccrual);
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
            CurrentStockHolder.Accruals.Add(new StockHolderAccrualRowViewModel(newItem)
            {
                StockHolder = CurrentStockHolder.StockHolder,
                Currency = GlobalOptions.SystemProfile.NationalCurrency,
                Parent = Document,
                State = RowStatus.NewRow
            });
            SetVisibleColums();
        }

        public override void DocNewCopy(object form)
        {
            var ctx = new StockHolderAccrualWindowViewModel();
            ctx.RefreshData(null);
            ctx.Document.Creator = GlobalOptions.UserInfo.NickName;
            ctx.Document.Date = DateTime.Today;
            ctx.Document.Note = Document.Note;
            ctx.Document.Num = -1;

            foreach (var acc in Document.Entity.StockHolderAccrualRows)
            {
                var newAcc = new StockHolderAccrualRows
                {
                    Id = Guid.NewGuid(),
                    DocId = ctx.Document.Id,
                    AcrrualTypeId = acc.AcrrualTypeId,
                    CurrencyDC = acc.CurrencyDC,
                    Note = acc.Note,
                    StockHolderId = acc.StockHolderId,
                    Summa = acc.Summa
                };
                ctx.Document.Entity.StockHolderAccrualRows.Add(newAcc);
                ctx.Document.Rows.Add(new StockHolderAccrualRowViewModel(newAcc)
                {
                    myState = RowStatus.NewRow
                });
            }

            foreach (var acc in ctx.Document.Entity.StockHolderAccrualRows)
            {
                if (ctx.StockHolders.Any(_ => _.StockHolder.Id == acc.StockHolderId)) continue;
                var newItem = new StockHolderItem
                {
                    Id = Guid.NewGuid(),
                    Parent = ctx.Document,
                    StockHolder = new StockHolderViewModel(acc.StockHolders),
                    myState = RowStatus.NewRow
                };
                foreach (var a in ctx.Document.Entity.StockHolderAccrualRows
                             .Where(_ => _.StockHolderId == newItem.StockHolder.Id))
                    newItem.Accruals.Add(new StockHolderAccrualRowViewModel(a)
                    {
                        Id = Guid.NewGuid(),
                        DocId = ctx.Document.Id,
                        myState = RowStatus.NewRow
                    });
                ctx.StockHolders.Add(newItem);
            }

            var frm = new StockHolderAccrualsView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
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
                isHolderAccrualsDeleted = false;
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
                        foreach (var entity in unitOfWork.Context.ChangeTracker.Entries().Where(_ => _.State == EntityState.Modified )) entity.Reload();
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

                        foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                        Document.myState = RowStatus.NotEdited;
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            try
            {
                isHolderAccrualsDeleted = false;
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

                    foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                    Document.myState = RowStatus.NotEdited;
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
