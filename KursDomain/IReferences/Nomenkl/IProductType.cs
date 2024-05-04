namespace KursDomain.IReferences.Nomenkl;

/// <summary>
///     тип продукции -> тип номенклатуры (вид продукции)
/// </summary>
public interface IProductType
{
    string FullName { set; get; }
    decimal? ParentDC { set; get; }
}
