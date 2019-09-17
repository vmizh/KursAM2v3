using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class USER_FORMS_RIGHTViewModel : RSViewModelBase, IEntity<USER_FORMS_RIGHT>
    {
        private USER_FORMS_RIGHT myEntity;

        public USER_FORMS_RIGHTViewModel()
        {
            Entity = new USER_FORMS_RIGHT();
        }

        public USER_FORMS_RIGHTViewModel(USER_FORMS_RIGHT entity)
        {
            Entity = entity ?? new USER_FORMS_RIGHT();
        }

        public string USER_NAME
        {
            get => Entity.USER_NAME;
            set
            {
                if (Entity.USER_NAME == value) return;
                Entity.USER_NAME = value;
                RaisePropertyChanged();
            }
        }
        public int FORM_ID
        {
            get => Entity.FORM_ID;
            set
            {
                if (Entity.FORM_ID == value) return;
                Entity.FORM_ID = value;
                RaisePropertyChanged();
            }
        }
        public MAIN_DOCUMENT_ITEM MAIN_DOCUMENT_ITEM
        {
            get => Entity.MAIN_DOCUMENT_ITEM;
            set
            {
                if (Entity.MAIN_DOCUMENT_ITEM == value) return;
                Entity.MAIN_DOCUMENT_ITEM = value;
                RaisePropertyChanged();
            }
        }
        public USER_FORMS_RIGHT Entity
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

        public List<USER_FORMS_RIGHT> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual USER_FORMS_RIGHT Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual USER_FORMS_RIGHT Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(USER_FORMS_RIGHT doc)
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

        public void UpdateFrom(USER_FORMS_RIGHT ent)
        {
            USER_NAME = ent.USER_NAME;
            FORM_ID = ent.FORM_ID;
            MAIN_DOCUMENT_ITEM = ent.MAIN_DOCUMENT_ITEM;
        }

        public void UpdateTo(USER_FORMS_RIGHT ent)
        {
            ent.USER_NAME = USER_NAME;
            ent.FORM_ID = FORM_ID;
            ent.MAIN_DOCUMENT_ITEM = MAIN_DOCUMENT_ITEM;
        }

        public USER_FORMS_RIGHT DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}