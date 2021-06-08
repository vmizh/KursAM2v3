using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.Invoices;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel.Dogovora
{
    [MetadataType(typeof(DogovorClientViewModel_FluentAPI))]
    public sealed class DogovorClientViewModel : RSWindowViewModelBase, IDataErrorInfo, IEntity<DogovorClient>
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

        public override bool IsCorrect()
        {
            if (Client != null && DogType != null && FormOfPayment != null
                && PayCondition != null && Rows.All(_ => _.IsCorrect())) 
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"Договор клиенту №{DogNum} от {DogDate.ToShortDateString()} для {Client} на {Summa} {Currency}";
        }

        #endregion

        #region Constructors

        public DogovorClientViewModel()
        {
            Entity = DefaultValue();
        }

        public DogovorClientViewModel(DogovorClient entity, RowStatus state = RowStatus.NotEdited)
        {
            Entity = entity ?? DefaultValue();

            foreach (var r in Entity.DogovorClientRow)
            {
                Rows.Add(new DogovorClientRowViewModel(r)
                {
                    Parent = this,
                    myState = state
                });
            }
            myState = state;
        }

        #endregion

        #region Properties

        public ObservableCollection<DogovorClientRowViewModel> Rows { set; get; } = new();
        public ObservableCollection<DogovorClientFactViewModel> FactsAll { set; get; } =
            new ObservableCollection<DogovorClientFactViewModel>();
        public ObservableCollection<InvoicePaymentDocument> PaymentList { set; get; } =
            new ObservableCollection<InvoicePaymentDocument>();
        
        public bool IsAccessRight { get; set; }

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

        public List<DogovorClient> LoadList()
        {
            throw new NotImplementedException();
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
                RaisePropertyChanged(nameof(Currency));
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

        public FormPay FormOfPayment
        {
            get => MainReferences.GetFormPay(Entity.FormOfPayment);
            set
            {
                if (MainReferences.GetFormPay(Entity.FormOfPayment) == value) return;
                Entity.FormOfPayment = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get => MainReferences.GetPayCondition(Entity.PayCondition);
            set
            {
                if (MainReferences.GetPayCondition(Entity.PayCondition) == value) return;
                Entity.PayCondition = value?.DocCode ?? 0;
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

        
        public bool IsCalcBack
        {
            get => Entity.IsCalckBack;
            set
            {
                if (Entity.IsCalckBack == value) return;
                Entity.IsCalckBack = value;
                foreach (var r in Rows)
                {
                    r.IsCalcBack = Entity.IsCalckBack;
                }
                RaisePropertyChanged();
            }
        }

        public override string Description => $"Договор клиенту №{DogNum} от {DogDate.ToShortDateString()} {Client} " +
                                              $"на {Summa} {Currency} {Note}";

        #endregion

        #region Commands

        [Display(AutoGenerateField = false)]
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

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Client":
                        return Client == null ? "Клиент должен быть обязательно выбран" : null;
                    case "DogType":
                        return DogType == null ? "Тип договора должен быть обязательно выбран" : null;
                    case "PayCondition":
                        return PayCondition == null ? "Условия оплаты должны быть указаны" : null;
                    case "FormOfPayment":
                        return DogType == null ? "Форма оплаты - обязательное поле" : null;
                    default:
                        return null;
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public string Error => "";

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
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.Creator).AutoGenerated().DisplayName("Создатель").ReadOnly();
            builder.Property(_ => _.FormOfPayment).DisplayName("Форма оплаты");
            builder.Property(_ => _.DogDate).AutoGenerated().DisplayName("Дата").Required(()=>notNullMessage);
            builder.Property(_ => _.DogNum).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.DogType).AutoGenerated().DisplayName("Тип договора").Required(()=>notNullMessage);;
            builder.Property(_ => _.EndDate).AutoGenerated().DisplayName("Дата завершения");
            builder.Property(_ => _.IsCalcBack).AutoGenerated().DisplayName("Обратный расчет");
            builder.Property(_ => _.IsExecuted).AutoGenerated().DisplayName("Закрыт");
            builder.Property(_ => _.Client).AutoGenerated().DisplayName("Клиент").Required(()=>notNullMessage);
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания");
            builder.Property(_ => _.PayCondition).AutoGenerated().DisplayName("Условия оплаты").Required(()=>notNullMessage);;
            builder.Property(_ => _.StartDate).AutoGenerated().DisplayName("Дата начала");
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2").ReadOnly();
        }
    }
}