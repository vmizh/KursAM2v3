using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.Menu;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Data;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.DistributeNaklad;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    public class DistributeNakladSearchViewModel : KursWindowSearchBaseViewModel,
        ISearchWindowViewModel<DistributeNakladViewModel>
    {
        #region Constructors

        public DistributeNakladSearchViewModel(Window form) : base(form)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            baseRepository = new GenericKursRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            ModelView = new DistributedNakladView();
            WindowName = "Поиск распределений накладных расходов";
        }

        #endregion

        #region Fields

        private DateTime myDateEnd;
        private DateTime myDateStart;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
        // ReSharper disable once NotAccessedField.Local
        private readonly GenericKursRepository<Data.DistributeNaklad> baseRepository;

        // ReSharper disable once NotAccessedField.Local
        private IDistributeNakladRepository distributeNakladRepository;

        #endregion

        #region Properties

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

        public ObservableCollection<DistributeNakladViewModel> Documents { get; set; } = 
            new ObservableCollection<DistributeNakladViewModel>();
        public ObservableCollection<DistributeNakladViewModel> SelectDocuments { get; set; } =
            new ObservableCollection<DistributeNakladViewModel>();

        public DistributeNakladViewModel CurrentDocument
        {
            get => GetValue<DistributeNakladViewModel>();
            set => SetValue(value);
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

        #region Commands

        public override void DocNewEmpty(object form)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var dsDataContext = new DistributeNakladViewModel(null,DocumentCreateTypeEnum.New);
            dsForm.DataContext = dsDataContext;
            dsDataContext.Form = dsForm;
            dsForm.Show();
        }

        public override void DocNewCopy(object form)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var dsDataContext = new DistributeNakladViewModel(CurrentDocument.Id,DocumentCreateTypeEnum.Copy);
            dsForm.DataContext = dsDataContext;
            dsDataContext.Form = dsForm;
            dsForm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var dsDataContext = new DistributeNakladViewModel(CurrentDocument.Id,DocumentCreateTypeEnum.Open);
            dsForm.DataContext = dsDataContext;
            dsDataContext.Form = dsForm;
            dsForm.Show();
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override bool CanSave()
        {
            return false;
        }

        public override void Load()
        {
            Documents.Clear();
            foreach (var d in distributeNakladRepository.GetAllByDates(StartDate, EndDate))
            {
                Documents.Add(new DistributeNakladViewModel(d));
            }
            RaisePropertiesChanged(nameof(Documents));
        }

        public override bool CanLoad()
        {
            return State != RowStatus.NewRow;
        }

        #endregion
    }
}