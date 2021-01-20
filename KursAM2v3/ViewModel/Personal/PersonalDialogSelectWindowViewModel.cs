using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Personal
{
    public class PersonalDialogSelectWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<decimal> myExistEmploeesDC;

        private ObservableCollection<PersonaDialogSelect> myEmploeeCollection =
            new ObservableCollection<PersonaDialogSelect>();

        private ObservableCollection<PersonaDialogSelect> mySelectedCollection =
            new ObservableCollection<PersonaDialogSelect>();

        public PersonalDialogSelectWindowViewModel()
        {
            myExistEmploeesDC = new List<decimal>();
        }

        public PersonalDialogSelectWindowViewModel(List<decimal> exists)
        {
            myExistEmploeesDC = exists;
        }

        public ObservableCollection<PersonaDialogSelect> EmployeeSelected { set; get; } =
            new ObservableCollection<PersonaDialogSelect>();

        public ObservableCollection<PersonaDialogSelect> SelectedItems { set; get; } =
            new ObservableCollection<PersonaDialogSelect>();

        public ObservableCollection<PersonaDialogSelect> EmploeeCollection
        {
            set
            {
                myEmploeeCollection = value;
                RaisePropertyChanged();
            }
            get => myEmploeeCollection;
        }

        public ObservableCollection<PersonaDialogSelect> SelectedCollection
        {
            set
            {
                mySelectedCollection = value;
                RaisePropertyChanged();
            }
            get => mySelectedCollection;
        }

        public override void RefreshData(object obj)
        {
            EmploeeCollection.Clear();
            SelectedCollection.Clear();
            foreach (var s in GlobalOptions.GetEntities().SD_2.Include(_ => _.SD_301))
            {
                if (myExistEmploeesDC.Any(_ => _ == s.DOC_CODE)) continue;
                EmploeeCollection.Add(new PersonaDialogSelect
                {
                    Persona = new Employee(s),
                    DocCode = s.DOC_CODE
                });
            }

            RaisePropertyChanged(nameof(EmploeeCollection));
            RaisePropertyChanged(nameof(SelectedCollection));
        }

        public override void SaveData(object obj)
        {
            throw new NotImplementedException();
        }

        public void AddToSelected(PersonaDialogSelect item)
        {
            if (EmployeeSelected == null)
            {
                if (item != null)
                {
                    if (!SelectedCollection.Contains(item))
                        SelectedCollection.Add(item);
                    RaisePropertyChanged(nameof(SelectedCollection));
                    return;
                }

                return;
            }

            foreach (var r in EmployeeSelected)
                if (!SelectedCollection.Contains(r))
                    SelectedCollection.Add(r);
            RaisePropertyChanged(nameof(SelectedCollection));
        }

        public void RemoveFromSelected(PersonaDialogSelect item)
        {
            if (SelectedItems == null)
            {
                if (item != null)
                {
                    if (!SelectedCollection.Contains(item))
                        SelectedCollection.Remove(item);
                    RaisePropertyChanged(nameof(SelectedCollection));
                }
            }
            else
            {
                var tempList = new List<PersonaDialogSelect>(SelectedItems);
                foreach (var s in tempList)
                    if (SelectedCollection.Contains(s))
                        SelectedCollection.Remove(s);
                RaisePropertyChanged(nameof(SelectedCollection));
            }
        }
    }
}