using System.Collections.Generic;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.IReferences;

/// <summary>
/// Касса
/// </summary>
public interface ICashBox
{
    ICurrency DefaultCurrency { get; set; }
    bool IsNegativeRests { get; set; }
    IKontragent Kontragent { get; set; }
    ICentrResponsibility CentrResponsibility { get; set; }
    bool IsNoBalans { set; get; }
    IEnumerable<ICashBoxStartRests> StartRests { get; set; }
}
