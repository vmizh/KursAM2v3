using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using KursDomain.Documents.Employee;
using KursDomain.ICommon;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaDialogSelect : RSViewModelBase
    {
        private Employee _Persona { set; get; }

        //[GridColumnView("Начисление/удержание", SettingsType.Default)]
        public Employee Persona
        {
            get => _Persona;
            set
            {
                if (_Persona != null && _Persona.Equals(value)) return;
                _Persona = value;
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
                RaisePropertyChanged();
            }
        }

        [GridColumnView("Имя", SettingsType.Default)]
        public new string Name => _Persona.Name;

        [GridColumnView("Таб. №", SettingsType.Default)]
        public int TabelNumber => _Persona.TabelNumber;

        [GridColumnView("Валюта", SettingsType.Default)]
        public string Crs => _Persona.Currency.Name;

        [GridColumnView("Справочно", SettingsType.Default)]
        public string ExtNotes => _Persona.StatusNotes;

        public override decimal DocCode { get; set; }
    }
}
