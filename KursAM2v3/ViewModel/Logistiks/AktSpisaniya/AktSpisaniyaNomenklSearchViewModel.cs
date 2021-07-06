using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Repositories;
using System;
using System.Collections.ObjectModel;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklSearchViewModel : RSWindowViewModelBase
    {

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> BaseRepository;

        public readonly IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklRepository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private AktSpisaniyaNomenklTitleViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        #region Constructors

        public AktSpisaniyaNomenklSearchViewModel()
        {

            BaseRepository = new GenericKursDBRepository<AktSpisaniyaNomenkl_Title>(unitOfWork);
            AktSpisaniyaNomenklRepository = new AktSpisaniyaNomenkl_TitleRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            IsCanDocNew = true;
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LayoutName = "AktSpisaniyaSearchView";
        }


        #endregion

        #region Methods

        #endregion

        #region Properties

        public ObservableCollection<AktSpisaniyaNomenklTitleViewModel> Documents { set; get; } =
            new ObservableCollection<AktSpisaniyaNomenklTitleViewModel>();

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
                if (myDateStart < myDateEnd) return;
                myDateStart = myDateEnd;
                RaisePropertyChanged(nameof(DateStart));
            }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
                if (myDateStart <= myDateEnd) return;
                myDateEnd = myDateStart;
                RaisePropertyChanged(nameof(DateEnd));
            }
        }

        public AktSpisaniyaNomenklTitleViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {

                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

    }
}
