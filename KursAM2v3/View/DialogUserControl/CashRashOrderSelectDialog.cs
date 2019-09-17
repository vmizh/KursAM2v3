using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.View.DialogUserControl
{
    public class CashRashOrderSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        private CashOut myCurrentItem;
        private StandartDialogSelectUC myDataUserControl;

        public CashRashOrderSelectDialog(CashIn order)
        {
            Order = order;
            LayoutControl = myDataUserControl = new StandartDialogSelectUC(GetType().Name);
            WindowName = "Выбор расходного ордера";
            RefreshData(null);
        }

        public ObservableCollection<CashOut> ItemsCollection { set; get; } =
            new ObservableCollection<CashOut>();

        private CashIn Order { get; }

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

        public CashOut CurrentItem
        {
            get => myCurrentItem;
            set
            {
                if (myCurrentItem != null && myCurrentItem.Equals(value)) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl { get; }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var exsitsDC = ctx.SD_33.Where(_ => _.RASH_ORDER_FROM_DC != null).Select(_ => _.RASH_ORDER_FROM_DC)
                    .ToList();
                ItemsCollection.Clear();
                foreach (var d in ctx.SD_34.Where(_ => _.CASH_TO_DC == Order.CA_DC && !exsitsDC.Contains(_.DOC_CODE))
                    .ToList())
                    ItemsCollection.Add(new CashOut(d));
            }
        }
    }
}