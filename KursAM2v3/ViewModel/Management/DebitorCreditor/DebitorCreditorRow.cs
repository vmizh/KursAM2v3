using System.Runtime.Serialization;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace KursAM2.ViewModel.Management.DebitorCreditor
{
    [DataContract]
    public class DebitorCreditorRow : RSViewModelBase
    {
        private bool myIsSelected;

        [DataMember] public Kontragent KontrInfo { set; get; }

        [DataMember] public decimal UchetStart { set; get; }

        [DataMember] public decimal UchetOut { set; get; }

        [DataMember] public decimal UchetIn { set; get; }

        [DataMember] public decimal UchetEnd { set; get; }

        [DataMember] public decimal KontrStart { set; get; }

        [DataMember] public decimal KontrOut { set; get; }

        [DataMember] public decimal KontrIn { set; get; }

        [DataMember] public decimal KontrEnd { set; get; }

        [DataMember] public decimal Delta { set; get; }

        [DataMember] public bool IsBalans { set; get; }

        [DataMember] public string Kontragent { set; get; }

        [DataMember] public string CurrencyName { set; get; }

        public bool IsSelected
        {
            get => myIsSelected;
            set
            {
                if (myIsSelected == value) return;
                myIsSelected = value;
                RaisePropertyChanged();
            }
        }
    }
}