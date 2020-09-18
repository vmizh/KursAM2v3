using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class CLOSED_DOC_TYPEViewModel : RSViewModelBase, IEntity<CLOSED_DOC_TYPE>
    {
        private CLOSED_DOC_TYPE myEntity;

        public CLOSED_DOC_TYPEViewModel()
        {
            Entity = new CLOSED_DOC_TYPE();
        }

        public CLOSED_DOC_TYPEViewModel(CLOSED_DOC_TYPE entity)
        {
            Entity = entity ?? new CLOSED_DOC_TYPE();
        }

        public CLOSED_DOC_TYPE Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public Guid ID
        {
            get => Entity.ID;
            set
            {
                if (Entity.ID == value) return;
                Entity.ID = value;
                RaisePropertyChanged();
            }
        }

        public string NAME
        {
            get => Entity.NAME;
            set
            {
                if (Entity.NAME == value) return;
                Entity.NAME = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<CLOSED_DOC_TYPE> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual CLOSED_DOC_TYPE Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual CLOSED_DOC_TYPE Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(CLOSED_DOC_TYPE doc)
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

        public void UpdateFrom(CLOSED_DOC_TYPE ent)
        {
            ID = ent.ID;
            NAME = ent.NAME;
            //PERIOD_CLOSED = ent.PERIOD_CLOSED;
        }

        public void UpdateTo(CLOSED_DOC_TYPE ent)
        {
            ent.ID = ID;
            ent.NAME = NAME;
            //ent.PERIOD_CLOSED = PERIOD_CLOSED;
        }

        public CLOSED_DOC_TYPE DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}