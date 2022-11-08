using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Data;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursDomain.Documents.Periods;
using KursDomain.Documents.Systems;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Period
{
    public class PeriodGroupsReferenceViewModel : RSWindowViewModelBase
    {
        private readonly PeriodGroupsRefenceManager Manager = new PeriodGroupsRefenceManager();

        public PeriodGroupsReferenceViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.TableEditRightBar(this);
            RefreshData(null);
        }

        #region property

        public ObservableCollection<PERIOD_CLOSEDViewModel> PeriodClosed { set; get; } =
            new ObservableCollection<PERIOD_CLOSEDViewModel>();

        public ObservableCollection<PERIOD_GROUPSViewModel> PeriodGroups { set; get; } =
            new ObservableCollection<PERIOD_GROUPSViewModel>();

        public ObservableCollection<PERIOD_GROUPSViewModel> PeriodGroupsDelete { set; get; } =
            new ObservableCollection<PERIOD_GROUPSViewModel>();

        public ObservableCollection<PERIOD_GROUPS_USERSViewModel> PeriodUsers { set; get; } =
            new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();

        public ObservableCollection<PERIOD_GROUPS_USERSViewModel> BasePeriodUsers { set; get; } =
            new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();

        public ObservableCollection<PERIOD_GROUPS_USERSViewModel> NewPeriodUsers { set; get; } =
            new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();

        public ObservableCollection<PERIOD_GROUPS_USERSViewModel> DeletedPeriodUsers { set; get; } =
            new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();

        public ObservableCollection<EXT_USERSViewModel> AllUsers { set; get; } =
            new ObservableCollection<EXT_USERSViewModel>();

        public ObservableCollection<PeriodGroupsExludeViewModel> PeriodExclude { set; get; } =
            new ObservableCollection<PeriodGroupsExludeViewModel>();

        public ObservableCollection<PeriodGroupsExludeViewModel> PeriodExcludeDeleteCollection { set; get; } =
            new ObservableCollection<PeriodGroupsExludeViewModel>();

        public ObservableCollection<PeriodGroupsExludeViewModel> BasePeriodExclude { set; get; } =
            new ObservableCollection<PeriodGroupsExludeViewModel>();

        private new Window myForm;

        public override Window Form
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myForm == value) return;
                myForm = value;
                RaisePropertyChanged();
            }
            get => myForm;
        }

        public override bool IsCanSaveData
        {
            get => NewPeriodUsers.Count > 0 || DeletedPeriodUsers.Count > 0 ||
                   PeriodGroups.FirstOrDefault(_ => _.State == RowStatus.NewRow) != null ||
                   PeriodGroups.FirstOrDefault(_ => _.State == RowStatus.Edited) != null ||
                   PeriodGroupsDelete.Count > 0;
            set => base.IsCanSaveData = value;
        }

        private PERIOD_GROUPSViewModel myCurrentGroup;

        public PERIOD_GROUPSViewModel CurrentGroup
        {
            set
            {
                if (Equals(myCurrentGroup,value)) return;
                myCurrentGroup = value;
                GetUsers();
                RaisePropertyChanged();
            }
            get => myCurrentGroup;
        }

        private PERIOD_GROUPS_USERSViewModel myCurrentPeriodUsers;

        public PERIOD_GROUPS_USERSViewModel CurrentPeriodUsers
        {
            set
            {
                if (Equals(myCurrentPeriodUsers,value)) return;
                myCurrentPeriodUsers = value;
                RaisePropertyChanged();
            }
            get => myCurrentPeriodUsers;
        }

        private PERIOD_CLOSEDViewModel myCurrentPeriodClosed;

        public PERIOD_CLOSEDViewModel CurrentPeriodClosed
        {
            set
            {
                if (Equals(myCurrentPeriodClosed,value)) return;
                myCurrentPeriodClosed = value;
                GetPeriodExclude();
                RaisePropertyChanged();
            }
            get => myCurrentPeriodClosed;
        }

        private PeriodGroupsExludeViewModel myCurrentPeriodExlude;

        public PeriodGroupsExludeViewModel CurrentPeriodExlude
        {
            set
            {
                if (Equals(myCurrentPeriodExlude,value)) return;
                myCurrentPeriodExlude = value;
                RaisePropertyChanged();
            }
            get => myCurrentPeriodExlude;
        }

        #endregion

        #region command

        public override void RefreshData(object obj)
        {
            PeriodGroups.Clear();
            PeriodClosed = new ObservableCollection<PERIOD_CLOSEDViewModel>();
            AllUsers = new ObservableCollection<EXT_USERSViewModel>();
            BasePeriodUsers = new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();
            NewPeriodUsers = new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();
            PeriodUsers.Clear();
            DeletedPeriodUsers = new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();
            PeriodExclude = new ObservableCollection<PeriodGroupsExludeViewModel>();
            PeriodClosed = Manager.GetPeriods();
            PeriodGroups = Manager.GetGroups();
            BasePeriodUsers = Manager.GetGroupsUsers();
            AllUsers = Manager.GetAllUsers();
            BasePeriodExclude = Manager.GetAllExcludePeriods();
            CurrentGroup = PeriodGroups.First();
            RaisePropertyChanged(nameof(PeriodUsers));
            RaisePropertyChanged(nameof(PeriodGroups));
        }

        public ICommand NewPeriodExcludeCommand
        {
            get { return new Command(NewPeriodExclude, _ => CurrentPeriodExlude != null); }
        }

        public ICommand SetNewClosedDateCommand
        {
            get { return new Command(SetNewClosedDate, _ => CurrentPeriodClosed != null); }
        }

        public ICommand DeleteGroupCommand
        {
            get { return new Command(DeleteGroup, _ => true); }
        }

        private void DeleteGroup(object obj)
        {
            if (CurrentGroup == null) return;
            PeriodGroupsDelete.Add(CurrentGroup);
            PeriodGroups.Remove(CurrentGroup);
            RaisePropertyChanged(nameof(PeriodGroups));
        }

        public ICommand AddNewGroupCommand
        {
            get { return new Command(AddNewGroup, _ => true); }
        }

        public ICommand DeletePeriodExcludeCommand
        {
            get { return new Command(DeletePeriodExclude, _ => true); }
        }

        private void DeletePeriodExclude(object obj)
        {
            if (CurrentPeriodExlude == null) return;
            PeriodExcludeDeleteCollection.Add(CurrentPeriodExlude);
            PeriodExclude.Remove(CurrentPeriodExlude);
            Manager.DeletedPeriodExclude(PeriodExcludeDeleteCollection);
            BasePeriodExclude = Manager.GetAllExcludePeriods();
        }

        public ICommand AddNewPeriodExcludeCommand
        {
            get { return new Command(AddNewPeriodExclude, _ => true); }
        }

        private void AddNewPeriodExclude(object obj)
        {
            if (CurrentPeriodClosed == null) return;
            var k = StandartDialogs.SelectPeriodGroups();
            if (k != null)
                foreach (var item in k)
                {
                    if (PeriodExclude.Any(_ => _.USER_GROUP_ID == item.Id)) continue;
                    PeriodExclude.Add(new PeriodGroupsExludeViewModel
                    {
                        GroupName = item.Name,
                        Id = Guid.NewGuid(),
                        CLOSED_ID = CurrentPeriodClosed.Id,
                        DateExclude = CurrentPeriodClosed.DateClosed,
                        DateFrom = DateTime.Today.AddDays(1),
                        USER_GROUP_ID = item.Id,
                        State = RowStatus.Edited
                    });
                }

            Manager.SaveExclude(PeriodExclude);
            BasePeriodExclude.Clear();
            BasePeriodExclude = Manager.GetAllExcludePeriods();
        }

        private void AddNewGroup(object obj)
        {
            PeriodGroups.Add(new PERIOD_GROUPSViewModel
            {
                Id = Guid.NewGuid(),
                Name = "Новая группа",
                State = RowStatus.NewRow
            });
            RaisePropertyChanged(nameof(PeriodGroups));
        }

        private void SetNewClosedDate(object obj)
        {
            if (CurrentPeriodClosed == null) return;
            GetPeriodExclude();
            foreach (var item in PeriodClosed)
            {
                if (item.State == RowStatus.Edited)
                    Manager.SavePeriodClosed(item);
                item.State = RowStatus.NotEdited;
            }
        }

        private void NewPeriodExclude(object obj)
        {
            if (CurrentPeriodClosed == null || CurrentPeriodClosed == null) return;
            foreach (var item in PeriodExclude)
                if (item.State == RowStatus.Edited)
                {
                    Manager.SavePeriodExlude(item);
                    BasePeriodExclude.Clear();
                    BasePeriodExclude = Manager.GetAllExcludePeriods();
                    item.State = RowStatus.NotEdited;
                }
        }

        private void GetPeriodExclude()
        {
            if (CurrentPeriodClosed == null) return;
            PeriodExclude.Clear();
            if (BasePeriodExclude.Any())
                foreach (var item in BasePeriodExclude)
                    if (item.CLOSED_ID == CurrentPeriodClosed.Id)
                    {
                        var newItem = new PeriodGroupsExludeViewModel
                        {
                            GroupName = PeriodGroups.FirstOrDefault(_ => _.Id == item.USER_GROUP_ID)?.Name,
                            USER_GROUP_ID = item.USER_GROUP_ID,
                            CLOSED_ID = item.CLOSED_ID,
                            Id = item.Id,
                            DateExclude = item.DateExclude,
                            DateFrom = item.DateFrom,
                            State = RowStatus.NotEdited
                        };
                        PeriodExclude.Add(newItem);
                    }

            RaisePropertyChanged(nameof(PeriodExclude));
        }

        public void GetUsers()
        {
            if (CurrentGroup == null) return;
            PeriodUsers = new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();
            foreach (var item in NewPeriodUsers)
                if (item.GROUP_ID == CurrentGroup.Id)
                    PeriodUsers.Add(item);
            foreach (var i in BasePeriodUsers)
                if (i.GROUP_ID == CurrentGroup.Id)
                    PeriodUsers.Add(i);
            RaisePropertyChanged(nameof(PeriodUsers));
        }

        public ICommand AddNewUserCommand
        {
            get { return new Command(AddNewUser, _ => true); }
        }

        public ICommand DeleteUserCommand
        {
            get { return new Command(DeleteUser, _ => CurrentPeriodUsers != null); }
        }

        private void AddNewUser(object obj)
        {
            var k = StandartDialogs.SelectUsers();
            if (k != null)
                foreach (var item in k)
                {
                    if (PeriodUsers.Any(_ => _.USER_ID == item.USR_ID)) continue;
                    NewPeriodUsers.Add(new PERIOD_GROUPS_USERSViewModel
                    {
                        Id = Guid.NewGuid(),
                        USER_ID = item.USR_ID,
                        GROUP_ID = CurrentGroup.Id,
                        EXT_USERS = new EXT_USERS(),
                        Name = AllUsers.FirstOrDefault(_ => _.USR_ID == item.USR_ID)?.USR_NICKNAME
                    });
                }

            GetUsers();
        }

        private void DeleteUser(object obj)
        {
            var delN = NewPeriodUsers.FirstOrDefault(_ => _.Id == CurrentPeriodUsers.Id);
            if (delN != null)
                NewPeriodUsers.Remove(CurrentPeriodUsers);
            var DelU = PeriodUsers.FirstOrDefault(_ => _.Id == CurrentPeriodUsers.Id);
            if (DelU != null)
            {
                DeletedPeriodUsers.Add(CurrentPeriodUsers);
                PeriodUsers.Remove(CurrentPeriodUsers);
            }
        }

        public override void SaveData(object data)
        {
            Manager.SaveGroup(PeriodGroups);
            Manager.SaveGroupUsers(NewPeriodUsers, DeletedPeriodUsers);
            Manager.DeleteGroup(PeriodGroupsDelete);
            PeriodGroupsDelete.Clear();
            foreach (var i in PeriodGroups)
                i.State = RowStatus.NotEdited;
            RefreshData(null);
        }

        public override void CloseWindow(object form)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        base.CloseWindow(form);
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            else
            {
                Form.Close();
            }
        }

        #endregion
    }
}
