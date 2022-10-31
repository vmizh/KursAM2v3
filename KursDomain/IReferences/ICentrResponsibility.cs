namespace KursDomain.IReferences;

/// <summary>
/// Центр ответственности
/// </summary>
public interface ICentrResponsibility
{
    string FullName { set; get; }
    decimal? ParentDC { set; get; }
    bool IsDeleted { set; get; }
}
