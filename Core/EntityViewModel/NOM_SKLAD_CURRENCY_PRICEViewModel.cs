using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class NOM_SKLAD_CURRENCY_PRICEViewModel : RSViewModelBase, IEntity<NOM_SKLAD_CURRENCY_PRICE>
    {
        private NOM_SKLAD_CURRENCY_PRICE myEntity;

        public NOM_SKLAD_CURRENCY_PRICEViewModel()
        {
            Entity = DefaultValue();
        }

        public NOM_SKLAD_CURRENCY_PRICEViewModel(NOM_SKLAD_CURRENCY_PRICE entity)
        {
            Entity = entity ?? DefaultValue();
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

        public DateTime CostDate
        {
            get => Entity.CostDate;
            set
            {
                if (Entity.CostDate == value) return;
                Entity.CostDate = value;
                RaisePropertyChanged();
            }
        }

        public decimal Cost
        {
            get => Entity.Cost;
            set
            {
                if (Entity.Cost == value) return;
                Entity.Cost = value;
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

        public NOM_SKLAD_CURRENCY_PRICE Entity
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

        public List<NOM_SKLAD_CURRENCY_PRICE> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public NOM_SKLAD_CURRENCY_PRICE Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NOM_SKLAD_CURRENCY_PRICE doc)
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

        public void UpdateFrom(NOM_SKLAD_CURRENCY_PRICE ent)
        {
            Id = ent.Id;
            SkladId = ent.SkladId;
            CurrencyId = ent.CurrencyId;
            NomenklId = ent.NomenklId;
            CostDate = ent.CostDate;
            Cost = ent.Cost;
            Quantity = ent.Quantity;
            Note = ent.Note;
        }

        public void UpdateTo(NOM_SKLAD_CURRENCY_PRICE ent)
        {
            ent.Id = Id;
            ent.SkladId = SkladId;
            ent.CurrencyId = CurrencyId;
            ent.NomenklId = NomenklId;
            ent.CostDate = CostDate;
            ent.Cost = Cost;
            ent.Quantity = Quantity;
            ent.Note = Note;
        }

        public NOM_SKLAD_CURRENCY_PRICE DefaultValue()
        {
            return new NOM_SKLAD_CURRENCY_PRICE
            {
                Id = Guid.NewGuid()
            };
        }

        public virtual NOM_SKLAD_CURRENCY_PRICE Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NOM_SKLAD_CURRENCY_PRICE Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NOM_SKLAD_CURRENCY_PRICE Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}