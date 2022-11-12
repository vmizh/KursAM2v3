using System.Collections.Generic;
using System.Linq;
using Core.ViewModel.Base;
using KursDomain;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostResetSearchWindowViewModel : RSWindowSearchViewModelBase
    {
        private KursDomain.References.Warehouse myCurrentWarehouse;
        private Nomenkl mySelectedNomenkl;

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

        public Nomenkl SelectedNomenkl
        {
            get => mySelectedNomenkl;
            set
            {
                if (Equals(mySelectedNomenkl, value)) return;
                mySelectedNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public List<KursDomain.References.Warehouse> Sklads { set; get; } =
            new List<KursDomain.References.Warehouse>();

        public List<Nomenkl> Nomenkls => GlobalOptions.ReferencesCache.GetNomenklsAll().Cast<Nomenkl>()
            .OrderBy(_ => _.Name).ToList();
    }
}
