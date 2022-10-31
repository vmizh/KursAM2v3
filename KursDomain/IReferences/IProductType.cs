namespace KursDomain.IReferences;

/// <summary>
/// Тип продукции
/// </summary>
public interface IProductType
{
    decimal? ParentDC { set; get; }
    string FullName { set; get; }
}
