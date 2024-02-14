using System.ComponentModel.DataAnnotations;

namespace KursDomain.ICommon;

public enum RowStatus
{
    [Display(Name = RowStatusText.NotEdited)] NotEdited = 0,
    [Display(Name = RowStatusText.Edited)] Edited = 1,
    [Display(Name = RowStatusText.Deleted)] Deleted = -1,
    [Display(Name = RowStatusText.NewRow)] NewRow = 2,
    [Display(Name = RowStatusText.NotDefinition)] NotDefinition = 3
}

public static class RowStatusText
{
    public const string NotEdited = "Сохранен";
    public const string Edited = "Изменен";
    public const string Deleted = "Удален";
    public const string NewRow = "Новый";
    public const string NotDefinition = "Не определен";
}
