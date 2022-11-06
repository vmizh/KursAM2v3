using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursAM2.View.Personal;
using KursDomain;
using KursDomain.Documents.Systems;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaAddUserForRights : RSWindowViewModelBase
    {
        //private readonly ALFAMEDIAEntities dbcontext = new ALFAMEDIAEntities();
        private User myCurrentRow;
        private PersonaAddUserRightsUC myDataUserControl;

        public PersonaAddUserForRights()
        {
            myDataUserControl = new PersonaAddUserRightsUC();
            Refresh();
        }

        public ObservableCollection<User> Rows { set; get; } = new ObservableCollection<User>();

        public User CurrentRow
        {
            get => myCurrentRow;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public PersonaAddUserRightsUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        private void Refresh()
        {
            Rows.Clear();
            using (var dtx = GlobalOptions.GetEntities())
            {
                var usrs = dtx.EXT_USERS.ToList();
                foreach (var u in usrs)
                    Rows.Add(new User(u));
            }
        }
    }
}
