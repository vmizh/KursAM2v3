using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Dogovora;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Dogovors;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorClientSearchViewModel : RSWindowViewModelBase
    {
        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorClient> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorClientRepository DogovorClientRepository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private DogovorClientViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

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
            DocumentsOpenManager.Open(DocumentType.DogovorClient, 0, CurrentDocument.Id);
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
            var ctx = new DogovorClientWindowViewModel
            {
                Form = frm
            };
            frm.DataContext = ctx;
            frm.Show();
        }


        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private Task Load()
        {
            var ctx = GlobalOptions.GetEntities();
            var res = Task.Factory.StartNew(() =>
                new List<DogovorClient>(ctx.DogovorClient.Where(_ => _.DogDate >= DateStart && _.DogDate <= DateEnd)));
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(new DogovorClientViewModel(d));
            RaisePropertyChanged(nameof(Documents));
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }

        public override async void RefreshData(object data)
        {
            SplashScreenService.ShowSplashScreen();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
            base.RefreshData(null);
            await Load();
        }

        #endregion
    }
}