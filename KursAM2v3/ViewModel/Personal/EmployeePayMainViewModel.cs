using System;
using System.Runtime.Serialization;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using DevExpress.Data;
using KursDomain.Documents.Employee;

namespace KursAM2.ViewModel.Personal
{
    public class EmployeePayMainViewModel : RSViewModelBase
    {
        public override decimal DocCode => Employee?.DocCode ?? 0;

        [DataMember] public Employee Employee { set; get; }

        [GridColumnView("Сотрудник", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string EmployeeName => Employee.Name;

        [GridColumnView("Таб. №", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public int TabelNumber => Employee.TabelNumber;

        [GridColumnView("Валюта", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public string CrsName => Employee.Currency.Name;

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Начислено", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal SummaNach { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Выплачено", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal PlatSumma { get; set; }

        [GridColumnSummary(SummaryItemType.Sum, "n2")]
        [GridColumnView("Долг", SettingsType.Decimal, ReadOnly = true)]
        [DataMember]
        public decimal DolgSumma { get; set; }

        [GridColumnView("Дата. посл. операции", SettingsType.Default, ReadOnly = true)]
        [DataMember]
        public DateTime DateLastOper { set; get; }

        public override string ToString()
        {
            return EmployeeName;
        }
    }
}
