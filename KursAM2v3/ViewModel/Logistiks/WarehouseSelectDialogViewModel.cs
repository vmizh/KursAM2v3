using System.Collections.ObjectModel;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers.Invoices;
using KursAM2.View.Logistiks.UC;

namespace KursAM2.ViewModel.Logistiks
{
    public class WarehouseSelectDialogViewModel : RSWindowViewModelBase
    {
        private Core.EntityViewModel.Warehouse myCurrentWarehouse;
        private SelectWarehouse myDataUserControl;

        public WarehouseSelectDialogViewModel()
        {
            myDataUserControl = new SelectWarehouse();
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            WarehouseCollection =
                new ObservableCollection<Core.EntityViewModel.Warehouse>(MainReferences.Warehouses.Values);
            WindowName = "Выбор склада";
        }

        public SelectWarehouse DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Core.EntityViewModel.Warehouse> WarehouseCollection { set; get; } =
            new ObservableCollection<Core.EntityViewModel.Warehouse>();

        public ICommand SearchExecuteCommand
        {
            get { return new Command(SearchExecute, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public Core.EntityViewModel.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                RaisePropertyChanged();
            }
        }

        public void SearchExecute(object obj)
        {
            WarehouseCollection.Clear();
            foreach (var n in WarehouseManager.GetWarehouses(null, SearchText))
                WarehouseCollection.Add(n);
            RaisePropertiesChanged(nameof(WarehouseCollection));
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            WarehouseCollection.Clear();
            CurrentWarehouse = null;
            RaisePropertiesChanged(nameof(WarehouseCollection));
        }
    }
}