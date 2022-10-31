using System;

namespace KursDomain.ICommon;

/// <summary>
///     Guid Id for documents
/// </summary>
public interface IDocGuid
{
    Guid Id { get; set; }
}
