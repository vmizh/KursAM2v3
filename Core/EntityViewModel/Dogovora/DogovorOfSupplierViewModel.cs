﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using Newtonsoft.Json;

namespace Core.EntityViewModel.Dogovora
{
    public class DogovorOfSupplierViewModel_FluentAPI : DataAnnotationForFluentApiBase,
        IMetadataProvider<DogovorOfSupplierViewModel>
    {
        void IMetadataProvider<DogovorOfSupplierViewModel>.BuildMetadata(
            MetadataBuilder<DogovorOfSupplierViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.Creator).AutoGenerated().DisplayName("Создатель").ReadOnly();
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата").Required(() => notNullMessage);
            builder.Property(_ => _.InNum).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.OutNum).AutoGenerated().DisplayName("Внешн.№");
            builder.Property(_ => _.DogType).AutoGenerated().DisplayName("Тип договора").Required(() => notNullMessage);
            builder.Property(_ => _.IsClosed).AutoGenerated().DisplayName("Закрыт");
            builder.Property(_ => _.Kontragent).AutoGenerated().DisplayName("Поставщик").Required(() => notNullMessage);
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания");
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.OtvetstvenLico).AutoGenerated().DisplayName("Ответственный");
        }
    }

    [MetadataType(typeof(DogovorOfSupplierViewModel_FluentAPI))]
    public class DogovorOfSupplierViewModel : RSWindowViewModelBase, IDataErrorInfo, IEntity<DogovorOfSupplier>
    {
        #region Fields

        private DogovorOfSupplier myEntity;

        #endregion

        #region Properties

        public ObservableCollection<DogovorOfSupplierRowViewModel> Rows { set; get; } =
            new();

        public DogovorOfSupplier Entity
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

        public DateTime DocDate
        {
            get => Entity.DocDate;
            set
            {
                if (Entity.DocDate == value) return;
                Entity.DocDate = value;
                RaisePropertyChanged();
            }
        }

        public int InNum
        {
            get => Entity.InNum;
            set
            {
                if (Entity.InNum == value) return;
                Entity.InNum = value;
                RaisePropertyChanged();
            }
        }

        public string OutNum
        {
            get => Entity.OutNum;
            set
            {
                if (Entity.OutNum == value) return;
                Entity.OutNum = value;
                RaisePropertyChanged();
            }
        }

        public Kontragent Kontragent
        {
            get => Entity.KontrDC != 0 ? MainReferences.GetKontragent(Entity.KontrDC) : null;
            set
            {
                if (MainReferences.GetKontragent(Entity.KontrDC) == value) return;
                Entity.KontrDC = value?.DocCode ?? 0;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Currency));
            }
        }

        public decimal Summa => Rows?.Sum(_ => _.Summa) ?? 0;

        public Currency Currency => Kontragent?.BalansCurrency;

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

        public bool IsClosed
        {
            get => Entity.IsClosed;
            set
            {
                if (Entity.IsClosed == value) return;
                Entity.IsClosed = value;
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

        public Employee.Employee OtvetstvenLico
        {
            get => Entity.OtvetstvenTN == 0 ? null : MainReferences.GetEmployee(Entity.OtvetstvenTN);
            set
            {
                if (MainReferences.GetEmployee(Entity.OtvetstvenTN) == value) return;
                Entity.OtvetstvenTN = value?.TabelNumber ?? 0;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(OtvetstvenLicoTN));
            }
        }

        public int? OtvetstvenLicoTN => Entity.OtvetstvenTN == 0 ? null : Entity.OtvetstvenTN;

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

        #endregion

        #region Constructors

        public DogovorOfSupplierViewModel()
        {
            Entity = DefaultValue();
        }

        public DogovorOfSupplierViewModel(DogovorOfSupplier entity)
        {
            Entity = entity ?? DefaultValue();

            foreach (var r in Entity.DogovorOfSupplierRow)
                Rows.Add(new DogovorOfSupplierRowViewModel(r)
                {
                    Parent = this
                });
        }

        #endregion

        #region Methods

        public override object ToJson()
        {
            var result = new
            {
                Статус = CustomFormat.GetEnumName(State),
                Id = Id.ToString(),
                Дата = DocDate.ToShortDateString(),
                ТипДоговора = DogType.ToString(),
                Номер = InNum.ToString(),
                Создатель = Creator,
                Закрыт = IsClosed ? "Да" : "Нет",
                Поставщик = Kontragent.ToString(),
                Сумма = Summa.ToString("n2"),
                Примечание = Note,
                Ответственный = OtvetstvenLico.ToString(),
                Позиции = Rows.Select(_ => _.ToJson())
            };
            return JsonConvert.SerializeObject(result);                                                                                          
        }

        public override string ToString()
        {
            return
                $"Договор от поставщика №{InNum}/{OutNum} " +
                $"от {DocDate.ToShortDateString()} Контрагент: {Kontragent} на сумму {Summa} " +
                $"{Currency}";
        }

        private DogovorOfSupplier DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                DocDate = DateTime.Today,
                InNum = -1,
                Creator = GlobalOptions.UserInfo.NickName
            };
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Kontragent):
                        return Kontragent == null ? "Поставщик должен быть обязательно выбран" : null;
                    case nameof(DogType):
                        return DogType == null ? "Тип договора должен быть обязательно выбран" : null;
                    case nameof(OtvetstvenLico):
                        return OtvetstvenLico == null ? "Ответственный должен быть выбран" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error => Kontragent == null || DogType == null || OtvetstvenLico == null
            ? "Не все поля заполнены"
            : null;

        #endregion
    }
}