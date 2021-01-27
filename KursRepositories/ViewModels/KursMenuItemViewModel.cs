using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    public class KursMenuItemViewModel : RSViewModelBase
    {
        #region Properties


        [Display(AutoGenerateField = false)] public KursMenuItem Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public int Id
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
        public string Name
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
        public string Note
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
        public string Code
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
