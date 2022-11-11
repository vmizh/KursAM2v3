using System;
using DevExpress.Mvvm;

namespace KursDomain.Documents.Cash.Interfaces;

public interface ICashIn
{
    int Num { set; get; }
    decimal Summa { set; get; }
    string Name { set; get; }
    string Osn { set; get; }
    string Note { set; get; }
    decimal DocCode { set; get; }
    Guid Id { set; get; }
    DateTime Date { set; get; }
    //References.Employee
}
