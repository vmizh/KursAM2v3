using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core;
using Core.EntityViewModel;
using Core.Helper;
using Core.Menu;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Repositories;
using KursAM2.Repositories.InvoicesRepositories;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [MetadataType(typeof(DataAnnotationDistribNaklad))]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class DistributeNakladViewModel : KursBaseControlViewModel, IViewModelToEntity<Data.DistributeNaklad>,
        IKursLayoutManager
    {
        #region Constructors

        public DistributeNakladViewModel(Window parentForm, IDocumentOpenType docOpenType) : this()
        {
            Form = parentForm;
            if (docOpenType.Id == null)
            {
                Entity = DistributeNakladRepository.CreateNew();
                State = RowStatus.NewRow;
                return;
            }

            switch (docOpenType.OpenType)
            {
                case DocumentCreateTypeEnum.Open:
                    Entity = DistributeNakladRepository.GetById((Guid)docOpenType.Id);
                    // ReSharper disable once VirtualMemberCallInConstructor
                    Load(docOpenType.Id);
                    State = RowStatus.NotEdited;
                    break;
                case DocumentCreateTypeEnum.Copy:
                    Entity = DistributeNakladRepository.CreateCopy((Guid)docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
                case DocumentCreateTypeEnum.RequisiteCopy:
                    Entity = DistributeNakladRepository.CreateRequisiteCopy((Guid)docOpenType.Id);
                    State = RowStatus.NewRow;
                    break;
            }
        }

        private DistributeNakladViewModel()
        {
            BaseRepository = new GenericKursRepository<Data.DistributeNaklad>(unitOfWork);
            DistributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            InvoiceProviderRepository = new InvoiceProviderRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Распределение накладных расходов";
        }

        public DistributeNakladViewModel(Data.DistributeNaklad entity)
        {
            Entity = entity;
            State = RowStatus.NotEdited;
        }

        #endregion

        #region Fields

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork
            = new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public readonly GenericKursRepository<Data.DistributeNaklad> BaseRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly IDistributeNakladRepository DistributeNakladRepository;

        public readonly IInvoiceProviderRepository InvoiceProviderRepository;

        private readonly WindowManager winManager = new WindowManager();

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)]
        public Helper.LayoutManager LayoutManager { get; set; }

        private ILayoutSerializationService LayoutSerializationService
            => GetService<ILayoutSerializationService>();

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                SetChangeStatus();
            }
        }

        [Display(AutoGenerateField = false)] public DistributeNakladViewModel Current => this;

        [Display(AutoGenerateField = false)]
        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DistributeNakladRowViewModel> Tovars { set; get; }
            = new ObservableCollection<DistributeNakladRowViewModel>();

        [Display(AutoGenerateField = false)]
        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<DistributeNakladRowViewModel> SelectedTovars { set; get; }
            = new ObservableCollection<DistributeNakladRowViewModel>();

        [Display(AutoGenerateField = false)]
        public DistributeNakladRowViewModel CurrentTovar
        {
            get => GetValue<DistributeNakladRowViewModel>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInfoViewModel> DistributeNaklads { set; get; }
            = new ObservableCollection<DistributeNakladInfoViewModel>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<DistributeNakladInfoViewModel> SelectedDistributeNaklads { set; get; }
            = new ObservableCollection<DistributeNakladInfoViewModel>();

        [Display(AutoGenerateField = false)]
        public DistributeNakladInfoViewModel CurrentDistributeNaklad
        {
            get => GetValue<DistributeNakladInfoViewModel>();
            set => SetValue(value);
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<InvoiceProviderShort> NakladInvoices { set; get; }
            = new ObservableCollection<InvoiceProviderShort>();

        [Display(AutoGenerateField = false)]
        public ObservableCollection<InvoiceProviderShort> SelectedNakladInvoices { set; get; }
            = new ObservableCollection<InvoiceProviderShort>();

        [Display(AutoGenerateField = false)]
        public InvoiceProviderShort CurrentNakladInvoice
        {
            get => GetValue<InvoiceProviderShort>();
            set => SetValue(value);
        }

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public Data.DistributeNaklad Entity
        {
            get => GetValue<Data.DistributeNaklad>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [DisplayName("Дата")]
        [Display(AutoGenerateField = true)]
        public DateTime DocDate
        {
            get => Entity.DocDate;
            set
            {
                if (Entity.DocDate == value) return;
                Entity.DocDate = value;
                SetChangeStatus();
            }
        }

        [DisplayName("№ ")]
        [Display(AutoGenerateField = true)]
        public string DocNum
        {
            get => Entity.DocNum;
            set
            {
                if (Entity.DocNum == value) return;
                Entity.DocNum = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Создатель")]
        [Display(AutoGenerateField = true)]
        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Код валюты")]
        [Display(AutoGenerateField = false)]
        public decimal? CurrencyDC
        {
            get => Entity.CurrencyDC;
            set
            {
                if (Entity.CurrencyDC == value) return;
                Entity.CurrencyDC = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Валюта")]
        [Display(AutoGenerateField = true)]
        // ReSharper disable once PossibleInvalidOperationException
        public Currency Currency
        {
            get => MainReferences.GetCurrency(Entity.CurrencyDC);
            set => SetValue(value, () => CurrencyDC = value?.DocCode);
        }

        [DisplayName("Примечания")]
        [Display(AutoGenerateField = true)]
        public string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                SetChangeStatus();
            }
        }

        #endregion

        #region Methods

        public static DistributeNakladViewModel Create()
        {
            var factory = ViewModelSource.Factory(()
                => new DistributeNakladViewModel());
            return factory();
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

        public override void Save()
        {
            try
            {
                unitOfWork.CreateTransaction();
                unitOfWork.Save();
                unitOfWork.Commit();
                State = RowStatus.NotEdited;
                RaisePropertiesChanged(nameof(State));
                RaisePropertiesChanged(nameof(Tovars));
                RaisePropertiesChanged(nameof(SelectedTovars));
                Load(Id);
            }

            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                unitOfWork.Rollback();
            }
        }

        public override bool CanSave()
        {
            return State != RowStatus.NotEdited;
        }

        public override void Load(object o)
        {
            if (o is Guid id)
            {
                Entity = DistributeNakladRepository.GetById(id);
                Tovars.Clear();
                foreach (var r in Entity.DistributeNakladRow)
                {
                    InvoiceProvider inv = null;
                    var invrow = InvoiceProviderRepository.GetInvoiceRow(r.TovarInvoiceRowId);
                    if (invrow != null)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        inv = InvoiceProviderRepository.GetInvoiceHead((Guid)invrow.DocId);
                    }
                    Tovars.Add(new DistributeNakladRowViewModel(r)
                    {
                        InvoiceRow = invrow,
                        Invoice = inv,
                        State = RowStatus.NotEdited
                    });
                }
                State = RowStatus.NotEdited;
            }
        }

        [Command]
        public void AddNomenkl()
        {
            if (InvoiceProviderRepository.Dialogs.ShowDialog(Currency,DateTime.Today.AddDays(-30),
                DateTime.Today) == true)
            {
                foreach (var d in InvoiceProviderRepository.Dialogs.SelectedItems)
                {
                    foreach (var r in d.Rows)
                    {
                        if (r.IsUsluga) continue;
                        if (Tovars.Any(_ => _.DocId == Id && _.TovarInvoiceRowId == r.Entity.Id)) continue;
                        var newRow = DistributeNakladRepository.CreateRowNew(Entity);

                        newRow.TovarInvoiceRowId = r.Entity.Id;
                        var newTovar = new DistributeNakladRowViewModel(newRow)
                        {
                            InvoiceRow = r,
                            State = RowStatus.NewRow,
                        };
                        ((ISupportParentViewModel) newTovar).ParentViewModel = this;
                        Tovars.Add(newTovar);
                        SetChangeStatus();
                    }
                }
            }
            SelectedTovars.Clear();
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(SelectedTovars));
        }

        public bool CanAddNomenkl()
        {
            return Currency != null;
        }

        [Command]
        public void DeleteNomenkl()
        {
            var ids = SelectedTovars.Select(_ => _.Id).ToList();
            foreach (var d in SelectedTovars)
            {
                unitOfWork.Context.Entry(d.Entity).State = unitOfWork.Context.Entry(d.Entity).State == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Deleted;
            }

            foreach (var id in ids)
            {
                var item = Tovars.SingleOrDefault(_ => _.Id == id);
                if (item != null)
                    Tovars.Remove(item);
            }
            SelectedTovars.Clear();
            SetChangeStatus();
            RaisePropertiesChanged(nameof(Tovars));
            RaisePropertiesChanged(nameof(SelectedTovars));
        }

        public bool CanDeleteNomenkl()
        {
            return CurrentTovar != null;
        }

        [Command]
        public void DeleteNakladInvoice()
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public bool CanDeleteNakladInvoice()
        {
            return CurrentNakladInvoice != null;
        }

        [Command]
        public void AddNakladInvoice()
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public bool CanAddNakladInvoice()
        {
            return true;
        }

        #endregion
    }

    public class DataAnnotationDistribNaklad : DataAnnotationForFluentApiBase,
        IMetadataProvider<DistributeNakladViewModel>
    {
        void IMetadataProvider<DistributeNakladViewModel>.BuildMetadata(
            MetadataBuilder<DistributeNakladViewModel> builder)
        {
            builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус").ReadOnly();

            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("g0", Orientation.Vertical)
                    .Group("a0",Orientation.Horizontal)
                        .ContainsProperty(_ => _.DocNum)
                        .ContainsProperty(_ => _.DocDate)
                        .ContainsProperty(_ => _.Currency)
                        .ContainsProperty(_ => _.Creator)
                        .ContainsProperty(_ => _.State)
                    .EndGroup()
                    .Group("a2", Orientation.Vertical)
                        
                        .ContainsProperty(_ => _.Note)
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }
}