using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Dogovora;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Dogovors;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientSearchViewModel : RSWindowSearchViewModelBase
    {
        private DogovorClientViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;

        #region Constructors

        public DogovorClientSearchViewModel()
        {
            BaseRepository = new GenericKursDBRepository<DogovorClient>(unitOfWork);
            DogovorClientRepository = new DogovorClientRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            IsCanDocNew = true;
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LayoutName = "DogovorClientSearchView";
            RefreshData(null);
        }

        #endregion

        #region Methods

        #endregion

        #region Properties

        public ObservableCollection<DogovorClientViewModel> Documents { set; get; } =
            new ObservableCollection<DogovorClientViewModel>();

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

        public DogovorClientViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.DogovorClient,0,CurrentDocument.Id);
        }

        public override void DocNewCopy(object form)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public override void DocNewCopyRequisite(object form)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new DogovorClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorClientWindowViewModel()
            {
                Form = frm
            };
            frm.DataContext = ctx;
            frm.Show();
        }

        
        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void RefreshData(object obj)
        {
            Documents.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.DogovorClient.Where(_ => _.DogDate >= DateStart && _.DogDate <= DateEnd).ToList();
                foreach (var d in data) Documents.Add(new DogovorClientViewModel(d));
            }
        }

        #endregion
    }
}