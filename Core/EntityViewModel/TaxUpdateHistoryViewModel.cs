using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class TaxUpdateHistoryViewModel : RSViewModelBase, IEntity<TaxUpdateHistory>
    {
        private TaxUpdateHistory myEntity;

        public TaxUpdateHistoryViewModel()
        {
            Entity = DefaultValue();
        }

        public TaxUpdateHistoryViewModel(TaxUpdateHistory entity)
        {
            Entity = entity ?? DefaultValue();
        }

        //public Guid Id
        //{
        //    get { return Entity.Id; }
        //    set
        //    {
        //        if (Entity.Id == value) return;
        //        Entity.Id = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public override Guid RowId
        {
            get => Entity.RowId;
            set
            {
                if (Entity.RowId == value) return;
                Entity.RowId = value;
                RaisePropertyChanged();
            }
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

        public string Updater
        {
            get => Entity.Updater;
            set
            {
                if (Entity.Updater == value) return;
                Entity.Updater = value;
                RaisePropertyChanged();
            }
        }

        public string SaleTaxRate
        {
            get => Entity.SaleTaxRate;
            set
            {
                if (Entity.SaleTaxRate == value) return;
                Entity.SaleTaxRate = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public TaxUpdateHistory Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public List<TaxUpdateHistory> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public TaxUpdateHistory DefaultValue()
        {
            return new TaxUpdateHistory
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now
            };
        }

        public virtual void Save(TaxUpdateHistory doc)
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

        public void UpdateFrom(TaxUpdateHistory ent)
        {
            Id = ent.Id;
            RowId = ent.RowId;
            Date = ent.Date;
            Updater = ent.Updater;
            SaleTaxRate = ent.SaleTaxRate;
        }

        public void UpdateTo(TaxUpdateHistory ent)
        {
            ent.Id = Id;
            ent.RowId = RowId;
            ent.Date = Date;
            ent.Updater = Updater;
            ent.SaleTaxRate = SaleTaxRate;
        }

        public TaxUpdateHistory Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public TaxUpdateHistory Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual TaxUpdateHistory Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TaxUpdateHistory Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}