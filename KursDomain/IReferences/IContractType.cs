namespace KursDomain.IReferences;

/// <summary>
/// Тип договоров
/// </summary>
public interface IContractType
{
    /// <summary>
    /// true - покупка, false - продажа
    /// </summary>
    bool IsBuy { set; get; }

    /// <summary>
    /// Дополнительное соглашение
    /// </summary>
    bool IsAadditionalAgreement { set; get; }
    bool IsDiler { set; get; }

}
