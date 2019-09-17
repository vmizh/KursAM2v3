using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
    [MetadataType(typeof(CountriesLayoutData_FluentAPI))]
    public class CountriesViewModel : RSViewModelBase, IEntity<Countries>
    {
        private Countries myEntity;

        public CountriesViewModel()
        {
            Entity = new Countries {Id = Guid.NewGuid()};
        }

        public CountriesViewModel(Countries entity)
        {
            Entity = entity ?? new Countries {Id = Guid.NewGuid()};
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
        public bool Deleted
        {
            get => Entity.Deleted;
            set
            {
                if (Entity.Deleted == value) return;
                Entity.Deleted = value;
                RaisePropertyChanged();
            }
        }
        [MaxLength(2)]
        public string Sign2
        {
            get => Entity.Sign2;
            set
            {
                if (Entity.Sign2 == value) return;
                Entity.Sign2 = value;
                RaisePropertyChanged();
            }
        }
        [MaxLength(3)]
        public string Sign3
        {
            get => Entity.Sign3;
            set
            {
                if (Entity.Sign3 == value) return;
                Entity.Sign3 = value;
                RaisePropertyChanged();
            }
        }
        public decimal Iso
        {
            get => Entity.Iso;
            set
            {
                if (Entity.Iso == value) return;
                Entity.Iso = value;
                RaisePropertyChanged();
            }
        }
        public string ForeignName
        {
            get => Entity.ForeignName;
            set
            {
                if (Entity.ForeignName == value) return;
                Entity.ForeignName = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<Countries> Countries { get; set; }
        public Countries Entity
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

        public List<Countries> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public Countries Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(Countries doc)
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

        public void UpdateFrom(Countries ent)
        {
            Id = ent.Id;
            Deleted = ent.Deleted;
            Note = ent.Note;
            Sign2 = ent.Sign2;
            Sign3 = ent.Sign3;
            Iso = ent.Iso;
            Name = ent.Name;
            ForeignName = ent.ForeignName;
        }

        public void UpdateTo(Countries ent)
        {
            ent.Id = Id;
            ent.Deleted = Deleted;
            ent.Note = Note;
            ent.Sign2 = Sign2;
            ent.Sign3 = Sign3;
            ent.Iso = Iso;
            ent.Name = Name;
            ent.ForeignName = ForeignName;
        }

        public Countries DefaultValue()
        {
            throw new NotImplementedException();
        }

        public virtual Countries Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual Countries Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public Countries Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    public static class CountriesLayoutData_FluentAPI
    {
        public static void BuildMetadata(MetadataBuilder<CountriesViewModel> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.LoadCondition).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.Deleted).NotAutoGenerated();
            builder.Property(_ => _.IsAccessRight).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.Code).NotAutoGenerated();
            
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.ForeignName).AutoGenerated().DisplayName("Междунар. наименование");
            builder.Property(_ => _.Iso).AutoGenerated().DisplayName("Iso");
            builder.Property(_ => _.Sign2).AutoGenerated().DisplayName("Код 2");
            builder.Property(_ => _.Sign3).AutoGenerated().DisplayName("Код 3");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        }
    }
}