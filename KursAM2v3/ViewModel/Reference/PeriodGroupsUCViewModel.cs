using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.KursReferences.UC;

namespace KursAM2.ViewModel.Reference
{
    internal class PeriodGroupsUCViewModel : RSWindowViewModelBase
    {
        public PeriodGroupsUCViewModel()
        {
            myDataUserControl = new PeriodGroupsUC();
            RefreshData(null);
            WindowName = "Группы пользователей";
        }

        #region properties

        public ObservableCollection<PERIOD_GROUPSViewModel> PeriodGroups { set; get; }
        public ObservableCollection<PERIOD_GROUPSViewModel> SelectedGroups { set; get; }
        public List<PERIOD_GROUPSViewModel> ListSelectedUsers { set; get; } = new List<PERIOD_GROUPSViewModel>();
        private PeriodGroupsUC myDataUserControl;

        public PeriodGroupsUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertiesChanged();
            }
            get => myDataUserControl;
        }

        private PERIOD_GROUPSViewModel myCurrentSelectedGroups;

        public PERIOD_GROUPSViewModel CurrentSelectedGroups
        {
            set
            {
                if (myCurrentSelectedGroups != null && myCurrentSelectedGroups.Equals(value)) return;
                myCurrentSelectedGroups = value;
                RaisePropertyChanged();
            }
            get => myCurrentSelectedGroups;
        }

        private PERIOD_GROUPSViewModel myCurrentGroup;

        public PERIOD_GROUPSViewModel CurrentGroup
        {
            set
            {
                if (myCurrentGroup != null && myCurrentGroup.Equals(value)) return;
                myCurrentGroup = value;
                RaisePropertyChanged();
            }
            get => myCurrentGroup;
        }

        #endregion

        #region command

        public override void RefreshData(object o)
        {
            PeriodGroups = new ObservableCollection<PERIOD_GROUPSViewModel>();
            SelectedGroups = new ObservableCollection<PERIOD_GROUPSViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.PERIOD_GROUPS.ToList();
                    foreach (var i in data)
                        PeriodGroups.Add(new PERIOD_GROUPSViewModel(i));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand AddGroupsCommand
        {
            get { return new Command(AddGroups, _ => CurrentGroup != null); }
        }

        public ICommand DeleteSelectedGroup
        {
            get { return new Command(DeleteSelected, _ => CurrentSelectedGroups != null); }
        }

        public void AddGroups(object obj)
        {
            if (SelectedGroups.Contains(CurrentGroup)) return;
            SelectedGroups.Add(CurrentGroup);
            ListSelectedUsers.Add(CurrentGroup);
        }

        public void DeleteSelected(object obj)
        {
            SelectedGroups.Remove(CurrentSelectedGroups);
            ListSelectedUsers.Remove(CurrentSelectedGroups);
        }

        #endregion
    }
}