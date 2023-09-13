using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

/// <summary>
///     DocCode
/// </summary>
public interface IDocCode
{
    [Display(AutoGenerateField = false)]
    decimal DocCode { get; set; }
}
