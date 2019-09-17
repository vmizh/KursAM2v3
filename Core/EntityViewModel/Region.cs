using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable UnusedMember.Global
namespace Core.EntityViewModel
{
    /// <summary>
    ///     Регионы - справочник
    /// </summary>
    [MetadataType(typeof(DataAnnotationsRegion))]
    public class Region : RSViewModelBase, IEntity<SD_23>
    {
        private SD_23 myEntity;

        public Region()
        {
            Entity = DefaultValue();
        }

        public Region(SD_23 entity)
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
        public string REG_NAME
        {
            get => Entity.REG_NAME;
            set
            {
                if (Entity.REG_NAME == value) return;
                Entity.REG_NAME = value;
                RaisePropertyChanged();
            }
        }
        public override string Name
        {
            get => REG_NAME;
            set
            {
                if (REG_NAME == value) return;
                REG_NAME = value;
                RaisePropertyChanged();
            }
        }
        public decimal? REG_PARENT_DC
        {
            get => Entity.REG_PARENT_DC;
            set
            {
                if (Entity.REG_PARENT_DC == value) return;
                Entity.REG_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public override decimal? ParentDC
        {
            get => REG_PARENT_DC;
            set
            {
                if (REG_PARENT_DC == value) return;
                REG_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }

        //TODO Подключить к регионам страны
        public decimal? REG_STRANA_DC
        {
            get => Entity.REG_STRANA_DC;
            set
            {
                if (Entity.REG_STRANA_DC == value) return;
                Entity.REG_STRANA_DC = value;
                RaisePropertyChanged();
            }
        }
        public SD_23 SD_232
        {
            get => Entity.SD_232;
            set
            {
                if (Entity.SD_232 == value) return;
                Entity.SD_232 = value;
                RaisePropertyChanged();
            }
        }
        public SD_434 SD_434
        {
            get => Entity.SD_434;
            set
            {
                if (Entity.SD_434 == value) return;
                Entity.SD_434 = value;
                RaisePropertyChanged();
            }
        }
        public SD_23 Entity
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

        public List<SD_23> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SD_23 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_23 doc)
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

        public void UpdateFrom(SD_23 ent)
        {
            REG_NAME = ent.REG_NAME;
            REG_PARENT_DC = ent.REG_PARENT_DC;
            REG_STRANA_DC = ent.REG_STRANA_DC;
            SD_232 = ent.SD_232;
            SD_434 = ent.SD_434;
        }

        public void UpdateTo(SD_23 ent)
        {
            ent.REG_NAME = REG_NAME;
            ent.REG_PARENT_DC = REG_PARENT_DC;
            ent.REG_STRANA_DC = REG_STRANA_DC;
            ent.SD_232 = SD_232;
            ent.SD_434 = SD_434;
        }

        public SD_23 DefaultValue()
        {
            return new SD_23
            {
                DOC_CODE = -1
            };
        }

        public virtual SD_23 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_23 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_23 Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsRegion : DataAnnotationForFluentApiBase, IMetadataProvider<Region>
    {
        void IMetadataProvider<Region>.BuildMetadata(MetadataBuilder<Region> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            //builder.Property(_ => _.IsCanNegativeRest).AutoGenerated().DisplayName("Отрицат.остатки");
            //builder.Property(_ => _.).AutoGenerated().DisplayName("Наименование");
        }
    }
}