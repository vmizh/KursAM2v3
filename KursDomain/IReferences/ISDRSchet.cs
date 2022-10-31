namespace KursDomain.IReferences;

/// <summary>
///     Счет доходов и расходов
/// </summary>
public interface ISDRSchet
{
    bool IsDeleted { set; get; }
    ISDRState SDRState { set; get; }
    bool IsPodOtchet { set; get; }

    /// <summary>
    ///     зарплата
    /// </summary>
    bool IsEmployeePayment { set; get; }
}
