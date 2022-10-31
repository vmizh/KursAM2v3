using KursDomain.ICommon;

namespace KursDomain.IReferences;

/// <summary>
///     Регион
/// </summary>
public interface IRegion
{
    decimal? ParentDC { get; set; }
}
