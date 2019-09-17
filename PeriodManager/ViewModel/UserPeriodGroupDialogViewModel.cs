using System.Collections.ObjectModel;
using Core;
using Core.ViewModel.Base;

namespace PeriodManager.ViewModel
{
    public class UserPeriodGroupDialogViewModel : KursViewModelBase
    {
        private PeriodGroupViewModel mySelectedRow;
        private string myViewName;

        public UserPeriodGroupDialogViewModel()
        {
            Documents = new ObservableCollection<PeriodGroupViewModel>();
            ViewName = "Группы пользователей для закрытия периодов";
            LayoutName = "KursAM2.UserPeriodGroupDialogViewModel.xml";
        }

        public ObservableCollection<PeriodGroupViewModel> Documents { set; get; }

        public string ViewName
        {
            get { return myViewName; }
            set
            {
                if (value == myViewName) return;
                myViewName = value;
                OnPropertyChanged(nameof(ViewName));
            }
        }

        public PeriodGroupViewModel SelectedRow
        {
            get { return mySelectedRow; }
            set
            {
                if (value == mySelectedRow) return;
                mySelectedRow = value;
                OnPropertyChanged(nameof(SelectedRow));
            }
        }

        public override void RefreshData()
        {
            Documents.Clear();
            foreach (var d in GlobalOptions.GetEntities().PERIOD_GROUPS)
            {
                Documents.Add(new PeriodGroupViewModel
                {
                    Id = d.ID,
                    Name = d.NAME
                });
            }
        }
    }
}