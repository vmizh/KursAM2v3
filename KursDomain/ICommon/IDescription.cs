using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public interface IDescription
{
    [Display(AutoGenerateField = true, Name = "Описание")]
    string Description { get; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    string Notes { get; set; }
}
