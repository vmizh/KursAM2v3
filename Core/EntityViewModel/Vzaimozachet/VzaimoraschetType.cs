﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel.Vzaimozachet
{
    [MetadataType(typeof(DataAnnotationsSD_77ViewModel))]
    public sealed class VzaimoraschetType : RSViewModelBase, IEntity<SD_77>
    {
        private SD_77 myEntity;

        private SDRSchet mySHPZ;

        public VzaimoraschetType()
        {
            Entity = DefaultValue();
        }

        public VzaimoraschetType(SD_77 entity)
        {
            Entity = entity ?? DefaultValue();
            if (Entity.TV_SHPZ_DC != null)
                SHPZ = MainReferences.SDRSchets[Entity.TV_SHPZ_DC.Value];
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

        public override string Name
        {
            get => Entity.TV_NAME;
            set
            {
                if (Entity.TV_NAME == value) return;
                Entity.TV_NAME = value;
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

        public string TV_NAME
        {
            get => Entity.TV_NAME;
            set
            {
                if (Entity.TV_NAME == value) return;
                Entity.TV_NAME = value;
                RaisePropertyChanged();
            }
        }

        public short? TV_TYPE
        {
            get => Entity.TV_TYPE;
            set
            {
                if (Entity.TV_TYPE == value) return;
                Entity.TV_TYPE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? TV_SHPZ_DC
        {
            get => Entity.TV_SHPZ_DC;
            set
            {
                if (Entity.TV_SHPZ_DC == value) return;
                Entity.TV_SHPZ_DC = value;
                mySHPZ = Entity.TV_SHPZ_DC != null ? MainReferences.SDRSchets[Entity.TV_SHPZ_DC.Value] : null;
                RaisePropertyChanged(nameof(SHPZ));
                RaisePropertyChanged();
            }
        }

        public SDRSchet SHPZ
        {
            get => mySHPZ;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (mySHPZ == value) return;
                mySHPZ = value;
                if (mySHPZ != null)
                    TV_SHPZ_DC = mySHPZ.DocCode;
                RaisePropertyChanged();
            }
        }

        public SD_303 SD_303
        {
            get => Entity.SD_303;
            set
            {
                if (Entity.SD_303 == value) return;
                Entity.SD_303 = value;
                RaisePropertyChanged();
            }
        }

        public SD_77 Entity
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

        public List<SD_77> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public void Save(SD_77 doc)
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

        public void UpdateFrom(SD_77 ent)
        {
            TV_NAME = ent.TV_NAME;
            TV_TYPE = ent.TV_TYPE;
            TV_SHPZ_DC = ent.TV_SHPZ_DC;
        }

        public void UpdateTo(SD_77 ent)
        {
            ent.TV_NAME = TV_NAME;
            ent.TV_TYPE = TV_TYPE;
            ent.TV_SHPZ_DC = TV_SHPZ_DC;
        }

        public SD_77 DefaultValue()
        {
            return new SD_77
            {
                DOC_CODE = -1
            };
        }

        public SD_77 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_77 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_77 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public SD_77 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsSD_77ViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<VzaimoraschetType>
    {
        void IMetadataProvider<VzaimoraschetType>.BuildMetadata(MetadataBuilder<VzaimoraschetType> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.SHPZ).AutoGenerated().DisplayName("Счет доходов/расходов");
        }
    }
}