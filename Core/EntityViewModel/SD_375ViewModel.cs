using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_375ViewModel : RSViewModelBase, IEntity<SD_375>
    {
        private SD_375 myEntity;

        public SD_375ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_375ViewModel(SD_375 entity)
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
        public short DOP_USL_DEFAULT
        {
            get => Entity.DOP_USL_DEFAULT;
            set
            {
                if (Entity.DOP_USL_DEFAULT == value) return;
                Entity.DOP_USL_DEFAULT = value;
                RaisePropertyChanged();
            }
        }
        public string DOP_USL_TEXT
        {
            get => Entity.DOP_USL_TEXT;
            set
            {
                if (Entity.DOP_USL_TEXT == value) return;
                Entity.DOP_USL_TEXT = value;
                RaisePropertyChanged();
            }
        }
        public SD_375 Entity
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

        public List<SD_375> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_375 doc)
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

        public void UpdateFrom(SD_375 ent)
        {
            DOP_USL_DEFAULT = ent.DOP_USL_DEFAULT;
            DOP_USL_TEXT = ent.DOP_USL_TEXT;
        }

        public void UpdateTo(SD_375 ent)
        {
            ent.DOP_USL_DEFAULT = DOP_USL_DEFAULT;
            ent.DOP_USL_TEXT = DOP_USL_TEXT;
        }

        public SD_375 DefaultValue()
        {
            return new SD_375
            {
                DOC_CODE = -1,
                DOP_USL_DEFAULT = 0
            };
        }

        public SD_375 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_375 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_375 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_375 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}