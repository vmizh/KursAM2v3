using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Repositories;
using KursAM2.View.Finance.DistributeNaklad;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{

    public sealed class DistributeNakladSearchViewModel : KursWindowSearchBaseViewModel,
        ISearchWindowViewModel<DistributeNakladViewModel>, IKursLayoutManager
    {
        #region Methods

        public static DistributeNakladSearchViewModel Create()
        {
            return ViewModelSource.Create(() => new DistributeNakladSearchViewModel());
        }

        #endregion

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

        [Display(AutoGenerateField = false)]
        public new Helper.LayoutManager LayoutManager { get; set; }

        [Display(AutoGenerateField = false)]
        private new ILayoutSerializationService LayoutSerializationService
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
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow

            };
            var dtx = new DistributeNakladViewModel(Form, new DocumentOpenType
            {
                OpenType = DocumentCreateTypeEnum.New,
                })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx;
            dsForm.Show();
        }

        public override bool IsDocNewCopyAllow => CurrentDocument != null;

        public override void DocNewCopy(object form)
        {
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new DistributeNakladViewModel(Form, new DocumentOpenType
            {
                Id = CurrentDocument.Id,
                OpenType = DocumentCreateTypeEnum.Copy,
            })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx;
            dsForm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new DistributeNakladViewModel(Form, new DocumentOpenType
            {
                Id = CurrentDocument.Id,
                OpenType = DocumentCreateTypeEnum.Open,
            })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx; 
            dtx.myState = RowStatus.NotEdited;
            foreach (var t in dtx.Tovars)
            {
                t.State = RowStatus.NotEdited;
            }
            dsForm.Show();
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => false;

        public override bool CanSave()
        {
            return false;
        }

        public override void Load(object o)
        {
            Documents.Clear();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

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