using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel.CommonReferences.Kontragent
{
    public class SD_43_GRUZOViewModel : RSViewModelBase, IEntity<SD_43_GRUZO>
    {
        private SD_43_GRUZO myEntity;

        public SD_43_GRUZOViewModel()
        {
            Entity = new SD_43_GRUZO {Id = Guid.NewGuid()};
        }

        public SD_43_GRUZOViewModel(SD_43_GRUZO entity)
        {
            if (entity == null)
                Entity = new SD_43_GRUZO {Id = Guid.NewGuid()};
        }

        public decimal doc_code
        {
            get => Entity.doc_code;
            set
            {
                if (Entity.doc_code == value) return;
                Entity.doc_code = value;
                RaisePropertyChanged();
            }
        }

        public bool? IsDefault
        {
            get => Entity.IsDefault;
            set
            {
                if (Entity.IsDefault == value) return;
                Entity.IsDefault = value;
                RaisePropertyChanged();
            }
        }

        public string GRUZO_TEXT_SF
        {
            get => Entity.GRUZO_TEXT_SF;
            set
            {
                if (Entity.GRUZO_TEXT_SF == value) return;
                Entity.GRUZO_TEXT_SF = value;
                RaisePropertyChanged();
            }
        }

        public string OKPO
        {
            get => Entity.OKPO;
            set
            {
                if (Entity.OKPO == value) return;
                Entity.OKPO = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? CHANGED_DATE
        {
            get => Entity.CHANGED_DATE;
            set
            {
                if (Entity.CHANGED_DATE == value) return;
                Entity.CHANGED_DATE = value;
                RaisePropertyChanged();
            }
        }

        public string GRUZO_TEXT_NAKLAD
        {
            get => Entity.GRUZO_TEXT_NAKLAD;
            set
            {
                if (Entity.GRUZO_TEXT_NAKLAD == value) return;
                Entity.GRUZO_TEXT_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public SD_43 SD_43
        {
            get => Entity.SD_43;
            set
            {
                if (Entity.SD_43 == value) return;
                Entity.SD_43 = value;
                RaisePropertyChanged();
            }
        }

        public SD_43_GRUZO Entity
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

        public List<SD_43_GRUZO> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_43_GRUZO doc)
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

        public void UpdateFrom(SD_43_GRUZO ent)
        {
            doc_code = ent.doc_code;
            GRUZO_TEXT_SF = ent.GRUZO_TEXT_SF;
            OKPO = ent.OKPO;
            CHANGED_DATE = ent.CHANGED_DATE;
            GRUZO_TEXT_NAKLAD = ent.GRUZO_TEXT_NAKLAD;
            Id = ent.Id;
            SD_43 = ent.SD_43;
        }

        public void UpdateTo(SD_43_GRUZO ent)
        {
            ent.doc_code = doc_code;
            ent.GRUZO_TEXT_SF = GRUZO_TEXT_SF;
            ent.OKPO = OKPO;
            ent.CHANGED_DATE = CHANGED_DATE;
            ent.GRUZO_TEXT_NAKLAD = GRUZO_TEXT_NAKLAD;
            ent.Id = Id;
            ent.SD_43 = SD_43;
        }

        public SD_43_GRUZO DefaultValue()
        {
            return new SD_43_GRUZO
            {
                Id = Guid.NewGuid(),
                doc_code = -1
            };
        }

        public SD_43_GRUZO Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_43_GRUZO Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_43_GRUZO Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_43_GRUZO Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}