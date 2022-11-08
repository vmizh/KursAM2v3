namespace KursDomain.IReferences.Nomenkl;

public interface INomenklGroup
{
    decimal? ParentDC { set; get; }
    string PathName { set; get; }
    int NomenklCount { get; set; }
}
