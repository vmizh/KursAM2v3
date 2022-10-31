namespace KursDomain.IReferences;

/// <summary>
/// Типы взаиморасчетов для инвойсов
/// </summary>
public interface ISettlementType
{
    int? MSType { get; set; }
    ISDRSchet SDRSchet { get; set; }
}
