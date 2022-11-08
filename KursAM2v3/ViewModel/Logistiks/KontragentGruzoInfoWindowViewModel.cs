using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursDomain;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class KontragentGruzoInfoWindowViewModel : RSWindowViewModelBase
    {
        private Kontragent mySelectedKontr;

        public KontragentGruzoInfoWindowViewModel()
        {
            Kontrs = new ObservableCollection<KontragentViewModel>();
            Data = new ObservableCollection<KontragentGruzoInfoViewModel>();
            ActualData = new ObservableCollection<KontragentGruzoInfoViewModel>();
        }

        public Kontragent SelectedKontr
        {
            get => mySelectedKontr;
            set
            {
                if (Equals(mySelectedKontr,value)) return;
                mySelectedKontr = value;
                LoadActualGruzoInfo(mySelectedKontr.DocCode);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ActualData));
            }
        }

        public ObservableCollection<KontragentGruzoInfoViewModel> Data { set; get; }
        public ObservableCollection<KontragentGruzoInfoViewModel> ActualData { set; get; }
        public ObservableCollection<KontragentViewModel> Kontrs { set; get; }

        public void LoadActualGruzoInfo(decimal dc)
        {
            ActualData.Clear();
            foreach (var d in Data.Where(_ => _.DocCode == dc))
                ActualData.Add(d);
        }

        public override void RefreshData(object data)
        {
            Kontrs.Clear();
            foreach (var d in GlobalOptions.GetEntities().SD_43.Include(_ => _.SD_301))
                Kontrs.Add(new KontragentViewModel(d));
            Data.Clear();
            foreach (var d in GlobalOptions.GetEntities().SD_43_GRUZO)
                Data.Add(new KontragentGruzoInfoViewModel(d));
        }
    }
}
