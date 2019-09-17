using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class KontragentGroup : RSViewModelBase, IEntity<UD_43>
    {
        private UD_43 myEntity;

        public KontragentGroup()
        {
            Entity = DefaultValue();
        }

        public KontragentGroup(UD_43 entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public int EG_ID
        {
            get => Entity.EG_ID;
            set
            {
                if (Entity.EG_ID == value) return;
                Entity.EG_ID = value;
                RaisePropertyChanged();
            }
        }
        public override decimal DocCode
        {
            get => EG_ID;
            set
            {
                if (EG_ID == value) return;
                EG_ID = (int) value;
                RaisePropertyChanged();
            }
        }
        public string EG_NAME
        {
            get => Entity.EG_NAME;
            set
            {
                if (Entity.EG_NAME == value) return;
                Entity.EG_NAME = value;
                RaisePropertyChanged();
            }
        }
        public override string Name
        {
            get => EG_NAME;
            set
            {
                if (EG_NAME == value) return;
                EG_NAME = value;
                RaisePropertyChanged();
            }
        }
        public int? EG_PARENT_ID
        {
            get => Entity.EG_PARENT_ID;
            set
            {
                if (Entity.EG_PARENT_ID == value) return;
                Entity.EG_PARENT_ID = value;
                RaisePropertyChanged();
            }
        }
        public override decimal? ParentDC
        {
            get => EG_PARENT_ID;
            set
            {
                if (EG_PARENT_ID == value) return;
                EG_PARENT_ID = (int?) value;
                RaisePropertyChanged();
            }
        }
        public short? EG_DELETED
        {
            get => Entity.EG_DELETED;
            set
            {
                if (Entity.EG_DELETED == value) return;
                Entity.EG_DELETED = value;
                RaisePropertyChanged();
            }
        }
        public bool IsDeleted
        {
            get => EG_DELETED == 1;
            set
            {
                if (EG_DELETED == 1 == value) return;
                EG_DELETED = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        public short? EG_BALANS_GROUP
        {
            get => Entity.EG_BALANS_GROUP;
            set
            {
                if (Entity.EG_BALANS_GROUP == value) return;
                Entity.EG_BALANS_GROUP = value;
                RaisePropertyChanged();
            }
        }
        public UD_43 UD_432
        {
            get => Entity.UD_432;
            set
            {
                if (Entity.UD_432 == value) return;
                Entity.UD_432 = value;
                RaisePropertyChanged();
            }
        }
        public UD_43 Entity
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

        public List<UD_43> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(UD_43 doc)
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

        public void UpdateFrom(UD_43 ent)
        {
            EG_ID = ent.EG_ID;
            EG_NAME = ent.EG_NAME;
            EG_PARENT_ID = ent.EG_PARENT_ID;
            EG_DELETED = ent.EG_DELETED;
            EG_BALANS_GROUP = ent.EG_BALANS_GROUP;
            UD_432 = ent.UD_432;
        }

        public void UpdateTo(UD_43 ent)
        {
            ent.EG_ID = EG_ID;
            ent.EG_NAME = EG_NAME;
            ent.EG_PARENT_ID = EG_PARENT_ID;
            ent.EG_DELETED = EG_DELETED;
            ent.EG_BALANS_GROUP = EG_BALANS_GROUP;
            ent.UD_432 = UD_432;
        }

        public UD_43 DefaultValue()
        {
            return new UD_43
            {
                EG_ID = -1,
                EG_DELETED = 0
            };
        }

        public UD_43 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public UD_43 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual UD_43 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual UD_43 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}