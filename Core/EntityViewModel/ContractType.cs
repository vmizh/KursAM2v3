using System;
using System.Collections.Generic;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Тип договора
    /// </summary>
    public class ContractType : RSViewModelBase, IEntity<SD_102>
    {
        private SD_102 myEntity;

        public ContractType()
        {
            Entity = new SD_102 {DOC_CODE = -1};
        }

        public ContractType(SD_102 entity)
        {
            Entity = entity ?? new SD_102 {DOC_CODE = -1};
        }

        public SD_102 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public decimal DOC_CODE
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string TD_NAME
        {
            get => Entity.TD_NAME;
            set
            {
                if (Entity.TD_NAME == value) return;
                Entity.TD_NAME = value;
                RaisePropertyChanged();
            }
        }

        public short TD_0BUY_1SALE
        {
            get => Entity.TD_0BUY_1SALE;
            set
            {
                if (Entity.TD_0BUY_1SALE == value) return;
                Entity.TD_0BUY_1SALE = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_DOP_SOGL
        {
            get => Entity.TD_DOP_SOGL;
            set
            {
                if (Entity.TD_DOP_SOGL == value) return;
                Entity.TD_DOP_SOGL = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_DILER
        {
            get => Entity.TD_DILER;
            set
            {
                if (Entity.TD_DILER == value) return;
                Entity.TD_DILER = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_SF_AUTOMAT
        {
            get => Entity.TD_SF_AUTOMAT;
            set
            {
                if (Entity.TD_SF_AUTOMAT == value) return;
                Entity.TD_SF_AUTOMAT = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_IN_SF_WITH_TEK_PRICES
        {
            get => Entity.TD_IN_SF_WITH_TEK_PRICES;
            set
            {
                if (Entity.TD_IN_SF_WITH_TEK_PRICES == value) return;
                Entity.TD_IN_SF_WITH_TEK_PRICES = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_USE_PRICES_FOR_ZAKUPOK
        {
            get => Entity.TD_USE_PRICES_FOR_ZAKUPOK;
            set
            {
                if (Entity.TD_USE_PRICES_FOR_ZAKUPOK == value) return;
                Entity.TD_USE_PRICES_FOR_ZAKUPOK = value;
                RaisePropertyChanged();
            }
        }

        public short? TD_DAVLENCH_DOG
        {
            get => Entity.TD_DAVLENCH_DOG;
            set
            {
                if (Entity.TD_DAVLENCH_DOG == value) return;
                Entity.TD_DAVLENCH_DOG = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Тип договора - true - продажа, false - закупка 
        /// </summary>
        public bool IsSale
        {
            get => Entity.TD_0BUY_1SALE == 1;
            set
            {
                if (Entity.TD_0BUY_1SALE == 1 == value) return;
                Entity.TD_0BUY_1SALE = (short) (value ? 1 : 0);
                RaisePropertiesChanged();
            }
        }

        public List<SD_102> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual SD_102 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_102 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_102 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_102 ent)
        {
            TD_NAME = ent.TD_NAME;
            TD_0BUY_1SALE = ent.TD_0BUY_1SALE;
            TD_DOP_SOGL = ent.TD_DOP_SOGL;
            TD_DILER = ent.TD_DILER;
            TD_SF_AUTOMAT = ent.TD_SF_AUTOMAT;
            TD_IN_SF_WITH_TEK_PRICES = ent.TD_IN_SF_WITH_TEK_PRICES;
            TD_USE_PRICES_FOR_ZAKUPOK = ent.TD_USE_PRICES_FOR_ZAKUPOK;
            TD_DAVLENCH_DOG = ent.TD_DAVLENCH_DOG;
        }

        public void UpdateTo(SD_102 ent)
        {
            ent.TD_NAME = TD_NAME;
            ent.TD_0BUY_1SALE = TD_0BUY_1SALE;
            ent.TD_DOP_SOGL = TD_DOP_SOGL;
            ent.TD_DILER = TD_DILER;
            ent.TD_SF_AUTOMAT = TD_SF_AUTOMAT;
            ent.TD_IN_SF_WITH_TEK_PRICES = TD_IN_SF_WITH_TEK_PRICES;
            ent.TD_USE_PRICES_FOR_ZAKUPOK = TD_USE_PRICES_FOR_ZAKUPOK;
            ent.TD_DAVLENCH_DOG = TD_DAVLENCH_DOG;
        }
    }
    public class ContractType_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<ContractType>
    {
        void IMetadataProvider<ContractType>.BuildMetadata(
            MetadataBuilder<ContractType> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.IsSale).AutoGenerated().DisplayName("Договор продажи");
        }
    }
}