using System;
using Core.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    public class Persona : Employee
    {
        public Persona(SD_2 entity) : base(entity)
        {
        }

        public Persona()
        {
            Entity = new SD_2();
        }
        public override Guid Id
        {
            get => Entity.ID == null ? Guid.NewGuid() : Guid.Parse(Entity.ID);
            set
            {
                if (!string.IsNullOrEmpty(Entity.ID) && Guid.Parse(Entity.ID) != value)
                {
                    Entity.ID = value.ToString();
                    RaisePropertyChanged();
                }
            }
        }
        public override Currency Currency
        {
            get => Entity.crs_dc != null ? MainReferences.Currencies[Entity.crs_dc.Value] : null;
            set
            {
                if (value != null)
                {
                    Entity.crs_dc = value.DocCode;
                    RaisePropertyChanged();
                }
            }
        }
        public override int TabelNumber
        {
            get => Entity.TABELNUMBER;
            set
            {
                if (Entity.TABELNUMBER == value) return;
                Entity.TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }
        public override string Name => $"{LastName} {FirstName} {SecondName}";
        public override string FirstName
        {
            get => Entity.NAME_FIRST;
            set
            {
                if (Entity.NAME_FIRST == value) return;
                Entity.NAME_FIRST = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Name));
            }
        }
        public override string LastName
        {
            get => Entity.NAME_LAST;
            set
            {
                if (Entity.NAME_LAST == value) return;
                Entity.NAME_LAST = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Name));
            }
        }
        public override string SecondName
        {
            get => Entity.NAME_SECOND;
            set
            {
                if (Entity.NAME_SECOND == value) return;
                Entity.NAME_SECOND = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Name));
            }
        }
        public DateTime? DateChanged
        {
            get => CHANGE_DATE;
            set
            {
                if (Entity.CHANGE_DATE == value) return;
                Entity.CHANGE_DATE = value;
                RaisePropertyChanged();
            }
        }
        public override bool IsDeleted
        {
            get => (Entity.DELETED ?? 0) != 0;
            set
            {
                if (Entity.DELETED == (value ? 1 : 0)) return;
                Entity.DELETED = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        public override string StatusNotes
        {
            get => Entity.STATUS_NOTES;
            set
            {
                if (Entity.STATUS_NOTES == value) return;
                Entity.STATUS_NOTES = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Name));
            }
        }
    }
}