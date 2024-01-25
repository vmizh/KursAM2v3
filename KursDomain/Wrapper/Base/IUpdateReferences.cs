using KursDomain.IReferences;

namespace KursDomain.Wrapper.Base;

/// <summary>
/// Обновление ссылок на справочники 
/// </summary>
public interface IUpdateReferences
{
    IReferencesCache ReferencesCache { get; }
    void UpdateReferences(IReferencesCache cache);
}
