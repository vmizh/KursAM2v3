using System.Collections.ObjectModel;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;

namespace Calculates.Materials
{
    public class NomenklCost : RSViewModelData
    {
        private Nomenkl myNomenkl;

        public Nomenkl Nomenkl
        {
            get => myNomenkl;
            set
            {
                if (myNomenkl != null && myNomenkl.Equals(value)) return;
                myNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklSkladRemain> Remains { set; get; } =
            new ObservableCollection<NomenklSkladRemain>();

        public ObservableCollection<NomenklCalcCostOperation> Operations { set; get; } =
            new ObservableCollection<NomenklCalcCostOperation>();
    }
}