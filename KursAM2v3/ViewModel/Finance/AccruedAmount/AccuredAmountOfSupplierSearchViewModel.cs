using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using KursDomain.Repository;
using KursAM2.Managers;
using KursAM2.Repositories.AccruedAmount;
using KursAM2.View.Finance.AccruedAmount;
using KursDomain;
using KursDomain.Documents.AccruedAmount;
using KursDomain.Documents.CommonReferences;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    public class AccuredAmountOfSupplierSearchViewModel : RSWindowSearchViewModelBase
    {
        #region Constructors

        public AccuredAmountOfSupplierSearchViewModel()
        {
            GenericRepository = new GenericKursDBRepository<AccruedAmountOfSupplier>(UnitOfWork);
            AccruedAmountOfSupplierRepository = new AccruedAmountOfSupplierRepository(UnitOfWork);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today; 
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month - 1, 1);
        }

        #endregion

        #region Commands

        public override bool IsDocDeleteAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.AccruedAmountOfSupplier, 0, CurrentDocument.Id, this);
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new AccruedAmountOfSupplierWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AccruedAmountOfSupplierWindowViewModel(null)
            {
                Form = view,
                ParentFormViewModel = this
            };
            view.DataContext = ctx;
            view.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new AccruedAmountOfSupplierWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AccruedAmountOfSupplierView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.State = EntityState.Detached;
            Documents.Clear();
            foreach (var d in AccruedAmountOfSupplierRepository.GetByDate(StartDate, EndDate))
                Documents.Add(new AccruedAmountOfSupplierViewModel(d,GenericRepository.Context));
            foreach (var item in Documents) item.RaisePropertyAllChanged();
        }

        #endregion

        #region Fields

        private DateTime myDateEnd;
        private DateTime myDateStart;
        private AccruedAmountOfSupplierViewModel myCurrentDocument;

        public readonly GenericKursDBRepository<AccruedAmountOfSupplier> GenericRepository;
        public readonly IAccruedAmountOfSupplierRepository AccruedAmountOfSupplierRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        #endregion

        #region Properties

        public override string LayoutName => "AccuredAmountOfSupplierSearchViewModel";
        public override string WindowName => "Реестр прямых затрат";

        public ObservableCollection<AccruedAmountOfSupplierViewModel> Documents { set; get; } =
            new ObservableCollection<AccruedAmountOfSupplierViewModel>();

        public AccruedAmountOfSupplierViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

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

        #endregion

        #region Methods

        #endregion
    }
}
