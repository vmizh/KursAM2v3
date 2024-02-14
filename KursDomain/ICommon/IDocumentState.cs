using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public interface IDocumentState
{
    [Display(AutoGenerateField = false)]
    RowStatus RowStatus { get; set; }
}
