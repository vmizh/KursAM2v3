using System;
using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

/// <summary>
///     Guid Id for documents
/// </summary>
public interface IDocGuid
{
    [Display(AutoGenerateField = false)]
    Guid Id { get; set; }
}
