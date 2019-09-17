using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;

namespace PeriodManager.ViewModel
{
    public class SetPeriodCloseForGroupViewModel : KursViewModelBase
    {
        private DateTime myDateFrom;
        private DateTime myDateWhile;
        private PeriodGroupViewModel mySlectedUserGroup;

        public SetPeriodCloseForGroupViewModel()
        {
            LayoutName = "KursAM2.SetPeriodCloseForGroupViewModel.xml";
            Groups = new ObservableCollection<PeriodGroupViewModel>();
            DocTypes = new ObservableCollection<PeriodClosedTypeViewModel>();
            DateFrom = DateTime.Today.AddDays(-30);
            DateWhile = DateTime.Today;
            LoadData();
        }

        public PeriodGroupViewModel SelectedUserGroup
        {
            get => mySlectedUserGroup;
            set
            {
                if (value == mySlectedUserGroup) return;
                mySlectedUserGroup = value;
                OnPropertyChanged(nameof(SelectedUserGroup));
            }
        }
        public DateTime DateFrom
        {
            get => myDateFrom;
            set
            {
                if (value == myDateFrom) return;
                myDateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
            }
        }
        public DateTime DateWhile
        {
            get => myDateWhile;
            set
            {
                if (value == myDateWhile) return;
                myDateWhile = value;
                OnPropertyChanged(nameof(DateWhile));
            }
        }
        public ObservableCollection<PeriodClosedTypeViewModel> DocTypes { get; set; }
        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<PeriodGroupViewModel> Groups { get; set; }

        private void Save()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                try
                {
                    using (var tnx = new TransactionScope())
                    {
                        var cc =
                            ent.PERIOD_CLOSED_EXCLUDE.ToList()
                                .Where(_ => _.USER_GROUP_ID == SelectedUserGroup.Id)
                                .ToList();
                        if (cc.Count > 0)
                            foreach (var c in cc)
                                if(c != null)
                                    ent.PERIOD_CLOSED_EXCLUDE.Remove(c);
                        ent.SaveChanges();
                        foreach (var d in DocTypes.Where(_ => _.IsSelected))
                        {
                            var cls = ent.PERIOD_CLOSED.FirstOrDefault(_ => _.TYPE_ID == d.Id);
                            if (cls == null) continue;
                            ent.PERIOD_CLOSED_EXCLUDE.Add(new PERIOD_CLOSED_EXCLUDE
                            {
                                ID = Guid.NewGuid(),
                                CLOSED_ID = cls.ID,
                                USER_GROUP_ID = SelectedUserGroup.Id,
                                DateFrom = DateFrom,
                                DateExclude = DateWhile
                            });
                        }
                        ent.SaveChanges();
                        tnx.Complete();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void LoadData()
        {
            foreach (var c in GlobalOptions.GetEntities().CLOSED_DOC_TYPE.ToList())
                DocTypes.Add(new PeriodClosedTypeViewModel
                {
                    Id = c.ID,
                    Name = c.NAME,
                    IsSelected = false
                });
            foreach (var d in GlobalOptions.GetEntities().PERIOD_GROUPS.Include("PERIOD_GROUPS_USERS")
                .Include("PERIOD_GROUPS_USERS.EXT_USERS").ToList())
            {
                var g = new PeriodGroupViewModel
                {
                    Id = d.ID,
                    Name = d.NAME,
                    State = RowStatus.NotEdited,
                    Users = new ObservableCollection<PeriodGroupUserViewModel>()
                };
                foreach (var r in d.PERIOD_GROUPS_USERS.ToList())
                    g.Users.Add(new PeriodGroupUserViewModel
                    {
                        Id = r.ID,
                        User =
                            new UserViewModel
                            {
                                UserId = r.USER_ID,
                                Name = r.EXT_USERS.USR_NICKNAME,
                                FullName = r.EXT_USERS.USR_FULLNAME
                            },
                        State = RowStatus.NotEdited
                    });
                Groups.Add(g);
            }
        }

        #region Command

        public ICommand OKCommand
        {
            get { return new Command(OkSelect, param => true); }
        }

        private void OkSelect(object obj)
        {
            if (SelectedUserGroup == null)
            {
                MessageBox.Show("Не выбрана группа пользователей.");
                return;
            }
            var win = obj as Window;
            if (win == null) return;
            Save();
            win.DialogResult = true;
        }

        #endregion
    }
}