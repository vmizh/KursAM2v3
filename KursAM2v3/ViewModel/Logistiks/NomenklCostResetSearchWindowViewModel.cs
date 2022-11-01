using System.Collections.Generic;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostResetSearchWindowViewModel : RSWindowSearchViewModelBase
    {
        private KursDomain.Documents.NomenklManagement.Warehouse myCurrentWarehouse;
        private Nomenkl mySelectedNomenkl;

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

        public List<KursDomain.Documents.NomenklManagement.Warehouse> Sklads { set; get; } =
            new List<KursDomain.Documents.NomenklManagement.Warehouse>();

        public List<Nomenkl> Nomenkls => MainReferences.ALLNomenkls.Values.ToList();
    }
}
