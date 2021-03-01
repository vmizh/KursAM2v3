using System;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    public class UsersViewModel : RSViewModelBase
    {
        public UsersViewModel(Users usersEntity)
        {
            Entity = usersEntity;
        }
        public UsersViewModel() { }

        #region Properties

        [Display(AutoGenerateField = false)] 
        public Users Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value)
                    return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Имя авторизации")]
        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value)
                    return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Полное имя")]
        public string FullName
        {
            get => Entity.FullName;
            set
            {
                if (Entity.FullName == value)
                    return;
                Entity.FullName = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Примечание")]
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value)
                    return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Тема приложения")]
        public string ThemeName
        {
            get => Entity.ThemeName;
            set
            {
                if (Entity.ThemeName == value)
                    return;
                Entity.ThemeName = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Администратор")]
        public bool IsAdmin
        {
            get => Entity.IsAdmin;
            set
            {
                if (Entity.IsAdmin == value)
                    return;
                Entity.IsAdmin = value;
            }
        }

        [Display(Name = "Тестировщик")]
        public bool IsTester
        {
            get => Entity.IsTester;
            set
            {
                if (Entity.IsTester == value)
                    return;
                Entity.IsTester = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Удален")]
        public bool IsDeleted
        {
            get => Entity.IsDeleted;
            set
            {
                if (Entity.IsDeleted == value)
                    return;
                Entity.IsDeleted = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public byte[] Avatar
        {
            get => Entity.Avatar;
            set
            {
                if (Entity.Avatar == value)
                    return;
                Entity.Avatar = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}