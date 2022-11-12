using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.Logistiks.UC;
using KursDomain;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Logistiks
{
    public class WarehouseSelectDialogViewModel : RSWindowViewModelBase
    {
        private KursDomain.References.Warehouse myCurrentWarehouse;
        private SelectWarehouse myDataUserControl;

        public WarehouseSelectDialogViewModel()
        {
            myDataUserControl = new SelectWarehouse();
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            WarehouseCollection =
                new ObservableCollection<KursDomain.References.Warehouse>(GlobalOptions.ReferencesCache
                    .GetWarehousesAll()
                    .Cast<KursDomain.References.Warehouse>().OrderBy(_ => _.Name));
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

        public ObservableCollection<KursDomain.References.Warehouse> WarehouseCollection { set; get; } =
            new ObservableCollection<KursDomain.References.Warehouse>();

        public ICommand SearchExecuteCommand
        {
            get { return new Command(SearchExecute, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        public KursDomain.References.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (Equals(myCurrentWarehouse, value)) return;
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
