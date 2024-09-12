using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.DistributeNaklad;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;

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
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            baseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
        }

        public DistributeNakladSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            baseRepository = new GenericKursDBRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
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

        public  void AddSearchList(object obj)
        {
            var dnakForm = new KursBaseSearchWindow
            {
                Owner = Application.Current.MainWindow
            };
            var v = new DistributeNakladSearchView(dnakForm);
            dnakForm.modelViewControl.Content = v;
            ((KursBaseControlViewModel)v.DataContext).Form = dnakForm;
            dnakForm.DataContext = v.DataContext;
            dnakForm.Show();

        }

        public override string WindowName => "Поиск распределений накладных расходов";
        public override string LayoutName => "DistributeNakladSearchViewModel";

        [Display(AutoGenerateField = false)] public override bool IsCanDocNew => true;

        [Display(AutoGenerateField = false)] public override bool IsDocNewCopyAllow => false;

        [Display(AutoGenerateField = false)] public override bool IsDocNewCopyRequisiteAllow => false;

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        [Display(AutoGenerateField = false)] public new Helper.LayoutManager LayoutManager { get; set; }

        [Display(AutoGenerateField = false)]
        private new ILayoutSerializationService LayoutSerializationService
            => GetService<ILayoutSerializationService>();

        /*
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
                RaisePropertyChanged(nameof(StartDate));
            }
        }
        */

        public ObservableCollection<DistributeNakladViewModel> Documents { get; set; } =
            new ObservableCollection<DistributeNakladViewModel>();

        public ObservableCollection<DistributeNakladViewModel> SelectDocuments { get; set; } =
            new ObservableCollection<DistributeNakladViewModel>();

        public DistributeNakladViewModel CurrentDocument
        {
            get => GetValue<DistributeNakladViewModel>();
            set => SetValue(value);
        }

        /*
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
        */

        #endregion

        #region Commands

        [Display(AutoGenerateField = false)]
        public ICommand OnWindowClosingCommand
        {
            get { return new Command(OnWindowClosing, _ => true); }
        }

        public override void OnWindowClosing(object obj)
        {
            LayoutManager.Save();
        }

        [Display(AutoGenerateField = false)]
        public ICommand OnWindowLoadedCommand
        {
            get { return new Command(OnWindowLoaded, _ => true); }
        }

        public override void OnWindowLoaded(object obj)
        {
            LayoutManager = new Helper.LayoutManager(Form, LayoutSerializationService,
                GetType().Name, null, GlobalOptions.KursSystemDBContext);
            LayoutManager.Load();
            if (Form is KursBaseSearchWindow ctrl)
                if (ctrl.modelViewControl.Content is DistributeNakladSearchView frm)
                    foreach (var col in frm.resultGridControl.Columns)
                        switch (col.FieldName)
                        {
                            case "State":
                                col.Visible = false;
                                break;
                        }
        }

        public override void ResetLayout(object form)
        {
            LayoutManager.ResetLayout();
        }

        public override void RefreshData(object obj)
        {
        }

        public override bool IsCanRefresh { set; get; } = true;

        public override void DocNewEmpty(object form)
        {
            var dsForm = new DistributedNakladView
            {
                Owner = Application.Current.MainWindow
            };
            var dtx = new DistributeNakladViewModel(null, new DocumentOpenType
            {
                OpenType = DocumentCreateTypeEnum.New
            })
            {
                Form = dsForm
            };
            dsForm.DataContext = dtx;
            dtx.Form = dsForm;
            dsForm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.Naklad, 0, CurrentDocument.Id);
        }

        public override bool CanSave()
        {
            return false;
        }

        public override void Load(object o)
        {
            Documents.Clear();
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
