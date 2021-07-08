using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.AktSpisaniya;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.AktSpisaniya;

namespace KursAM2.ViewModel.Logistiks.AktSpisaniya
{
    public sealed class AktSpisaniyaNomenklSearchViewModel : RSWindowViewModelBase
    {
        public readonly IAktSpisaniyaNomenkl_TitleRepository AktSpisaniyaNomenklRepository;

        public readonly GenericKursDBRepository<AktSpisaniyaNomenkl_Title> BaseRepository;

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

        #region Commands

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(DocumentType.AktSpisaniya, 0, CurrentDocument.Id);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel
            {
                Form = frm
            };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null)
                return;
            var ctx = new AktSpisaniyaNomenklTitleWIndowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AktSpisaniyaView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };

            ctx.Form = frm;
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            foreach (var d in AktSpisaniyaNomenklRepository.GetAllByDates(DateStart, DateEnd).ToList())
                Documents.Add(new AktSpisaniyaNomenklTitleViewModel(d));
        }

        #endregion
    }
}