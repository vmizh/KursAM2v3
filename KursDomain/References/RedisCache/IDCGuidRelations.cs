using System;

namespace KursDomain.References.RedisCache;

public interface IDCGuidRelations
{
    Guid Id { set; get; }
    decimal DocCode { set; get; }
}

public class DCGuidRelations : IDCGuidRelations
{
    public Guid Id { get; set; }
    public decimal DocCode { get; set; }
}


public interface IDCIntRelations
{
    int Id { set; get; }
    decimal DocCode { set; get; }
}

public class DCIntRelations : IDCIntRelations
{
    public int Id { get; set; }
    public decimal DocCode { get; set; }
}
