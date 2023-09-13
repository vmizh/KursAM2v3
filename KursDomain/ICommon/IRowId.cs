using System;
using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

/// <summary>
///     Строка документа
/// </summary>
public interface IRowId
{
    [Display(AutoGenerateField = false)]
    Guid Id { set; get; }
    [Display(AutoGenerateField = false)]
    Guid DocId { set; get; }
}
