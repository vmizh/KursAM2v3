using System;

namespace KursDomain.Base;

/// <summary>
/// Вспомогательный клас, чтобы вернуть значение Id
/// </summary>
public class IdItem
{
    public Guid? Id { set; get; }
    public decimal? DocCode { set; get; }
    public int? Code {set; get; }
}
