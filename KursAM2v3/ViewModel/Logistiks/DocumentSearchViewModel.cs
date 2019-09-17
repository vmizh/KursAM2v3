using System;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class DocumentSearchViewModel : RSViewModelBase
    {
        private ObservableCollection<DocumentRowSearchViewModel> _rows;
        private GruzoInfoViewModel myGruzoInfo;

        public DocumentSearchViewModel()
        {
            Rows = new ObservableCollection<DocumentRowSearchViewModel>();
        }

        public ObservableCollection<DocumentRowSearchViewModel> Rows
        {
            set
            {
                _rows = value;
                RaisePropertyChanged();
            }
            get => _rows;
        }

        public string DocumentType { set; get; }
        public string DocumentName { set; get; }
        public string DocNum { set; get; }
        public DateTime DocDate { set; get; }
        public string KontragentOuter { set; get; }
        public decimal KontragentOuterDC { set; get; }
        public string KontragentInner { set; get; }
        public decimal KontragentInnerDC { set; get; }
        public string Currency { set; get; }
        public decimal Summa { set; get; }

        public GruzoInfoViewModel GruzoInfo
        {
            set
            {
                if (myGruzoInfo != null && myGruzoInfo.Equals(value)) return;
                myGruzoInfo = value;
                RaisePropertyChanged();
            }
            get => myGruzoInfo;
        }
    }
}