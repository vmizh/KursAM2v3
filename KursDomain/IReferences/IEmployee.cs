using KursDomain.ICommon;

namespace KursDomain.IReferences;

/// <summary>
///     Работник
/// </summary>
public interface IEmployee
{
    int TabelNumber { get; set; }
    string NameFirst { get; set; }
    string NameSecond { get; set; }
    string NameLast { get; set; }

    ICurrency Currency { get; set; }

    /// <summary>
    ///     Описание должности
    /// </summary>
    string Position { get; set; }

    bool IsDeleted { set; get; }
}
