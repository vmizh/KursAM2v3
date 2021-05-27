using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    /// <summary>
    /// sd_148 Категория клиентов
    /// </summary>
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [MetadataType(typeof(SD148_FluentAPI))]
    public class CategoryClientTypeViewModel : RSViewModelBase, IEntity<SD_148>
    {
        private SD_148 myEntity;

        public CategoryClientTypeViewModel()
        {
            Entity = DefaultValue();
        }

        public CategoryClientTypeViewModel(SD_148 entity)
        {
            Entity = entity ?? DefaultValue();
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

        public decimal CK_MIN_OBOROT
        {
            get => Entity.CK_MIN_OBOROT;
            set
            {
                if (Entity.CK_MIN_OBOROT == value) return;
                Entity.CK_MIN_OBOROT = value;
                RaisePropertyChanged();
            }
        }

        public double CK_MAX_PROSROCH_DNEY
        {
            get => Entity.CK_MAX_PROSROCH_DNEY;
            set
            {
                if (Entity.CK_MAX_PROSROCH_DNEY == value) return;
                Entity.CK_MAX_PROSROCH_DNEY = value;
                RaisePropertyChanged();
            }
        }

        public decimal CK_CREDIT_SUM
        {
            get => Entity.CK_CREDIT_SUM;
            set
            {
                if (Entity.CK_CREDIT_SUM == value) return;
                Entity.CK_CREDIT_SUM = value;
                RaisePropertyChanged();
            }
        }

        public override string Name
        {
            get => Entity.CK_NAME;
            set
            {
                if (Entity.CK_NAME == value) return;
                Entity.CK_NAME = value;
                RaisePropertyChanged();
            }
        }

        public double? CK_NACEN_DEFAULT_ROZN
        {
            get => Entity.CK_NACEN_DEFAULT_ROZN;
            set
            {
                if (Entity.CK_NACEN_DEFAULT_ROZN == value) return;
                Entity.CK_NACEN_DEFAULT_ROZN = value;
                RaisePropertyChanged();
            }
        }

        public double? CK_NACEN_DEFAULT_KOMPL
        {
            get => Entity.CK_NACEN_DEFAULT_KOMPL;
            set
            {
                if (Entity.CK_NACEN_DEFAULT_KOMPL == value) return;
                Entity.CK_NACEN_DEFAULT_KOMPL = value;
                RaisePropertyChanged();
            }
        }

        public short? CK_IMMEDIATE_PRICE_CHANGE
        {
            get => Entity.CK_IMMEDIATE_PRICE_CHANGE;
            set
            {
                if (Entity.CK_IMMEDIATE_PRICE_CHANGE == value) return;
                Entity.CK_IMMEDIATE_PRICE_CHANGE = value;
                RaisePropertyChanged();
            }
        }

        public string CK_GROUP
        {
            get => Entity.CK_GROUP;
            set
            {
                if (Entity.CK_GROUP == value) return;
                Entity.CK_GROUP = value;
                RaisePropertyChanged();
            }
        }

        public SD_148 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<SD_148> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SD_148 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_148 doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_148 ent)
        {
            
        }

        public void UpdateTo(SD_148 ent)
        {
        }

        public SD_148 DefaultValue()
        {
            return new SD_148
            {
                DOC_CODE = -1
            };
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public virtual SD_148 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public virtual SD_148 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_148 Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    public class SD148_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<CategoryClientTypeViewModel>
    {
        void IMetadataProvider<CategoryClientTypeViewModel>.BuildMetadata(
            MetadataBuilder<CategoryClientTypeViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.CK_CREDIT_SUM).AutoGenerated().DisplayName("Макс.кредит");
            builder.Property(_ => _.CK_MAX_PROSROCH_DNEY).AutoGenerated().DisplayName("Макс. просрочка(дн)");
            builder.Property(_ => _.CK_NACEN_DEFAULT_ROZN).AutoGenerated().DisplayName("Наценка");

        }
    }
}