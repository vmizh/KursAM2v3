namespace KursDomain.IReferences;

/// <summary>
///     Статья доходов и расходов
/// </summary>
public interface ISDRState
{
    string Shifr { set; get; }
    decimal? ParentDC { set; get; }
    bool IsDohod { set; get; }
}
