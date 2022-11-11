using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Managers;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.View.Dogovors;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Dogovora;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Dogovora
{
    public sealed class DogovorOfSupplierSearchViewModel : RSWindowViewModelBase
    {
        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursDBRepository<DogovorOfSupplier> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDogovorOfSupplierRepository DogovorOfSupplierRepository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private DogovorOfSupplierViewModel myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        #region Constructors

        public DogovorOfSupplierSearchViewModel()
        {
            BaseRepository = new GenericKursDBRepository<DogovorOfSupplier>(unitOfWork);
            DogovorOfSupplierRepository = new DogovorOfSupplierRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            IsCanDocNew = true;
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LayoutName = "DogovorOfSupplierSearchView";
        }

        #endregion

        #region Methods

        #endregion

        #region Properties

        public ObservableCollection<DogovorOfSupplierViewModel> Documents { set; get; } =
            new ObservableCollection<DogovorOfSupplierViewModel>();

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

        public DogovorOfSupplierViewModel CurrentDocument
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
            DocumentsOpenManager.Open(DocumentType.DogovorOfSupplier, 0, CurrentDocument.Id, this);
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new DogovorOfSupplierWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new DogovorOfSupplierWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new DogovorOfSupplierView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new DogovorOfSupplierWindowViewModel(null)
            {
                Form = frm
            };
            frm.DataContext = ctx;
            frm.Show();
        }


        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private Task Load()
        {
            var res = Refresh();
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }


        public override async void RefreshData(object data)
        {
            SplashScreenService.ShowSplashScreen();
            base.RefreshData(null);
            await Load();
        }

        public Task<List<DogovorOfSupplier>> Refresh()
        {
            var ctx = GlobalOptions.GetEntities();
            var res = Task.Factory.StartNew(() =>
                new List<DogovorOfSupplier>(ctx.DogovorOfSupplier.Where(_ =>
                    _.DocDate >= DateStart && _.DocDate <= DateEnd)));
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(new DogovorOfSupplierViewModel(d));
            RaisePropertyChanged(nameof(Documents));
            return res;
        }

        #endregion
    }
}
