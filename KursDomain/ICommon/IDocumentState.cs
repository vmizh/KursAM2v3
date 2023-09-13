using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public enum RowStatus
{
    [Display(Name = "Сохранен")] NotEdited = 0,
    [Display(Name = "Изменен")] Edited = 1,
    [Display(Name = "Удален")] Deleted = -1,
    [Display(Name = "Новый")] NewRow = 2,
    [Display(Name = "Не определен")] NotDefinition = 3
}

public interface IDocumentState
{
    [Display(AutoGenerateField = false)]
    RowStatus RowStatus { get; set; }
}
