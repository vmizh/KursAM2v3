using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_40ViewModel : RSViewModelBase, IEntity<SD_40>
    {
        private SD_40 myEntity;

        public SD_40ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_40ViewModel(SD_40 entity)
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

        public string CENT_FULLNAME
        {
            get => Entity.CENT_FULLNAME;
            set
            {
                if (Entity.CENT_FULLNAME == value) return;
                Entity.CENT_FULLNAME = value;
                RaisePropertyChanged();
            }
        }

        public string CENT_NAME
        {
            get => Entity.CENT_NAME;
            set
            {
                if (Entity.CENT_NAME == value) return;
                Entity.CENT_NAME = value;
                RaisePropertyChanged();
            }
        }

        public decimal? CENT_PARENT_DC
        {
            get => Entity.CENT_PARENT_DC;
            set
            {
                if (Entity.CENT_PARENT_DC == value) return;
                Entity.CENT_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }

        public int? IS_DELETED
        {
            get => Entity.IS_DELETED;
            set
            {
                if (Entity.IS_DELETED == value) return;
                Entity.IS_DELETED = value;
                RaisePropertyChanged();
            }
        }

        public SD_40 SD_402
        {
            get => Entity.SD_402;
            set
            {
                if (Entity.SD_402 == value) return;
                Entity.SD_402 = value;
                RaisePropertyChanged();
            }
        }

        public SD_40 Entity
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

        public List<SD_40> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_40 doc)
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

        public void UpdateFrom(SD_40 ent)
        {
            CENT_FULLNAME = ent.CENT_FULLNAME;
            CENT_NAME = ent.CENT_NAME;
            CENT_PARENT_DC = ent.CENT_PARENT_DC;
            IS_DELETED = ent.IS_DELETED;
            SD_402 = ent.SD_402;
        }

        public void UpdateTo(SD_40 ent)
        {
            ent.CENT_FULLNAME = CENT_FULLNAME;
            ent.CENT_NAME = CENT_NAME;
            ent.CENT_PARENT_DC = CENT_PARENT_DC;
            ent.IS_DELETED = IS_DELETED;
            ent.SD_402 = SD_402;
        }

        public SD_40 DefaultValue()
        {
            return new SD_40
            {
                DOC_CODE = -1
            };
        }

        public SD_40 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_40 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_40 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_40 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}