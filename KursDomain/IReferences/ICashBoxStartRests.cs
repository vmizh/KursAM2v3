using System;

namespace KursDomain.IReferences;

/// <summary>
/// Стартовые остатки в кассе
/// </summary>
public interface ICashBoxStartRests
{
    ICurrency Currency { get; set; }
    DateTime DateStart { get; set; }
    decimal SummaStart { get; set; }
    decimal CashDateDC { get; set; }
}
