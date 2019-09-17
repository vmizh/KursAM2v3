using System.Collections.Generic;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;

namespace KursAM2.ViewModel.Personal
{
    public class PREmpUserRight : RSViewModelData
    {
        public PREmpUserRight()
        {
            Employee = new List<Employee>();
        }

        [GridColumnView("Пользователь", SettingsType.Default, ReadOnly = true)]
        public string UserName { set; get; }

        [GridColumnView("Сотрудник", SettingsType.Default, ReadOnly = true)]
        public List<Employee> Employee { set; get; }
    }
}