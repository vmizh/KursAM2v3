using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

/// <summary>
///     Строка документа
/// </summary>
public interface IRowDC : IDocCode
{
    [Display(AutoGenerateField = false)]
    int Code { get; set; }
}
