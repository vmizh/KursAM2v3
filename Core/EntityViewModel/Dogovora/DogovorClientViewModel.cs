using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel.Dogovora
{
    [MetadataType(typeof(DogovorClientViewModel_FluentAPI))]
    public class DogovorClientViewModel : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Fields

        private DogovorClient myEntity;

        #endregion

        #region Methods

        private DogovorClient DefaultValue()
        {
            return new DogovorClient
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion

        #region Constructors

        public DogovorClientViewModel()
        {
            Entity = DefaultValue();
        }

        public DogovorClientViewModel(DogovorClient entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Properties

        public ObservableCollection<DogovorClientRowViewModel> Rows { set; get; } = new();

        public DogovorClient Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        public string DogNum
        {
            get => Entity.DogNum;
            set
            {
                if (Entity.DogNum == value) return;
                Entity.DogNum = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DogDate
        {
            get => Entity.DogDate;
            set
            {
                if (Entity.DogDate == value) return;
                Entity.DogDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get => Entity.StartDate;
            set
            {
                if (Entity.StartDate == value) return;
                Entity.StartDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => Entity.EndDate;
            set
            {
                if (Entity.EndDate == value) return;
                Entity.EndDate = value;
                RaisePropertyChanged();
            }
        }

        public Kontragent Client
        {
            get => MainReferences.GetKontragent(Entity.KontrDC);
            set
            {
                if (MainReferences.GetKontragent(Entity.KontrDC) == value) return;
                Entity.KontrDC = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        public Currency Currency => Client?.BalansCurrency;

        public decimal Summa => Rows.Sum(_ => _.Summa);

        public bool IsExecuted
        {
            get => Entity.IsExecuted;
            set
            {
                if (Entity.IsExecuted == value) return;
                Entity.IsExecuted = value;
                RaisePropertyChanged();
            }
        }

        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        public FormPay DeliveryCondition
        {
            get => MainReferences.GetFormPay(Entity.DeliveryConditions);
            set
            {
                if (MainReferences.GetFormPay(Entity.DeliveryConditions) == value) return;
                Entity.DeliveryConditions = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get => MainReferences.GetPayCondition(Entity.FormOfPayment);
            set
            {
                if (MainReferences.GetPayCondition(Entity.FormOfPayment) == value) return;
                Entity.FormOfPayment = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        public ContractType DogType
        {
            get => MainReferences.GetContractType(Entity.DogType);
            set
            {
                if (MainReferences.GetContractType(Entity.DogType) == value) return;
                Entity.DogType = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        public bool IsNDSInPrice
        {
            get => Entity.NDSInPrice;
            set
            {
                if (Entity.NDSInPrice == value) return;
                Entity.NDSInPrice = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCalcBack
        {
            get => Entity.CalckBack;
            set
            {
                if (Entity.CalckBack == value) return;
                Entity.CalckBack = value;
                RaisePropertyChanged();
            }
        }

        public override string Description => $"Договор клиенту №{DogNum} от {DogDate.ToShortDateString()} {Client} " +
                                              $"на {Summa} {Currency} {Note}";

        #endregion

        #region Commands

        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => true); }
        }

        private void KontragentSelect(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName] => "Не обработано";

        [Display(AutoGenerateField = false)]
        public string Error { get; } = "";

        #endregion
    }

    public class DogovorClientViewModel_FluentAPI : DataAnnotationForFluentApiBase,
        IMetadataProvider<DogovorClientViewModel>
    {
        void IMetadataProvider<DogovorClientViewModel>.BuildMetadata(
            MetadataBuilder<DogovorClientViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.Creator).AutoGenerated().DisplayName("Создатель").ReadOnly();
            builder.Property(_ => _.DeliveryCondition).AutoGenerated().DisplayName("Форма оплаты");
            builder.Property(_ => _.DogDate).AutoGenerated().DisplayName("Дата").Required(()=>notNullMessage);
            builder.Property(_ => _.DogNum).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.DogType).AutoGenerated().DisplayName("Тип договора").Required(()=>notNullMessage);;
            builder.Property(_ => _.EndDate).AutoGenerated().DisplayName("Дата завершения");
            builder.Property(_ => _.IsCalcBack).AutoGenerated().DisplayName("Обратный расчет");
            builder.Property(_ => _.IsExecuted).AutoGenerated().DisplayName("Закрыт");
            builder.Property(_ => _.IsNDSInPrice).AutoGenerated().DisplayName("НДС в цене");
            builder.Property(_ => _.Client).AutoGenerated().DisplayName("Клиент").Required(()=>notNullMessage);;
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания");
            builder.Property(_ => _.PayCondition).AutoGenerated().DisplayName("Условия оплаты").Required(()=>notNullMessage);;
            builder.Property(_ => _.StartDate).AutoGenerated().DisplayName("Дата начала");
        }
    }
}