using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Managers;
using KursAM2.Repositories.AccruedAmount;
using KursAM2.View.Finance.AccruedAmount;
using KursDomain.Documents.AccruedAmount;

namespace KursAM2.ViewModel.Finance.AccruedAmount
{
    public class AccuredAmountForClientSearchViewModel : RSWindowSearchViewModelBase
    {
        #region Constructors

        public AccuredAmountForClientSearchViewModel()
        {
            GenericRepository = new GenericKursDBRepository<AccruedAmountForClient>(UnitOfWork);
            AccruedAmountForClientRepository = new AccruedAmountForClientRepository(UnitOfWork);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
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
                DocumentType.AccruedAmountForClient, 0, CurrentDocument.Id, this);
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new AccruedAmountForClientWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var view = new AccruedAmountForClientView
            {
                Owner = Application.Current.MainWindow
            };
            var ctx = new AccruedAmountForClientWindowViewModel(null)
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
            var ctx = new AccruedAmountForClientWindowViewModel(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new AccruedAmountForClientView
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
            foreach (var d in AccruedAmountForClientRepository.GetByDate(StartDate, EndDate))
                Documents.Add(new AccruedAmountForClientViewModel(d));
            foreach (var item in Documents) item.RaisePropertyAllChanged();
        }

        #endregion

        #region Fields

        private DateTime myDateEnd;
        private DateTime myDateStart;
        private AccruedAmountForClientViewModel myCurrentDocument;

        public readonly GenericKursDBRepository<AccruedAmountForClient> GenericRepository;
        public readonly IAccruedAmountForClientRepository AccruedAmountForClientRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        #endregion

        #region Properties

        public override string WindowName => "Реестр прямых расходов для клиентов";

        public override string LayoutName => "AccuredAmountForClientSearchViewModel";

        public ObservableCollection<AccruedAmountForClientViewModel> Documents { set; get; } =
            new ObservableCollection<AccruedAmountForClientViewModel>();

        public AccruedAmountForClientViewModel CurrentDocument
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
