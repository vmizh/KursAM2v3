using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class OffBalanceSheetChargesTypeViewModel : RSViewModelBase, IEntity<OffBalanceSheetChargesType>
    {
        private OffBalanceSheetChargesType myEntity;

        public OffBalanceSheetChargesTypeViewModel()
        {
            Entity = new OffBalanceSheetChargesType {Id = Guid.NewGuid()};
        }

        public OffBalanceSheetChargesTypeViewModel(OffBalanceSheetChargesType entity)
        {
            Entity = entity ?? new OffBalanceSheetChargesType {Id = Guid.NewGuid()};
        }

        public OffBalanceSheetChargesType Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
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
        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
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
        public bool IsAccessRight { get; set; }

        public List<OffBalanceSheetChargesType> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetChargesType Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetChargesType Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(OffBalanceSheetChargesType doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(OffBalanceSheetChargesType ent)
        {
            Id = ent.Id;
            Name = ent.Name;
            Note = ent.Note;
        }

        public void UpdateTo(OffBalanceSheetChargesType ent)
        {
            ent.Id = Id;
            ent.Name = Name;
            ent.Note = Note;
        }
    }
}