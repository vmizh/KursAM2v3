using System;
using System.Runtime.Serialization;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Data;

namespace KursAM2.ViewModel.Personal
{
    [DataContract]
    public class NachForEmployeeRowModelOld : RSViewModelData
    {
        private string myNotes;

        [DataMember] public Employee Employee { set; get; }

        [GridColumnView("Начисление", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public PayrollType PayType { get; set; }

        [GridColumnView("Валюта", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public Currency Crs { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Начислено", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal Summa { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Выплачено", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal PlatSumma { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Начислено (сотрудник)", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaEmp { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Выплачено (сотрудник)", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal PlatSummaEmp { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n4")]
        [GridColumnView("Курс", SettingsType.Decimal4, ReadOnly = true)]
        [DataMember]
        public decimal Rate { get; set; }

        [GridColumnView("Документ", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string PlatDocName { set; get; }

        [GridColumnView("Примечание", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string PlatDocNotes { set; get; }

        [GridColumnView("Дата", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public DateTime DocDate { set; get; }

        public string Notes
        {
            get => myNotes;
            set
            {
                if (myNotes == value) return;
                myNotes = value;
                RaisePropertyChanged();
            }
        }
    }
}