using System;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class LastMenuUserSearchViewModel : RSViewModelBase, IEntity<LastMenuUserSearch>
    {
        #region Fields

        private DataSourcesViewModel myDataBase;
        private UsersViewModel myUser;
        private KursMenuItemViewModel myMenu;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public LastMenuUserSearch Entity { get; set; }

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

        public DataSourcesViewModel DataBase
        {
            get => myDataBase;
            set
            {
                if (myDataBase == value) return;
                myDataBase = value;
                if (myDataBase != null)
                    Entity.DbId = myDataBase.Id;
                RaisePropertyChanged();
            }
        }

        public UsersViewModel User
        {
            get => myUser;
            set
            {
                if (myUser == value) return;
                myUser = value;
                if (myUser != null)
                    Entity.DbId = myUser.Id;
                RaisePropertyChanged();
            }
        }

        public KursMenuItemViewModel Menu
        {
            get => myMenu;
            set
            {
                if (myMenu == value) return;
                myMenu = value;
                if (myMenu != null)
                    Entity.MenuId = myMenu.Id;
                RaisePropertyChanged();
            }
        }

        public DateTime LastOpen
        {
            get => Entity.LastOpen;
            set
            {
                if (Entity.LastOpen == value) return;
                Entity.LastOpen = value;
                RaisePropertyChanged();
            }
        }

        public int OpenCount
        {
            get => Entity.OpenCount;
            set
            {
                if (Entity.OpenCount == value) return;
                Entity.OpenCount = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public LastMenuUserSearchViewModel()
        {
            Entity = DefaultValue();
        }

        public LastMenuUserSearchViewModel(LastMenuUserSearch entity)
        {
            if (entity == null)
            {
                Entity = DefaultValue();
                return;
            }
            Entity = entity;
            DataBase = new DataSourcesViewModel(Entity.DataSources);
            User = new UsersViewModel(Entity.Users);
            Menu = new KursMenuItemViewModel(Entity.KursMenuItem);
        }

        #endregion

        #region Methods

        public LastMenuUserSearch DefaultValue()
        {
            return new LastMenuUserSearch()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion


    }
}