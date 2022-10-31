using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.IReferences;

/// <summary>
/// Расчетный счет
/// </summary>
public interface IBankAccount
{
    int RashAccCode { get; set; }
    string RashAccount { get; set; }
    short BACurrency { get; set; }
    IKontragent Kontragent { set; get; }
    bool IsTransit { get; set; }
    IBank Bank { set; get; }
    bool IsNegativeRests { get; set; }
    short? BABankAccount { get; set; }
    string ShortName { get; set; }
    ICentrResponsibility CentrResponsibility { get; set; }
    ICurrency Currency { get; set; }
    DateTime? StartDate { get; set; }
    decimal? StartSumma { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DateNonZero { get; set; }
}
