using System;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.Systems;

namespace Core.EntityViewModel
{
    public class UserMenuFavoriteViewModel : RSViewModelBase, IEntity<UserMenuFavorites>
    {
        private DataSourcesViewModel myDataSource;

        private KursMenuItemViewModel myMenuItem;


        private UsersViewModel myUser;

        public UserMenuFavoriteViewModel()
        {
            Entity = DefaultValue();
        }

        public UserMenuFavoriteViewModel(UserMenuFavorites entity)
        {
            if (entity == null)
            {
                Entity = DefaultValue();
            }
            else
            {
                Entity = entity;
                MenuItem = new KursMenuItemViewModel(entity.KursMenuItem);
                DataSource = new DataSourcesViewModel(entity.DataSources);
                User = new UsersViewModel(entity.Users);
            }
            
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.DbId == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Документ")]
        public KursMenuItemViewModel MenuItem
        {
            get => myMenuItem;
            set
            {
                if (myMenuItem == value) return;
                myMenuItem = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "База данных")]
        public DataSourcesViewModel DataSource
        {
            get => myDataSource;
            set
            {
                if (myDataSource == value) return;
                myDataSource = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Пользователь")]
        public UsersViewModel User
        {
            get => myUser;
            set
            {
                if (myUser == value) return;
                myUser = value;
                RaisePropertyChanged();
            }
        }

        public override string Name => MenuItem?.Name;


        [Display(AutoGenerateField = false)] public UserMenuFavorites Entity { get; set; }

        public UserMenuFavorites DefaultValue()
        {
            return new UserMenuFavorites { Id = Guid.NewGuid() };
        }
    }
}
