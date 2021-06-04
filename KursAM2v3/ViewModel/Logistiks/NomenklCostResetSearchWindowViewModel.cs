using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostResetSearchWindowViewModel : RSWindowSearchViewModelBase
    {
        private Core.EntityViewModel.NomenklManagement.Warehouse myCurrentWarehouse;
        private Nomenkl mySelectedNomenkl;

        public Core.EntityViewModel.NomenklManagement.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl SelectedNomenkl
        {
            get => mySelectedNomenkl;
            set
            {
                if (mySelectedNomenkl != null && mySelectedNomenkl.Equals(value)) return;
                mySelectedNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public List<Core.EntityViewModel.NomenklManagement.Warehouse> Sklads { set; get; } = new List<Core.EntityViewModel.NomenklManagement.Warehouse>();
        public List<Nomenkl> Nomenkls => MainReferences.ALLNomenkls.Values.ToList();
    }
}