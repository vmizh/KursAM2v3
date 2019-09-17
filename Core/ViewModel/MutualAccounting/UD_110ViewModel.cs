using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once InconsistentNaming
namespace Core.ViewModel.MutualAccounting
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class UD_110ViewModel : RSViewModelBase, IEntity<UD_110>
    {
        private UD_110 myEntity;

        public UD_110ViewModel()
        {
            Entity = new UD_110 {Id = Guid.NewGuid()};
        }

        public UD_110ViewModel(UD_110 entity)
        {
            Entity = entity ?? DefaultValue();
            UpdateFrom(Entity);
        }

        public bool IsDelete
        {
            get => Entity.IsDelete;
            set
            {
                if (Entity.IsDelete == value) return;
                Entity.IsDelete = value;
                RaisePropertyChanged();
            }
        }
        public UD_110 Entity
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

        public List<UD_110> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public UD_110 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public UD_110 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(UD_110 doc)
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

        public void UpdateFrom(UD_110 ent)
        {
            Id = ent.Id;
            Name = ent.Name;
            Note = ent.Note;
            IsDelete = ent.IsDelete;
            //SD_110 = ent.SD_110;
        }

        public void UpdateTo(UD_110 ent)
        {
            ent.Id = Id;
            ent.Name = Name;
            ent.Note = Note;
            ent.IsDelete = IsDelete;
            //ent.SD_110 = SD_110;
        }

        public UD_110 DefaultValue()
        {
            return new UD_110();
        }
    }
}