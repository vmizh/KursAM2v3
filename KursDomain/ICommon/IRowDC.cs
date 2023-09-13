using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

/// <summary>
///     Строка документа
/// </summary>
public interface IRowDC
{
    [Display(AutoGenerateField = false)]
    decimal DocCode { get; set; }
    [Display(AutoGenerateField = false)]
    int Code { get; set; }
}
