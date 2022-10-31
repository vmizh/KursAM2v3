using System;

namespace KursDomain.ICommon;

/// <summary>
///     Строка документа
/// </summary>
public interface IRowId
{
    Guid Id { set; get; }
    Guid DocId { set; get; }
}
