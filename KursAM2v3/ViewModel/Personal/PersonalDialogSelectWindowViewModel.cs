using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core.ViewModel.Base;
using KursDomain;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    public class PersonalDialogSelectWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<decimal> myExistEmploeesDC;

        private ObservableCollection<Employee> myEmploeeCollection =
            new ObservableCollection<Employee>();

        private ObservableCollection<Employee> mySelectedCollection =
            new ObservableCollection<Employee>();

        public PersonalDialogSelectWindowViewModel()
        {
            myExistEmploeesDC = new List<decimal>();
        }

        public PersonalDialogSelectWindowViewModel(List<decimal> exists)
        {
            myExistEmploeesDC = exists;
        }

        public ObservableCollection<Employee> EmployeeSelected { set; get; } =
            new ObservableCollection<Employee>();

        public ObservableCollection<Employee> SelectedItems { set; get; } =
            new ObservableCollection<Employee>();

        public ObservableCollection<Employee> EmploeeCollection
        {
            set
            {
                myEmploeeCollection = value;
                RaisePropertyChanged();
            }
            get => myEmploeeCollection;
        }

        public ObservableCollection<Employee> SelectedCollection
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
                var emp = new Employee();
                emp.LoadFromEntity(s, GlobalOptions.ReferencesCache);
                EmploeeCollection.Add(emp);
            }

            RaisePropertyChanged(nameof(EmploeeCollection));
            RaisePropertyChanged(nameof(SelectedCollection));
        }

        public override void SaveData(object obj)
        {
            throw new NotImplementedException();
        }

        public void AddToSelected(Employee item)
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

        public void RemoveFromSelected(Employee item)
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
                var tempList = new List<Employee>(SelectedItems);
                foreach (var s in tempList)
                    if (SelectedCollection.Contains(s))
                        SelectedCollection.Remove(s);
                RaisePropertyChanged(nameof(SelectedCollection));
            }
        }
    }
}
