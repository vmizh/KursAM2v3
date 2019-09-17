using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Единица измерения
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    [MetadataType(typeof(Unit_FluentAPI))]
    public class Unit : RSViewModelBase, IEntity<SD_175>
    {
        private SD_175 myEntity;

        public Unit()
        {
            Entity = DefaultValue();
        }

        public Unit(SD_175 entity)
        {
            Entity = entity ?? DefaultValue();
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
            get => DOC_CODE;
            set
            {
                if (DOC_CODE == value) return;
                DOC_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string ED_IZM_NAME
        {
            get => Entity.ED_IZM_NAME;
            set
            {
                if (Entity.ED_IZM_NAME == value) return;
                Entity.ED_IZM_NAME = value;
                RaisePropertyChanged();
            }
        }
        public override string Name
        {
            get => ED_IZM_NAME;
            set
            {
                if (ED_IZM_NAME == value) return;
                ED_IZM_NAME = value;
                RaisePropertyChanged();
            }
        }
        public string ED_IZM_OKEI
        {
            get => Entity.ED_IZM_OKEI;
            set
            {
                if (Entity.ED_IZM_OKEI == value) return;
                Entity.ED_IZM_OKEI = value;
                RaisePropertyChanged();
            }
        }
        public short? ED_IZM_MONEY
        {
            get => Entity.ED_IZM_MONEY;
            set
            {
                if (Entity.ED_IZM_MONEY == value) return;
                Entity.ED_IZM_MONEY = value;
                RaisePropertyChanged();
            }
        }
        public short? ED_IZM_INT
        {
            get => Entity.ED_IZM_INT;
            set
            {
                if (Entity.ED_IZM_INT == value) return;
                Entity.ED_IZM_INT = value;
                RaisePropertyChanged();
            }
        }
        public string ED_IZM_OKEI_CODE
        {
            get => Entity.ED_IZM_OKEI_CODE;
            set
            {
                if (Entity.ED_IZM_OKEI_CODE == value) return;
                Entity.ED_IZM_OKEI_CODE = value;
                RaisePropertyChanged();
            }
        }
        public SD_175 Entity
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

        public List<SD_175> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SD_175 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_175 doc)
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

        public void UpdateFrom(SD_175 ent)
        {
            ED_IZM_NAME = ent.ED_IZM_NAME;
            ED_IZM_OKEI = ent.ED_IZM_OKEI;
            ED_IZM_MONEY = ent.ED_IZM_MONEY;
            ED_IZM_INT = ent.ED_IZM_INT;
            ED_IZM_OKEI_CODE = ent.ED_IZM_OKEI_CODE;
        }

        public void UpdateTo(SD_175 ent)
        {
            ent.ED_IZM_NAME = ED_IZM_NAME;
            ent.ED_IZM_OKEI = ED_IZM_OKEI;
            ent.ED_IZM_MONEY = ED_IZM_MONEY;
            ent.ED_IZM_INT = ED_IZM_INT;
            ent.ED_IZM_OKEI_CODE = ED_IZM_OKEI_CODE;
        }

        public SD_175 DefaultValue()
        {
            return new SD_175
            {
                DOC_CODE = -1
            };
        }

        public virtual SD_175 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_175 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_175 Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    public class Unit_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<Unit>
    {
        void IMetadataProvider<Unit>.BuildMetadata(
            MetadataBuilder<Unit> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(x => x.Name).AutoGenerated()
                .DisplayName("Наименование");
            builder.Property(x => x.ED_IZM_OKEI).AutoGenerated()
                .DisplayName("OKEI");
            builder.Property(x => x.ED_IZM_OKEI_CODE).AutoGenerated()
                .DisplayName("OKEI Code");
        }
    }
}