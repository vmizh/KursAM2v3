namespace KursDomain.IReferences.Nomenkl;

public interface INomenklMain
{
    string NomenklNumber { set; get; }
    IUnit Unit { set; get; }
    INomenklGroup Category { set; get; }
    string FullName { set; get; }
    bool IsUsluga { set; get; }
    bool IsProguct { set; get; }
    bool IsNakladExpense { set; get; }
    INomenklType NomenklType { set; get; }
    IProductType ProductType { set; get; }
    bool IsDeleted { set; get; }
    bool IsCurrencyTransfer { set; get; }
    bool IsRentabelnost { set; get; }
    bool IsOnlyState { set; get; }
    ICountry Country { set; get; }
}
