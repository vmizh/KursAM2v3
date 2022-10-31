namespace KursDomain.IReferences.Kontragent;

/// <summary>
///     Категория контрагента (для справочника и выбора)
/// </summary>
public interface IKontragentCategory
{
    int Id { get; set; }
    bool IsDeleted { get; set; }
    int? ParentId { get; set; }
}
