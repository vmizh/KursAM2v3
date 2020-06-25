using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.Repositories;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Finance.DistributeNaklad;

namespace KursAM2.ViewModel.Finance.DistributeNaklad
{
    [MetadataType(typeof(DataAnnotationDistribNaklad))]
    public class DistributeNakladViewModel : KursBaseControlViewModel, IViewModelToEntity<Data.DistributeNaklad>
    {
        #region Constructors

        public DistributeNakladViewModel(Guid? id, DocumentCreateTypeEnum createType) : this()
        {
            if (id == null)
            {
                Entity = DistributeNakladRepository.CreateNew();
                State = RowStatus.NewRow;
                return;
            }

            switch (createType)
            {
                case DocumentCreateTypeEnum.Open:
                    Entity = DistributeNakladRepository.GetById((Guid) id);
                    State = RowStatus.NotEdited;
                    break;
                case DocumentCreateTypeEnum.Copy:
                    Entity = DistributeNakladRepository.CreateCopy((Guid) id);
                    State = RowStatus.NewRow;
                    break;
                case DocumentCreateTypeEnum.RequisiteCopy:
                    Entity = DistributeNakladRepository.CreateRequisiteCopy((Guid) id);
                    State = RowStatus.NewRow;
                    break;
            }
        }

        private DistributeNakladViewModel()
        {
            ModelView = new DistributedNakladView();
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

        #endregion

        #region Commands

        public override void Save()
        {
            try
            {
                unitOfWork.CreateTransaction();
                unitOfWork.Save();
                unitOfWork.Commit();
                State = RowStatus.NotEdited;
                RaisePropertiesChanged(nameof(State));
                Load();
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

        public override void Load()
        {
            if (CanSave())
            {
                if (winManager.ShowWinUIMessageBox("В документ внесены изменения. Сохранить?",
                    "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Save();
                    return;
                }
            }
            Entity = DistributeNakladRepository.GetById(Id);
            State = RowStatus.NotEdited;
        }

        [Command]
        public void AddNomenkl()
        {
            if (InvoiceProviderRepository.Dialogs.ShowDialog(Currency) == true)
            {
                foreach (var d in InvoiceProviderRepository.Dialogs.SelectedItems)
                {
                    foreach (var r in d.Rows)
                    { 
                        var newRow = DistributeNakladRepository.CreateRowNew(Entity);
                        newRow.TovarInvoiceRowId = d.Entity.Id;
                        Tovars.Add(new DistributeNakladRowViewModel(newRow)
                        {
                            InvoiceRow = r,
                            State = RowStatus.NewRow,
                        });
                    }
                   
                }

            }
        }

        public bool CanAddNomenkl()
        {
            return Currency != null;
        }

        [Command]
        public void DeleteNomenkl()
        {
            WindowManager.ShowFunctionNotReleased();
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