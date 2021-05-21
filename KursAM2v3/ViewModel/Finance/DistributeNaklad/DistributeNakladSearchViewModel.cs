using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.Menu;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.DistributeNaklad;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [POCOViewModel]
    public sealed class DistributeNakladSearchViewModel : KursWindowSearchBaseViewModel,
        ISearchWindowViewModel<DistributeNakladViewModel>, IKursLayoutManager
    {
        #region Constructors

        public DistributeNakladSearchViewModel(Window form) : base(form)
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            baseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            //ModelView = new DistributeNakladSearchView();
            WindowName = "Поиск распределений накладных расходов";
        }

        public DistributeNakladSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddDays(-30);
            baseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            WindowName = "Поиск распределений накладных расходов";
        }

        #endregion

        #region Fields

        private DateTime myDateEnd;
        private DateTime myDateStart;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        private readonly GenericKursDBRepository<Data.DistributeNaklad> baseRepository;

        // ReSharper disable once NotAccessedField.Local
        private readonly IDistributeNakladRepository distributeNakladRepository;

        #endregion

        #region Properties

        public Helper.LayoutManager LayoutManager { get; set; }

        private ILayoutSerializationService LayoutSerializationService
            => GetService<ILayoutSerializationService>();

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

        public static DistributeNakladSearchViewModel Create()
        {
            return ViewModelSource.Create(() => new DistributeNakladSearchViewModel());
        }

        #endregion

        #region Commands

        [Command]
        public void OnWindowClosing()
        {
            LayoutManager.Save();
        }

        [Command]
        public void OnWindowLoaded()
        {
            LayoutManager = new Helper.LayoutManager(Form, LayoutSerializationService,
                GetType().Name, null);
            LayoutManager.Load();
        }

        public override void ResetLayout(object form)
        {
            LayoutManager.ResetLayout();
        }

        public override bool IsCanRefresh { set; get; } = true;

        public override void DocNewEmpty(object form)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var v = new DistributedNakladView(dsForm, new DocumentOpenType
            {
                OpenType = DocumentCreateTypeEnum.New
            });
            dsForm.modelViewControl.Content = v;
            dsForm.DataContext = v.DataContext;
            dsForm.Show();
        }

        public override void DocNewCopy(object form)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var v = new DistributedNakladView(dsForm, new DocumentOpenType
            {
                Id = CurrentDocument.Id,
                OpenType = DocumentCreateTypeEnum.Copy
            });
            dsForm.modelViewControl.Content = v;
            dsForm.DataContext = v.DataContext;
            dsForm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            var dsForm = new KursBaseWindow
            {
                Owner = Application.Current.MainWindow
            };
            var v = new DistributedNakladView(dsForm, new DocumentOpenType
            {
                Id = CurrentDocument.Id,
                OpenType = DocumentCreateTypeEnum.Open
            });
            dsForm.modelViewControl.Content = v;
            dsForm.DataContext = v.DataContext;
            dsForm.Show();
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override bool CanSave()
        {
            return false;
        }

        public override void Load(object o)
        {
            Documents.Clear();
            while (!MainReferences.IsReferenceLoadComplete) {}
            foreach (var d in distributeNakladRepository.GetAllByDates(StartDate, EndDate))
                Documents.Add(new DistributeNakladViewModel(d));
            RaisePropertiesChanged(nameof(Documents));
        }

        public override bool CanLoad(object o)
        {
            return State != RowStatus.NewRow;
        }

        #endregion
    }
}