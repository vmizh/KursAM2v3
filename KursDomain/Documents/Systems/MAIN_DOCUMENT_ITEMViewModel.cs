using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Systems
{
    public class MAIN_DOCUMENT_ITEMViewModel : RSViewModelBase, IEntity<MAIN_DOCUMENT_ITEM>
    {
        private MAIN_DOCUMENT_ITEM myEntity;

        public MAIN_DOCUMENT_ITEMViewModel()
        {
            Entity = new MAIN_DOCUMENT_ITEM();
        }

        public MAIN_DOCUMENT_ITEMViewModel(MAIN_DOCUMENT_ITEM entity)
        {
            Entity = entity ?? new MAIN_DOCUMENT_ITEM();
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

        public int GROUP_ID
        {
            get => Entity.GROUP_ID;
            set
            {
                if (Entity.GROUP_ID == value) return;
                Entity.GROUP_ID = value;
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

        public int? ORDERBY
        {
            get => Entity.ORDERBY;
            set
            {
                if (Entity.ORDERBY == value) return;
                Entity.ORDERBY = value;
                RaisePropertyChanged();
            }
        }

        public string SCODE
        {
            get => Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            }
        }

        public MAIN_DOCUMENT_GROUP MAIN_DOCUMENT_GROUP
        {
            get => Entity.MAIN_DOCUMENT_GROUP;
            set
            {
                if (Entity.MAIN_DOCUMENT_GROUP == value) return;
                Entity.MAIN_DOCUMENT_GROUP = value;
                RaisePropertyChanged();
            }
        }

        public MAIN_DOCUMENT_ITEM Entity
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

        public List<MAIN_DOCUMENT_ITEM> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual MAIN_DOCUMENT_ITEM Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual MAIN_DOCUMENT_ITEM Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(MAIN_DOCUMENT_ITEM doc)
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

        public void UpdateFrom(MAIN_DOCUMENT_ITEM ent)
        {
            ID = ent.ID;
            GROUP_ID = ent.GROUP_ID;
            NAME = ent.NAME;
            PICTURE = ent.PICTURE;
            NOTES = ent.NOTES;
            ORDERBY = ent.ORDERBY;
            SCODE = ent.CODE;
            MAIN_DOCUMENT_GROUP = ent.MAIN_DOCUMENT_GROUP;
        }

        public void UpdateTo(MAIN_DOCUMENT_ITEM ent)
        {
            ent.ID = ID;
            ent.GROUP_ID = GROUP_ID;
            ent.NAME = NAME;
            ent.PICTURE = PICTURE;
            ent.NOTES = NOTES;
            ent.ORDERBY = ORDERBY;
            ent.CODE = SCODE;
            ent.MAIN_DOCUMENT_GROUP = MAIN_DOCUMENT_GROUP;
        }

        public MAIN_DOCUMENT_ITEM DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}