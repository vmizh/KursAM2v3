using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Data;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    public class EmployeePayMainViewModel : RSViewModelBase
    {
        public override decimal DocCode => Employee?.DocCode ?? 0;

        [DataMember] public Employee Employee { set; get; }

        [Display(Name = "Сотрудник")]
        [ReadOnly(true)]
        [DataMember]
        public string EmployeeName => Employee.Name;

        [Display(Name = "Таб. №")]
        [ReadOnly(true)]
        [DataMember]
        public int TabelNumber => Employee.TabelNumber;

        [Display(Name = "Валюта")]
        [ReadOnly(true)]
        [DataMember]
        public string CrsName => ((IName)Employee.Currency).Name;

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [Display(Name = "Начислено")]
        [ReadOnly(true)]
        [DataMember]
        public decimal SummaNach { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [Display(Name = "Выплачено")]
        [ReadOnly(true)]
        [DataMember]
        public decimal PlatSumma { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [Display(Name = "Долг")]
        [ReadOnly(true)]
        [DataMember]
        public decimal DolgSumma { get; set; }

        [Display(Name = "Дата. посл. операции")]
        [ReadOnly(true)]
        [DataMember]
        public DateTime DateLastOper { set; get; }

        public override string ToString()
        {
            return EmployeeName;
        }
    }
}
