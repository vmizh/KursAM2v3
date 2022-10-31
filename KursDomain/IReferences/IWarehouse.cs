using System;

namespace KursDomain.IReferences;

/// <summary>
///     Склад
/// </summary>
public interface IWarehouse
{
    /// <summary>
    ///     Кладовщик
    /// </summary>
    IEmployee StoreKeeper { get; set; }

    IRegion Region { set; get; }
    bool IsNegativeRest { set; get; }
    bool IsDeleted { set; get; }
    Guid? ParentId { set; get; }
    bool IsOutBalans { set; get; }
}
