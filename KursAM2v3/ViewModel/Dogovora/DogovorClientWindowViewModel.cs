using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.EntityViewModel.Dogovora;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Repositories.DogovorsRepositories;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private DogovorClientViewModel myDocument;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;

        #endregion

        #region Constructors

        public DogovorClientWindowViewModel()
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Договор клиенту";
            Document = new DogovorClientViewModel(DogovorClientRepository.CreateNew());
            State = RowStatus.NewRow;
        }

        public DogovorClientWindowViewModel(Guid id)
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Договор клиенту";
            RefreshData(id);
        }

        public override string LayoutName => "DogovorClientView";

        #endregion

        #region Properties

        public DogovorClientViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }
        
        private DogovorClientRowViewModel myCurrentRow;

        public ObservableCollection<DogovorClientRowViewModel> SelectedRows { set; get; }
            = new ObservableCollection<DogovorClientRowViewModel>();

        public DogovorClientRowViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        private Guid myId;

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

        #region Methods

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
        }

        public override void SaveData(object data)
        {
            unitOfWork.CreateTransaction();
            unitOfWork.Save();
            unitOfWork.Commit();
            State = RowStatus.NotEdited;
            RaisePropertiesChanged(nameof(State));
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => "Не определено";

        [Display(AutoGenerateField = false)] public string Error { get; } = "";

        #endregion
    }
}