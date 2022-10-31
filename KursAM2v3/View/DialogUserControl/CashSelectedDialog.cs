using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using KursDomain.Documents.Cash;

namespace KursAM2.View.DialogUserControl
{
    public class CashSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private Cash myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public CashSelectedDialog(List<Cash> excludeList = null)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор кассы";
            if (excludeList == null)
            {
                ItemsCollection = new ObservableCollection<Cash>(MainReferences.Cashs.Values);
            }
            else
            {
                ItemsCollection = new ObservableCollection<Cash>();
                foreach (var c in MainReferences.Cashs.Values)
                    if (!excludeList.Contains(c))
                        ItemsCollection.Add(c);
            }
        }

        public ObservableCollection<Cash> ItemsCollection { set; get; }

        public Cash CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem != null && myCurrentItem.Equals(value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public StandartDialogSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl { get; }
    }
}
