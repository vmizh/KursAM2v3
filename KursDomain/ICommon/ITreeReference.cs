#nullable enable
using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public interface ITreeReference<I, P>
{
    [Display(AutoGenerateField = false)]
    I Id { set; get; }
    [Display(AutoGenerateField = false)]
    P ParentId { set; get; }
}
