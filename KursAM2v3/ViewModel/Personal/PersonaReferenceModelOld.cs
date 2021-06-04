using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Employee;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaReferenceModelOld : RSViewModelBase
    {
        private readonly Employee _Persona;

        public PersonaReferenceModelOld(Employee pers)
        {
            _Persona = pers;
        }

        public string NameFirst
        {
            get => _Persona.NameFirst;
            set
            {
                if (_Persona.NameFirst == value) return;
                _Persona.NameFirst = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public string NameLast
        {
            get => _Persona.NameLast;
            set
            {
                if (_Persona.NameLast == value) return;
                _Persona.NameLast = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public string NameSecond
        {
            get => _Persona.NameSecond;
            set
            {
                if (_Persona.NameSecond == value) return;
                _Persona.NameSecond = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public int TabelNumber
        {
            get => _Persona.TabelNumber;
            set
            {
                if (_Persona.TabelNumber == value) return;
                _Persona.TabelNumber = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public bool IsDeleted
        {
            get => _Persona.IsDeleted;
            set
            {
                if (_Persona.IsDeleted == value) return;
                _Persona.IsDeleted = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public Currency Crs
        {
            get => _Persona.Currency;
            set
            {
                if (Equals(_Persona.Currency, value)) return;
                _Persona.Currency = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public byte[] Photo
        {
            get => _Persona.Photo;
            set
            {
                if (_Persona.Photo == value) return;
                _Persona.Photo = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public string StatusNotes
        {
            get => _Persona.StatusNotes;
            set
            {
                if (_Persona.StatusNotes == value) return;
                _Persona.StatusNotes = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}