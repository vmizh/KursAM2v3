using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class NomenklCostResetViewModel : RSViewModelBase, IEntity<NomenklCostReset>
    {
        private NomenklCostReset myEntity;

        public NomenklCostResetViewModel()
        {
            Entity = DefaultValue();
        }

        public NomenklCostResetViewModel(NomenklCostReset entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }
        public int DocNumber
        {
            get => Entity.DocNumber;
            set
            {
                if (Entity.DocNumber == value) return;
                Entity.DocNumber = value;
                RaisePropertyChanged();
            }
        }
        public DateTime DocDate
        {
            get => Entity.DocDate;
            set
            {
                if (Entity.DocDate == value) return;
                Entity.DocDate = value;
                RaisePropertyChanged();
            }
        }
        public Guid SkladId
        {
            get => Entity.SkladId;
            set
            {
                if (Entity.SkladId == value) return;
                Entity.SkladId = value;
                RaisePropertyChanged();
            }
        }
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }
        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
                RaisePropertyChanged();
            }
        }
        public NomenklCostReset Entity
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

        public List<NomenklCostReset> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public NomenklCostReset Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NomenklCostReset doc)
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

        public void UpdateFrom(NomenklCostReset ent)
        {
            Id = ent.Id;
            DocNumber = ent.DocNumber;
            DocDate = ent.DocDate;
            SkladId = ent.SkladId;
            Note = ent.Note;
            Creator = ent.Creator;
        }

        public void UpdateTo(NomenklCostReset ent)
        {
            ent.Id = Id;
            ent.DocNumber = DocNumber;
            ent.DocDate = DocDate;
            ent.SkladId = SkladId;
            ent.Note = Note;
            ent.Creator = Creator;
        }

        public NomenklCostReset DefaultValue()
        {
            return new NomenklCostReset
            {
                Id = Guid.NewGuid(),
                Creator = GlobalOptions.UserInfo.NickName
            };
        }

        public virtual NomenklCostReset Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NomenklCostReset Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NomenklCostReset Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}