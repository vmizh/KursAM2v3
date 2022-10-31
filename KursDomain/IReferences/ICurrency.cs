namespace KursDomain.IReferences;

/// <summary>
///     Валюта
/// </summary>
public interface ICurrency
{
    string Code { get; set; }
    string FullName { get; set; }
    bool IsActive { get; set; }
}
