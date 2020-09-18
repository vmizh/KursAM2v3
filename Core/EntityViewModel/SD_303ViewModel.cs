using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_303ViewModel : RSViewModelBase, IEntity<SD_303>
    {
        private SD_303 myEntity;

        public SD_303ViewModel()
        {
            Entity = new SD_303 {DOC_CODE = -1};
        }

        public SD_303ViewModel(SD_303 entity)
        {
            Entity = entity ?? new SD_303 {DOC_CODE = -1};
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
            get => Entity.SHPZ_NAME;
            set
            {
                if (Entity.SHPZ_NAME == value) return;
                Entity.SHPZ_NAME = value;
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

        public string SHPZ_NAME
        {
            get => Entity.SHPZ_NAME;
            set
            {
                if (Entity.SHPZ_NAME == value) return;
                Entity.SHPZ_NAME = value;
                RaisePropertyChanged();
            }
        }

        public short SHPZ_DELETED
        {
            get => Entity.SHPZ_DELETED;
            set
            {
                if (Entity.SHPZ_DELETED == value) return;
                Entity.SHPZ_DELETED = value;
                RaisePropertyChanged();
            }
        }

        public int? OP_CODE
        {
            get => Entity.OP_CODE;
            set
            {
                if (Entity.OP_CODE == value) return;
                Entity.OP_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string SHPZ_SHIRF
        {
            get => Entity.SHPZ_SHIRF;
            set
            {
                if (Entity.SHPZ_SHIRF == value) return;
                Entity.SHPZ_SHIRF = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_1OSN_0NO
        {
            get => Entity.SHPZ_1OSN_0NO;
            set
            {
                if (Entity.SHPZ_1OSN_0NO == value) return;
                Entity.SHPZ_1OSN_0NO = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SHPZ_STATIA_DC
        {
            get => Entity.SHPZ_STATIA_DC;
            set
            {
                if (Entity.SHPZ_STATIA_DC == value) return;
                Entity.SHPZ_STATIA_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SHPZ_ELEMENT_DC
        {
            get => Entity.SHPZ_ELEMENT_DC;
            set
            {
                if (Entity.SHPZ_ELEMENT_DC == value) return;
                Entity.SHPZ_ELEMENT_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_PODOTCHET
        {
            get => Entity.SHPZ_PODOTCHET;
            set
            {
                if (Entity.SHPZ_PODOTCHET == value) return;
                Entity.SHPZ_PODOTCHET = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_1DOHOD_0_RASHOD
        {
            get => Entity.SHPZ_1DOHOD_0_RASHOD;
            set
            {
                if (Entity.SHPZ_1DOHOD_0_RASHOD == value) return;
                Entity.SHPZ_1DOHOD_0_RASHOD = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_1TARIFIC_0NO
        {
            get => Entity.SHPZ_1TARIFIC_0NO;
            set
            {
                if (Entity.SHPZ_1TARIFIC_0NO == value) return;
                Entity.SHPZ_1TARIFIC_0NO = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_1ZARPLATA_0NO
        {
            get => Entity.SHPZ_1ZARPLATA_0NO;
            set
            {
                if (Entity.SHPZ_1ZARPLATA_0NO == value) return;
                Entity.SHPZ_1ZARPLATA_0NO = value;
                RaisePropertyChanged();
            }
        }

        public short? SHPZ_NOT_USE_IN_OTCH_DDS
        {
            get => Entity.SHPZ_NOT_USE_IN_OTCH_DDS;
            set
            {
                if (Entity.SHPZ_NOT_USE_IN_OTCH_DDS == value) return;
                Entity.SHPZ_NOT_USE_IN_OTCH_DDS = value;
                RaisePropertyChanged();
            }
        }

        public SD_99 SD_99
        {
            get => Entity.SD_99;
            set
            {
                if (Entity.SD_99 == value) return;
                Entity.SD_99 = value;
                RaisePropertyChanged();
            }
        }

        public SD_97 SD_97
        {
            get => Entity.SD_97;
            set
            {
                if (Entity.SD_97 == value) return;
                Entity.SD_97 = value;
                RaisePropertyChanged();
            }
        }

        public SD_303 Entity
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

        public List<SD_303> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_303 doc)
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

        public void UpdateFrom(SD_303 ent)
        {
            SHPZ_NAME = ent.SHPZ_NAME;
            SHPZ_DELETED = ent.SHPZ_DELETED;
            OP_CODE = ent.OP_CODE;
            SHPZ_SHIRF = ent.SHPZ_SHIRF;
            SHPZ_1OSN_0NO = ent.SHPZ_1OSN_0NO;
            SHPZ_STATIA_DC = ent.SHPZ_STATIA_DC;
            SHPZ_ELEMENT_DC = ent.SHPZ_ELEMENT_DC;
            SHPZ_PODOTCHET = ent.SHPZ_PODOTCHET;
            SHPZ_1DOHOD_0_RASHOD = ent.SHPZ_1DOHOD_0_RASHOD;
            SHPZ_1TARIFIC_0NO = ent.SHPZ_1TARIFIC_0NO;
            SHPZ_1ZARPLATA_0NO = ent.SHPZ_1ZARPLATA_0NO;
            SHPZ_NOT_USE_IN_OTCH_DDS = ent.SHPZ_NOT_USE_IN_OTCH_DDS;
        }

        public void UpdateTo(SD_303 ent)
        {
            ent.SHPZ_NAME = SHPZ_NAME;
            ent.SHPZ_DELETED = SHPZ_DELETED;
            ent.OP_CODE = OP_CODE;
            ent.SHPZ_SHIRF = SHPZ_SHIRF;
            ent.SHPZ_1OSN_0NO = SHPZ_1OSN_0NO;
            ent.SHPZ_STATIA_DC = SHPZ_STATIA_DC;
            ent.SHPZ_ELEMENT_DC = SHPZ_ELEMENT_DC;
            ent.SHPZ_PODOTCHET = SHPZ_PODOTCHET;
            ent.SHPZ_1DOHOD_0_RASHOD = SHPZ_1DOHOD_0_RASHOD;
            ent.SHPZ_1TARIFIC_0NO = SHPZ_1TARIFIC_0NO;
            ent.SHPZ_1ZARPLATA_0NO = SHPZ_1ZARPLATA_0NO;
            ent.SHPZ_NOT_USE_IN_OTCH_DDS = SHPZ_NOT_USE_IN_OTCH_DDS;
        }

        public SD_303 DefaultValue()
        {
            throw new NotImplementedException();
        }

        public SD_303 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_303 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_303 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_303 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}