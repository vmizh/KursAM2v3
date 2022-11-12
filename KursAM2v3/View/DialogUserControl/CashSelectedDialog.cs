using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain;
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
                ItemsCollection =
                    new ObservableCollection<CashBox>(GlobalOptions.ReferencesCache.GetCashBoxAll().Cast<CashBox>());
            }
            else
            {
                ItemsCollection = new ObservableCollection<CashBox>();
                foreach (var c in GlobalOptions.ReferencesCache.GetCashBoxAll().Cast<CashBox>())
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
