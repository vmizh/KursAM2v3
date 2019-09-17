using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostResetSearchWindowViewModel : RSWindowSearchViewModelBase
    {
        private Core.EntityViewModel.Warehouse myCurrentWarehouse;
        private Nomenkl mySelectedNomenkl;

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

        public List<Core.EntityViewModel.Warehouse> Sklads { set; get; } = new List<Core.EntityViewModel.Warehouse>();
        public List<Nomenkl> Nomenkls => MainReferences.ALLNomenkls.Values.ToList();
    }
}