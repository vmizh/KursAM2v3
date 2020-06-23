using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        public DistributeNakladViewModel(Guid id) : this()
        {
            Entity = distributeNakladRepository.GetById(id);
            State = RowStatus.NotEdited;
        }

        public DistributeNakladViewModel()
        {
            Entity = new Data.DistributeNaklad
            {
                Id = Guid.Empty,
                CurrencyDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode
            };
            ModelView = new DistributedNakladView();
            baseRepository = new GenericKursRepository<Data.DistributeNaklad>(unitOfWork);
            distributeNakladRepository = new DistributeNakladRepository(unitOfWork);
            invoiceProviderRepository = new InvoiceProviderRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            WindowName = "Распределение накладных расходов";
        }

        public DistributeNakladViewModel(Data.DistributeNaklad entity,
            RowStatus state = RowStatus.NotEdited):this()
        {
            if (entity == null)
                Entity = new Data.DistributeNaklad
                {
                    Id = Guid.NewGuid(),
                    CurrencyDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode
                };
            else
                // ReSharper disable once VirtualMemberCallInConstructor
                Id = state == RowStatus.NewRow ? Guid.NewGuid() : Entity.Id;
            State = state;
        }

        #endregion

        #region Fields

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork = new UnitOfWork<ALFAMEDIAEntities>();
        // ReSharper disable once NotAccessedField.Local
        private readonly GenericKursRepository<Data.DistributeNaklad> baseRepository;

        // ReSharper disable once NotAccessedField.Local
        private readonly IDistributeNakladRepository distributeNakladRepository;

        private readonly IInvoiceProviderRepository invoiceProviderRepository;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)]
        public DistributeNakladViewModel Current => this;

        [Display(AutoGenerateField = false)]
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

        [DisplayName("№")]
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
        public Currency Currency => MainReferences.GetCurrency(Entity.CurrencyDC);

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

        public override void Load()
        {
            WindowManager.ShowFunctionNotReleased();
        }
        [Command]
        public void AddNomenkl()
        {
            if (invoiceProviderRepository.Dialogs.ShowDialog(Currency) == true)
            {

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
            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("g0", Orientation.Vertical)
                    .Group("a0",Orientation.Horizontal)
                        .ContainsProperty(_ => _.DocNum)
                        .ContainsProperty(_ => _.DocDate)
                        .ContainsProperty(_ => _.Currency)
                    .EndGroup()
                    .Group("a2", Orientation.Vertical)
                        .ContainsProperty(_ => _.Creator)
                        .ContainsProperty(_ => _.Note)
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }
}