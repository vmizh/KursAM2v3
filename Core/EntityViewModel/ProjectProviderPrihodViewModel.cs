using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class ProjectProviderPrihodViewModel : RSViewModelBase, IEntity<ProjectProviderPrihod>
    {
        private ProjectProviderPrihod myEntity;

        public ProjectProviderPrihodViewModel()
        {
            Entity = new ProjectProviderPrihod {Id = Guid.NewGuid()};
        }

        public ProjectProviderPrihodViewModel(ProjectProviderPrihod entity)
        {
            Entity = entity ?? new ProjectProviderPrihod {Id = Guid.NewGuid()};
        }

        public ProjectProviderPrihod Entity
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

        public List<ProjectProviderPrihod> LoadList()
        {
            return null;
        }

        public bool IsAccessRight { get; set; }

        public virtual ProjectProviderPrihod Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual ProjectProviderPrihod Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(ProjectProviderPrihod doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(ProjectProviderPrihod ent)
        {
            Id = ent.Id;
            RowId = ent.RowId;
            Quantity = ent.Quantity;
            Note = ent.Note;
        }

        public void UpdateTo(ProjectProviderPrihod ent)
        {
            ent.Id = Id;
            ent.RowId = RowId;
            ent.Quantity = Quantity;
            ent.Note = Note;
        }
    }
}