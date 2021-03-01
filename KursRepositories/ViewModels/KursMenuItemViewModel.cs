using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    public class KursMenuItemViewModel : RSViewModelBase
    {
        private int myIsSelectedItem;

        #region Properties


        [Display(AutoGenerateField = false)] 
        public KursMenuItem Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public new int Id
        {
            get => myIsSelectedItem;
            set
            {
                if (myIsSelectedItem == value)
                    return;
                myIsSelectedItem = value;
                RaisePropertyChanged();
            }
        }

        
        [Display(AutoGenerateField = false)]
        public int GroupId
        {
            get => Entity.GroupId;
            set
            {
                if (Entity.GroupId == value)
                    return;
                Entity.GroupId = value;
                RaisePropertyChanged();

            }
        }

        [Display(Name = "Наименование")]
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

        [DisplayName("Примечание")]
        [Display(AutoGenerateField = false)]
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

        [DisplayName("Ссылка на меню")]
        [Display(AutoGenerateField = false)]
        public int? OrderBy
        {
            get => Entity.OrderBy;
            set
            {
                if (Entity.OrderBy == value)
                    return;
                Entity.OrderBy = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public new string Code
        {
            get => Entity.Code;
            set
            {
                if (Entity.Code == value)
                    return;
                Entity.Code = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public byte[] Picture
        {
            get => Entity.Picture;
            set
            {
                if (Entity.Picture == value)
                    return;
                Entity.Picture = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public KursMenuItemViewModel(KursMenuItem entityKursMenuItem)
        {
            Entity = entityKursMenuItem;
        }
    }

    public class WrapKursMenuItemViewModel : KursMenuItemViewModel
    {
        private bool myIsSelectedItem;

        public bool IsSelectedItem
        {
            get => myIsSelectedItem;
            set
            {
                if (myIsSelectedItem == value)
                    return;
                myIsSelectedItem = value;
                RaisePropertyChanged();
            }
        }

        public WrapKursMenuItemViewModel(KursMenuItem entityKursMenuItem) : base(entityKursMenuItem)
        {
            IsSelectedItem = true;
        }
    }

    
}