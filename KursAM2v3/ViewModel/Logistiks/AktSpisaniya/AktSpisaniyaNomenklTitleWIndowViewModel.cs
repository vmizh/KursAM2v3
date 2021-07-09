using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Dialogs;
using KursAM2.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.EntityViewModel.NomenklManagement;
using KursAM2.Managers.Nomenkl;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklTitleWIndowViewModel : RSWindowViewModelBase
    {
        #region Methods

        public override bool IsCorrect()
        {
            return Document.IsCorrect();
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

        #endregion

        #region Fields

        private AktSpisaniyaNomenklTitleViewModel myDocument;
        private Guid myId;
        private AktSpisaniyaRowViewModel myCurrentRow;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> BaseRepository;
        public readonly IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklTitleRepository;

        #endregion

        #region Constructors

        public AktSpisaniyaNomenklTitleWIndowViewModel()
        {
            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Акт списания";
            Document = new AktSpisaniyaNomenklTitleViewModel(AktSpisaniyaNomenklTitleRepository.CreateNew(), RowStatus.NewRow);

        }

        public AktSpisaniyaNomenklTitleWIndowViewModel(Guid id)
        {
            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            RefreshData(id);

        }

        public override string LayoutName => "AktSpisaniyaNomenklTitleView";

        #endregion

        #region Properties
        public override string WindowName =>
            Document == null ? "Акт списания" :
            $"Акт списания №{Document?.DocNumber} от {Document?.DocDate}";

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<AktSpisaniyaRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<AktSpisaniyaRowViewModel>();

        public List<Core.EntityViewModel.NomenklManagement.Warehouse>
            WarehouseList
        { set; get; } = MainReferences.Warehouses.Values.OrderBy(_ => _.Name).ToList();

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
            Document != null && Document.State != RowStatus.NotEdited;

        public override void RefreshData(object obj)
        {
            if (Document != null)
            {
                Document = new AktSpisaniyaNomenklTitleViewModel(
                    AktSpisaniyaNomenklTitleRepository.GetByGuidId(Document.Id));
                foreach (var r in Document.Rows)
                {
                    r.Price = NomenklManager.NomenklPrice(r.Nomenkl.DocCode, Document.DocDate).Price;
                }
            }
            RaisePropertiesChanged(nameof(Document));
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        private void AddNomenkl(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;

            var nomenkls = StandartDialogs.SelectNomenkls(null, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            foreach (var n in nomenkls)
                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                {
                    var nPrice = Nomenkl.Price(n.DocCode, Document.DocDate);
                    Document.Rows.Add(new AktSpisaniyaRowViewModel()
                    {
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n,
                        Quantity = 1,
                        Price = nPrice,
                        Note = "Списание",
                        Code = newCode,
                        State = RowStatus.NewRow
                    });
                }

            RaisePropertyChanged(nameof(Document));
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
                    Document.Rows.Remove(row);
                    unitOfWork.Context.Entry(row.Entity).State =
                        unitOfWork.Context.Entry(row.Entity).State == EntityState.Added
                            ? EntityState.Detached
                            : EntityState.Deleted;

                }

                Document.State = RowStatus.Edited;
            }
        }

        #endregion



    }
}
