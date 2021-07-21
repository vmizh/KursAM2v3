using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.PivotGrid.ServerMode.Queryable;
using Helper;
using KursAM2.Managers.Nomenkl;
using KursAM2.Repositories;
using KursAM2.View.Logistiks.AktSpisaniya;

// ReSharper disable IdentifierTypo

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklTitleWIndowViewModel : RSWindowViewModelBase
    {
        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect();
        }

        private void RaiseAll()
        {
            if (Document != null)
            {
                Document.RaisePropertyAllChanged();
                foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
            }
        }

        public void SetAsNewCopy(bool isCopy)
        {
            var newId = Guid.NewGuid();
            unitOfWork.Context.Entry(Document.Entity).State = EntityState.Detached;
            Document.DocCode = -1;
            Document.DocDate = DateTime.Today;
            Document.DocCreator = GlobalOptions.UserInfo.Name;
            Document.myState = RowStatus.NewRow;
            Document.Id = newId;
            unitOfWork.Context.AktSpisaniyaNomenkl_Title.Add(Document.Entity);
            if (isCopy)
            {
                var newCode = 1;
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    item.DocCode = -1;
                    item.Id = Guid.NewGuid();
                    item.DocId = newId;
                    item.DocCode = newCode;
                    item.Nomenkl = item.Nomenkl;
                    item.State = RowStatus.NewRow;
                    newCode++;
                }

                foreach (var r in Document.Rows)
                {
                    unitOfWork.Context.AktSpisaniya_row.Add(r.Entity);
                    r.State = RowStatus.NewRow;
                }
            }
            else
            {
                foreach (var item in Document.Rows)
                {
                    unitOfWork.Context.Entry(item.Entity).State = EntityState.Detached;
                    Document.Entity.AktSpisaniya_row.Clear();
                }

                Document.Rows.Clear();
            }
        }

        public void ResetVisibleCurrency()
        {
            if (!(Form is AktSpisaniyaView f)) return;
            var bandCrs = f.gridRows.Bands.FirstOrDefault(_ => (string) _.Header == "Валюта");
            if (bandCrs != null)
                foreach (var b in bandCrs.Bands)
                {
                    b.Visible = Document.Rows.Any(_ => _.Currency.Name == (string)b.Header);
                }
        }

        #endregion

        #region Fields

        // ReSharper disable once InconsistentNaming
        private AktSpisaniyaNomenklTitleViewModel myDocument;

        // ReSharper disable once InconsistentNaming
        private Guid myId;

        // ReSharper disable once InconsistentNaming
        private AktSpisaniyaRowViewModel myCurrentRow;

        // ReSharper disable once InconsistentNaming
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> GenericRepository;
        public readonly IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklTitleRepository;

        #endregion

        #region Constructors

        public AktSpisaniyaNomenklTitleWIndowViewModel()
        {
            GenericRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Акт списания";
            Document = new AktSpisaniyaNomenklTitleViewModel(AktSpisaniyaNomenklTitleRepository.CreateNew(),
                RowStatus.NewRow);
        }

        public AktSpisaniyaNomenklTitleWIndowViewModel(Guid? id)
        {
            GenericRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var doc = id != null ? GenericRepository.GetById(id.Value) : null;
            if (doc == null)
            {
                Document = new AktSpisaniyaNomenklTitleViewModel {State = RowStatus.NewRow};
                unitOfWork.Context.AktSpisaniyaNomenkl_Title.Add(Document.Entity);
            }
            else
            {
                Document = new AktSpisaniyaNomenklTitleViewModel(doc)
                {
                    State = RowStatus.NotEdited
                };
                if (Document != null)
                    WindowName = Document.ToString();
                foreach (var r in Document.Rows)
                {
                    r.Prices = NomenklManager.NomenklPrice(r.Nomenkl.DocCode, Document.DocDate);
                    r.MaxQuantity = r.Quantity + NomenklManager.GetNomenklCount(Document.DocDate, r.Nomenkl.DocCode); 
                    r.RaisePropertyAllChanged();
                    r.myState = RowStatus.NotEdited;
                }
                Document.myState = RowStatus.NotEdited;
            }
        }

        public override string LayoutName => "AktSpisaniyaNomenklTitleView";

        #endregion

        #region Properties

        public override string WindowName =>
            Document == null ? "Акт списания" : $"Акт списания №{Document?.DocNumber} от {Document?.DocDate}";

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<AktSpisaniyaRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<AktSpisaniyaRowViewModel>();

        public List<Core.EntityViewModel.NomenklManagement.Warehouse> WarehouseList { set; get; }
            = MainReferences.Warehouses.Values.OrderBy(_ => _.Name).ToList();

        public AktSpisaniyaNomenklTitleViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value)
                    return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public AktSpisaniyaRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value)
                    return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => myId;
            set
            {
                if (myId == value) return;
                myId = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Command

        public override bool IsCanSaveData =>
            Document != null && Document.State != RowStatus.NotEdited
                             && Document.Rows.All(_ => _.Quantity <= _.MaxQuantity);

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
                switch (res)
                {
                    case MessageResult.Yes:
                        SaveData(null);
                        return;
                }
            }
            foreach (var id in Document.Rows.Where(_ => _.State == RowStatus.NewRow).Select(_ => _.Id)
                .ToList())
            {
                Document.Rows.Remove(Document.Rows.Single(_ => _.Id == id));
            }
            EntityManager.EntityReload(unitOfWork.Context);
            foreach (var r in Document.Rows)
            {
                r.Prices = NomenklManager.NomenklPrice(r.Nomenkl.DocCode, Document.DocDate);
                r.MaxQuantity = r.Quantity + NomenklManager.GetNomenklCount(Document.DocDate, r.Nomenkl.DocCode);
                r.myState = RowStatus.NotEdited;
                r.RaisePropertyAllChanged();
            }
            RaiseAll();
            Document.myState = RowStatus.NotEdited;
            
            ResetVisibleCurrency();
        }

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
                Document.DocNumber = unitOfWork.Context.AccruedAmountForClient.Any()
                    ? unitOfWork.Context.AccruedAmountForClient.Max(_ => _.DocInNum) + 1
                    : 1;
            unitOfWork.CreateTransaction();
            unitOfWork.Save();
            unitOfWork.Commit();
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;

            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
            //ParentFormViewModel?.RefreshData(null);
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document.Warehouse != null); }
        }

        private void AddNomenkl(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var ctx = new DialogSelectExistNomOnSkaldViewModel(Document.Warehouse, Document.DocDate);
            var service = GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            if (ctx.NomenklSelectedList.Count == 0) return;
            foreach (var n in ctx.NomenklSelectedList)
            {
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.Nomenkl.DocCode))
                {
                    var nPrice = NomenklManager.NomenklPrice(n.Nomenkl.DocCode, Document.DocDate);
                    var newItem = new AktSpisaniyaRowViewModel
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n.Nomenkl,
                        Quantity = 1,
                        MaxQuantity = n.Quantity,
                        Prices = nPrice,
                        Code = newCode,
                        State = RowStatus.NewRow,
                        Parent = this
                    };
                    Document.Rows.Add(newItem);
                    Document.Entity.AktSpisaniya_row.Add(newItem.Entity);
                    if (Document.State != RowStatus.NewRow)
                        Document.State = RowStatus.Edited;
                }

                newCode++;
            }

            ResetVisibleCurrency();
            RaisePropertyChanged(nameof(Document));
        }

        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteRow, _ => CurrentRow != null || SelectedRows.Count > 1); }
        }

        private void DeleteRow(object obj)
        {
            var service = GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Хотите удалить выделенные строки?";
            if (service.ShowDialog(MessageButton.YesNo, "Запрос", this) == MessageResult.No)
                return;
            var list = SelectedRows.Select(_ => _.Id).ToList();
            foreach (var id in list)
            {
                var row = Document.Rows.Single(_ => _.Id == id);
                Document.Rows.Remove(row);
                unitOfWork.Context.Entry(row.Entity).State =
                    unitOfWork.Context.Entry(row.Entity).State == EntityState.Added
                        ? EntityState.Detached
                        : EntityState.Deleted;
            }

            Document.State = RowStatus.Edited;
            ResetVisibleCurrency();
        }

        #endregion
    }
}