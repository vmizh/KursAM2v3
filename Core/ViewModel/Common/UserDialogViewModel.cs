using System.Collections.ObjectModel;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    public class UserDialogViewModel : KursViewModelBase
    {
        private UserViewModel mySelectedRow;
        private string myViewName;

        public UserDialogViewModel()
        {
            Documents = new ObservableCollection<UserViewModel>();
            ViewName = "Пользователи";
            LayoutName = "KursAM2.UserDialogViewModel.xml";
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<UserViewModel> Documents { set; get; }
        public string ViewName
        {
            get => myViewName;
            set
            {
                if (value == myViewName) return;
                myViewName = value;
                OnPropertyChanged("ViewName");
            }
        }
        public UserViewModel SelectedRow
        {
            get => mySelectedRow;
            set
            {
                if (value == mySelectedRow) return;
                mySelectedRow = value;
                OnPropertyChanged("SelectedRow");
            }
        }

        public override void RefreshData()
        {
            Documents.Clear();
            foreach (var d in GlobalOptions.GetEntities().EXT_USERS)
                Documents.Add(new UserViewModel
                {
                    UserId = d.USR_ID,
                    Name = d.USR_NICKNAME,
                    FullName = d.USR_FULLNAME
                });
        }
    }
}