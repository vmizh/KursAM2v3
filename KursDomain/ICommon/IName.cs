using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public interface IName : IDescription
{
    [Display(AutoGenerateField = true, Name = "Наименование")]
    string Name { set; get; }
    
}
