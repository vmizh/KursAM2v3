using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class KONTRAGENT_REF_OUTViewModel : RSViewModelBase, IEntity<KONTRAGENT_REF_OUT>
    {
        private KONTRAGENT_REF_OUT myEntity;

        public KONTRAGENT_REF_OUTViewModel()
        {
            Entity = DefaultValue();
        }

        public KONTRAGENT_REF_OUTViewModel(KONTRAGENT_REF_OUT entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public string Director
        {
            get => Entity.Director;
            set
            {
                if (Entity.Director == value) return;
                Entity.Director = value;
                RaisePropertyChanged();
            }
        }
        public string GlavBuh
        {
            get => Entity.GlavBuh;
            set
            {
                if (Entity.GlavBuh == value) return;
                Entity.GlavBuh = value;
                RaisePropertyChanged();
            }
        }
        public string INN
        {
            get => Entity.INN;
            set
            {
                if (Entity.INN == value) return;
                Entity.INN = value;
                RaisePropertyChanged();
            }
        }
        public string KPP
        {
            get => Entity.KPP;
            set
            {
                if (Entity.KPP == value) return;
                Entity.KPP = value;
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
        public string Address
        {
            get => Entity.Address;
            set
            {
                if (Entity.Address == value) return;
                Entity.Address = value;
                RaisePropertyChanged();
            }
        }
        public string OKONH
        {
            get => Entity.OKONH;
            set
            {
                if (Entity.OKONH == value) return;
                Entity.OKONH = value;
                RaisePropertyChanged();
            }
        }
        public decimal? OLD_DC
        {
            get => Entity.OLD_DC;
            set
            {
                if (Entity.OLD_DC == value) return;
                Entity.OLD_DC = value;
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
        public KONTRAGENT_REF_OUT Entity
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

        public List<KONTRAGENT_REF_OUT> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public KONTRAGENT_REF_OUT Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(KONTRAGENT_REF_OUT doc)
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

        public void UpdateFrom(KONTRAGENT_REF_OUT ent)
        {
            Id = ent.Id;
            Name = ent.NAME;
            Director = ent.Director;
            GlavBuh = ent.GlavBuh;
            Note = ent.Note;
            INN = ent.INN;
            KPP = ent.KPP;
            OKPO = ent.OKPO;
            Address = ent.Address;
            OKONH = ent.OKONH;
            OLD_DC = ent.OLD_DC;
            SD_43 = ent.SD_43;
        }

        public void UpdateTo(KONTRAGENT_REF_OUT ent)
        {
            ent.Id = Id;
            ent.NAME = Name;
            ent.Director = Director;
            ent.GlavBuh = GlavBuh;
            ent.Note = Note;
            ent.INN = INN;
            ent.KPP = KPP;
            ent.OKPO = OKPO;
            ent.Address = Address;
            ent.OKONH = OKONH;
            ent.OLD_DC = OLD_DC;
            ent.SD_43 = SD_43;
        }

        public KONTRAGENT_REF_OUT DefaultValue()
        {
            return new KONTRAGENT_REF_OUT
            {
                Id = Guid.NewGuid()
            };
        }

        public virtual KONTRAGENT_REF_OUT Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual KONTRAGENT_REF_OUT Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public KONTRAGENT_REF_OUT Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}