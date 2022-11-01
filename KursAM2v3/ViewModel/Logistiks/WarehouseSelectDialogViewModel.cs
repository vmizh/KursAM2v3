using System.Collections.ObjectModel;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.View.Logistiks.UC;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Logistiks
{
    public class WarehouseSelectDialogViewModel : RSWindowViewModelBase
    {
        private KursDomain.Documents.NomenklManagement.Warehouse myCurrentWarehouse;
        private SelectWarehouse myDataUserControl;

        public WarehouseSelectDialogViewModel()
        {
            myDataUserControl = new SelectWarehouse();
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            WarehouseCollection =
                new ObservableCollection<KursDomain.Documents.NomenklManagement.Warehouse>(MainReferences.Warehouses.Values);
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

        public ObservableCollection<KursDomain.Documents.NomenklManagement.Warehouse> WarehouseCollection { set; get; } =
            new ObservableCollection<KursDomain.Documents.NomenklManagement.Warehouse>();

        public ICommand SearchExecuteCommand
        {
            get { return new Command(SearchExecute, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public KursDomain.Documents.NomenklManagement.Warehouse CurrentWarehouse
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
            RaisePropertyChanged(nameof(WarehouseCollection));
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            WarehouseCollection.Clear();
            CurrentWarehouse = null;
            RaisePropertyChanged(nameof(WarehouseCollection));
        }
    }
}
