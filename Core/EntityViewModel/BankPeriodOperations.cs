using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class BankPeriodsOperationsViewModel : RSViewModelBase, IEntity<BankPeriodsOperations>
    {
        private BankPeriodsOperations myEntity;

        public BankPeriodsOperationsViewModel()
        {
            Entity = new BankPeriodsOperations();
        }

        public BankPeriodsOperationsViewModel(BankPeriodsOperations entity)
        {
            Entity = entity ?? new BankPeriodsOperations();
        }

        public BankPeriodsOperations Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public decimal BankDC
        {
            get => Entity.BankDC;
            set
            {
                if (Entity.BankDC == value) return;
                Entity.BankDC = value;
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

        public decimal CrsDC
        {
            get => Entity.CrsDC;
            set
            {
                if (Entity.CrsDC == value) return;
                Entity.CrsDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SummaStart
        {
            get => Entity.SummaStart;
            set
            {
                if (Entity.SummaStart == value) return;
                Entity.SummaStart = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SummaIn
        {
            get => Entity.SummaIn;
            set
            {
                if (Entity.SummaIn == value) return;
                Entity.SummaIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SummaOut
        {
            get => Entity.SummaOut;
            set
            {
                if (Entity.SummaOut == value) return;
                Entity.SummaOut = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SummaEnd
        {
            get => Entity.SummaEnd;
            set
            {
                if (Entity.SummaEnd == value) return;
                Entity.SummaEnd = value;
                RaisePropertyChanged();
            }
        }

        public List<BankPeriodsOperations> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual BankPeriodsOperations Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual BankPeriodsOperations Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(BankPeriodsOperations doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(BankPeriodsOperations ent)
        {
            BankDC = ent.BankDC;
            Date = ent.Date;
            CrsDC = ent.CrsDC;
            SummaStart = ent.SummaStart;
            SummaIn = ent.SummaIn;
            SummaOut = ent.SummaOut;
            SummaEnd = ent.SummaEnd;
        }

        public void UpdateTo(BankPeriodsOperations ent)
        {
            ent.BankDC = BankDC;
            ent.Date = Date;
            ent.CrsDC = CrsDC;
            ent.SummaStart = SummaStart;
            ent.SummaIn = SummaIn;
            ent.SummaOut = SummaOut;
            ent.SummaEnd = SummaEnd;
        }
    }
}