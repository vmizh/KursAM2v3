using System;
using System.Collections.ObjectModel;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using KursAM2.ViewModel.Dogovora;

namespace KursAM2.ViewModel.Logistiks
{
    public class DogovorForClientViewModel : RSViewModelBase
    {
        private DogovorClientHead myHead;

        public DogovorForClientViewModel()
        {
            Rows = new ObservableCollection<DogovorClientRow>();
        }

        public DogovorForClientViewModel(SD_9 doc) : this()
        {
            Head = new DogovorClientHead(doc);
            foreach (var row in doc.TD_9)
                Rows.Add(new DogovorClientRow(row));
        }

        public DogovorClientHead Head
        {
            get => myHead;
            set
            {
                if (myHead != null && myHead.Equals(value)) return;
                myHead = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DocDate => Head.ZAK_DATE;
        public string DocNum => Head.ZAK_NUM + "/" + Head.ZAK_OUT_NUM;
        public Kontragent Client => Head.Client;
        public Kontragent Diler => Head.Diler;
        public decimal DilerSumma => (decimal) Head.ZAK_DOHOD_DILERA;
        public ObservableCollection<DogovorClientRow> Rows { set; get; }
        public Currency Currency => Head.Currency;
        public string Base => Head.ZAK_BASE;
        public string DogTypeName => Head.SD_102?.TD_NAME;
        public string FormRaschet => Head.SD_189?.OOT_NAME;

        public decimal Summa
        {
            get
            {
                if (Head.ZAK_CRS_SUMMA != null) return (decimal) Head.ZAK_CRS_SUMMA;
                return 0;
            }
        }
    }
}