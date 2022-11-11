using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursAM2.View.DialogUserControl;
using KursDomain.Documents.Employee;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    public class EmployeesUCViewModel : RSWindowViewModelBase
    {
        private DialogEmployee myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public EmployeesUCViewModel()
        {
            myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #region command

        public override void RefreshData(object o)
        {
            ItemsCollection = new ObservableCollection<DialogEmployee>();
            foreach (var i in MainReferences.Employees.Values.ToList())
                ItemsCollection.Add(new DialogEmployee
                {
                    DocCode = i.DocCode,
                    Name = i.Name,
                    Currency = i.Currency,
                    TabelNumber = i.TabelNumber,
                    NameFirst = i.NameFirst,
                    NameLast = i.NameLast,
                    NameSecond = i.NameSecond
                });
            RaisePropertyChanged(nameof(ItemsCollection));
        }

        #endregion

        #region properties

        public ObservableCollection<DialogEmployee> ItemsCollection { set; get; } =
            new ObservableCollection<DialogEmployee>();

        public StandartDialogSelectUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
            get => myDataUserControl;
        }

        public DialogEmployee CurrentItem
        {
            set
            {
                if (Equals(myCurrentItem,value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
            get => myCurrentItem;
        }

        #endregion
    }
}
