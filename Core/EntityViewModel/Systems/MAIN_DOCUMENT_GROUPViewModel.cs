using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Systems
{
    public class MAIN_DOCUMENT_GROUPViewModel : RSViewModelBase, IEntity<MAIN_DOCUMENT_GROUP>
    {
        private MAIN_DOCUMENT_GROUP myEntity;

        public MAIN_DOCUMENT_GROUPViewModel()
        {
            Entity = new MAIN_DOCUMENT_GROUP();
        }

        public MAIN_DOCUMENT_GROUPViewModel(MAIN_DOCUMENT_GROUP entity)
        {
            Entity = entity ?? new MAIN_DOCUMENT_GROUP();
        }

        public int ID
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

        public string NOTES
        {
            get => Entity.NOTES;
            set
            {
                if (Entity.NOTES == value) return;
                Entity.NOTES = value;
                RaisePropertyChanged();
            }
        }

        public int ORDERBY
        {
            get => Entity.ORDERBY;
            set
            {
                if (Entity.ORDERBY == value) return;
                Entity.ORDERBY = value;
                RaisePropertyChanged();
            }
        }

        public byte[] PICTURE
        {
            get => Entity.PICTURE;
            set
            {
                if (Entity.PICTURE == value) return;
                Entity.PICTURE = value;
                RaisePropertyChanged();
            }
        }

        public MAIN_DOCUMENT_GROUP Entity
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

        public List<MAIN_DOCUMENT_GROUP> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual MAIN_DOCUMENT_GROUP Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual MAIN_DOCUMENT_GROUP Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(MAIN_DOCUMENT_GROUP doc)
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

        public void UpdateFrom(MAIN_DOCUMENT_GROUP ent)
        {
            ID = ent.ID;
            NAME = ent.NAME;
            NOTES = ent.NOTES;
            ORDERBY = ent.ORDERBY;
            PICTURE = ent.PICTURE;
        }

        public void UpdateTo(MAIN_DOCUMENT_GROUP ent)
        {
            ent.ID = ID;
            ent.NAME = NAME;
            ent.NOTES = NOTES;
            ent.ORDERBY = ORDERBY;
            ent.PICTURE = PICTURE;
        }

        public MAIN_DOCUMENT_GROUP DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}