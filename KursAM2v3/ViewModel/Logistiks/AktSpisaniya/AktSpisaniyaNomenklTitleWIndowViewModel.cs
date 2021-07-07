using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.NomenklManagement;

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
            Id = Document.Id;
            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Акт списания";
            Document = new AktSpisaniyaNomenklTitleViewModel(AktSpisaniyaNomenklTitleRepository.CreateNew(), RowStatus.NewRow);
            
        }

        public AktSpisaniyaNomenklTitleWIndowViewModel(Guid id)
        {
            Id = id;
            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklTitleRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Акт списания";
            
            
        }

        public override string LayoutName => "AktSpisaniyaNomenkl_TitleView";

        #endregion

        #region Properties
        public ObservableCollection<AktSpisaniyaRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<AktSpisaniyaRowViewModel>();

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

        #endregion



    }
}
