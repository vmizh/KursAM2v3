using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    // ReSharper disable once InconsistentNaming
    public class SD_83_TransferViewModel : RSViewModelBase, IEntity<SD_83_Transfer>
    {
        private SD_83_Transfer myEntity;

        public SD_83_TransferViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_83_TransferViewModel(SD_83_Transfer entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public DateTime Date
        {
            get => Entity.Date;
            set
            {
                if (Entity.Date == value) return;
                Entity.Date = value;
                RaisePropertyChanged();
            }
        }
        public decimal SkladDC
        {
            get => Entity.SkladDC;
            set
            {
                if (Entity.SkladDC == value) return;
                Entity.SkladDC = value;
                RaisePropertyChanged();
            }
        }
        public decimal NomenklOutDC
        {
            get => Entity.NomenklOutDC;
            set
            {
                if (Entity.NomenklOutDC == value) return;
                Entity.NomenklOutDC = value;
                RaisePropertyChanged();
            }
        }
        public decimal NomenklInDC
        {
            get => Entity.NomenklInDC;
            set
            {
                if (Entity.NomenklInDC == value) return;
                Entity.NomenklInDC = value;
                RaisePropertyChanged();
            }
        }
        public decimal Quantity
        {
            get => Entity.Quantity;
            set
            {
                if (Entity.Quantity == value) return;
                Entity.Quantity = value;
                RaisePropertyChanged();
            }
        }
        public decimal PriceIn
        {
            get => Entity.PriceIn;
            set
            {
                if (Entity.PriceIn == value) return;
                Entity.PriceIn = value;
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
        public SD_27 SD_27
        {
            get => Entity.SD_27;
            set
            {
                if (Entity.SD_27 == value) return;
                Entity.SD_27 = value;
                RaisePropertyChanged();
            }
        }
        public SD_83 SD_83
        {
            get => Entity.SD_83;
            set
            {
                if (Entity.SD_83 == value) return;
                Entity.SD_83 = value;
                RaisePropertyChanged();
            }
        }
        public SD_83 SD_831
        {
            get => Entity.SD_831;
            set
            {
                if (Entity.SD_831 == value) return;
                Entity.SD_831 = value;
                RaisePropertyChanged();
            }
        }
        public SD_83_Transfer Entity
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

        public List<SD_83_Transfer> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_83_Transfer doc)
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

        public void UpdateFrom(SD_83_Transfer ent)
        {
            Id = ent.Id;
            Date = ent.Date;
            SkladDC = ent.SkladDC;
            NomenklOutDC = ent.NomenklOutDC;
            NomenklInDC = ent.NomenklInDC;
            Quantity = ent.Quantity;
            PriceIn = ent.PriceIn;
            Creator = ent.Creator;
            Note = ent.Note;
            SD_27 = ent.SD_27;
            SD_83 = ent.SD_83;
            SD_831 = ent.SD_831;
        }

        public void UpdateTo(SD_83_Transfer ent)
        {
            ent.Id = Id;
            ent.Date = Date;
            ent.SkladDC = SkladDC;
            ent.NomenklOutDC = NomenklOutDC;
            ent.NomenklInDC = NomenklInDC;
            ent.Quantity = Quantity;
            ent.PriceIn = PriceIn;
            ent.Creator = Creator;
            ent.Note = Note;
            ent.SD_27 = SD_27;
            ent.SD_83 = SD_83;
            ent.SD_831 = SD_831;
        }

        public SD_83_Transfer DefaultValue()
        {
            return new SD_83_Transfer {Id = Guid.NewGuid()};
        }

        public SD_83_Transfer Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_83_Transfer Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_83_Transfer Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_83_Transfer Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}