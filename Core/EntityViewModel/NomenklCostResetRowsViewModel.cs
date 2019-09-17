using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class NomenklCostResetRowsViewModel : RSViewModelBase, IEntity<NomenklCostResetRows>
    {
        private NomenklCostResetRows myEntity;

        public NomenklCostResetRowsViewModel()
        {
            Entity = DefaultValue();
        }

        public NomenklCostResetRowsViewModel(NomenklCostResetRows entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public Guid DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                RaisePropertyChanged();
            }
        }
        public Guid NomenklId
        {
            get => Entity.NomenklId;
            set
            {
                if (Entity.NomenklId == value) return;
                Entity.NomenklId = value;
                RaisePropertyChanged();
            }
        }
        public Guid CurrencyId
        {
            get => Entity.CurrencyId;
            set
            {
                if (Entity.CurrencyId == value) return;
                Entity.CurrencyId = value;
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
        public Guid CurrencyNewId
        {
            get => Entity.CurrencyNewId;
            set
            {
                if (Entity.CurrencyNewId == value) return;
                Entity.CurrencyNewId = value;
                RaisePropertyChanged();
            }
        }
        public decimal QuantityReset
        {
            get => Entity.QuantityReset;
            set
            {
                if (Entity.QuantityReset == value) return;
                Entity.QuantityReset = value;
                RaisePropertyChanged();
            }
        }
        public decimal Rate
        {
            get => Entity.Rate;
            set
            {
                if (Entity.Rate == value) return;
                Entity.Rate = value;
                RaisePropertyChanged();
            }
        }
        public NomenklCostReset NomenklCostReset
        {
            get => Entity.NomenklCostReset;
            set
            {
                if (Entity.NomenklCostReset == value) return;
                Entity.NomenklCostReset = value;
                RaisePropertyChanged();
            }
        }
        public NomenklCostResetRows Entity
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

        public List<NomenklCostResetRows> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public NomenklCostResetRows Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NomenklCostResetRows doc)
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

        public void UpdateFrom(NomenklCostResetRows ent)
        {
            Id = ent.Id;
            DocId = ent.DocId;
            NomenklId = ent.NomenklId;
            CurrencyId = ent.CurrencyId;
            Quantity = ent.Quantity;
            CurrencyNewId = ent.CurrencyNewId;
            QuantityReset = ent.QuantityReset;
            Rate = ent.Rate;
            Note = ent.Note;
            NomenklCostReset = ent.NomenklCostReset;
        }

        public void UpdateTo(NomenklCostResetRows ent)
        {
            ent.Id = Id;
            ent.DocId = DocId;
            ent.NomenklId = NomenklId;
            ent.CurrencyId = CurrencyId;
            ent.Quantity = Quantity;
            ent.CurrencyNewId = CurrencyNewId;
            ent.QuantityReset = QuantityReset;
            ent.Rate = Rate;
            ent.Note = Note;
            ent.NomenklCostReset = NomenklCostReset;
        }

        public NomenklCostResetRows DefaultValue()
        {
            return new NomenklCostResetRows
            {
                Id = Guid.NewGuid(),
                Rate = 0
            };
        }

        public virtual NomenklCostResetRows Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NomenklCostResetRows Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NomenklCostResetRows Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}