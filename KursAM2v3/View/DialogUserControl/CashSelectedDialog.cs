using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl
{
    public class CashSelectedDialog : RSWindowViewModelBase, IDataUserControl
    {
        private CashBox myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public CashSelectedDialog(List<CashBox> excludeList = null)
        {
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор кассы";
            if (excludeList == null)
            {
                ItemsCollection = new ObservableCollection<CashBox>(MainReferences.Cashs.Values);
            }
            else
            {
                ItemsCollection = new ObservableCollection<CashBox>();
                foreach (var c in MainReferences.Cashs.Values)
                    if (!excludeList.Contains(c))
                        ItemsCollection.Add(c);
            }
        }

        public ObservableCollection<CashBox> ItemsCollection { set; get; }

        public CashBox CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (Equals(myCurrentItem, value)) return;
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
