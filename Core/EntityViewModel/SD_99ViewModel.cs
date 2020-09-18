using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_99ViewModel : RSViewModelBase, IEntity<SD_99>
    {
        private SD_99 myEntity;

        public SD_99ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_99ViewModel(SD_99 entity)
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

        public override string Name
        {
            get => Entity.SZ_NAME;
            set
            {
                if (Entity.SZ_NAME == value) return;
                Entity.SZ_NAME = value;
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

        public string SZ_NAME
        {
            get => Entity.SZ_NAME;
            set
            {
                if (Entity.SZ_NAME == value) return;
                Entity.SZ_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string SZ_SHIFR
        {
            get => Entity.SZ_SHIFR;
            set
            {
                if (Entity.SZ_SHIFR == value) return;
                Entity.SZ_SHIFR = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SZ_PARENT_DC
        {
            get => Entity.SZ_PARENT_DC;
            set
            {
                if (Entity.SZ_PARENT_DC == value) return;
                Entity.SZ_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? SZ_1DOHOD_0_RASHOD
        {
            get => Entity.SZ_1DOHOD_0_RASHOD;
            set
            {
                if (Entity.SZ_1DOHOD_0_RASHOD == value) return;
                Entity.SZ_1DOHOD_0_RASHOD = value;
                RaisePropertyChanged();
            }
        }

        public SD_99 SD_992
        {
            get => Entity.SD_992;
            set
            {
                if (Entity.SD_992 == value) return;
                Entity.SD_992 = value;
                RaisePropertyChanged();
            }
        }

        public SD_99 Entity
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

        public List<SD_99> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_99 doc)
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

        public void UpdateFrom(SD_99 ent)
        {
            SZ_NAME = ent.SZ_NAME;
            SZ_SHIFR = ent.SZ_SHIFR;
            SZ_PARENT_DC = ent.SZ_PARENT_DC;
            SZ_1DOHOD_0_RASHOD = ent.SZ_1DOHOD_0_RASHOD;
            SD_992 = ent.SD_992;
        }

        public void UpdateTo(SD_99 ent)
        {
            ent.SZ_NAME = SZ_NAME;
            ent.SZ_SHIFR = SZ_SHIFR;
            ent.SZ_PARENT_DC = SZ_PARENT_DC;
            ent.SZ_1DOHOD_0_RASHOD = SZ_1DOHOD_0_RASHOD;
            ent.SD_992 = SD_992;
        }

        public SD_99 DefaultValue()
        {
            return new SD_99 {DOC_CODE = -1};
        }

        public SD_99 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_99 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_99 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_99 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}