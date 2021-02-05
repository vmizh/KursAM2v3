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
    class UserRolesViewModel : RSViewModelBase
    {
        
        #region Constructors
            public UserRolesViewModel() { }

            public UserRolesViewModel(UserRoles userRolesEntity)
            {
                Entity = userRolesEntity;
            }

        #endregion

        #region Fields

        #endregion



        #region Properties

        [Display(AutoGenerateField = false)]
        public UserRoles Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.id;
            set
            {
                if(Entity.id == null)
                    return;
                Entity.id = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Роль")]
        public override string Name
        {
            get => Entity.Name;
            set
            {
                if(Entity.Name == null)
                    return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }
        [Display(Name = "Описание роли")]
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if(Entity.Note == null)
                    return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }
        

        #endregion

    }
}
