using System;
using System.ComponentModel.DataAnnotations;

namespace KursDomain.IDocuments;

public interface ILastChanged
{
    /// <summary>
    /// Пользователь, последний изменивший документ
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Посл.изменил")]
    string LastChanger { set; get; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Дата посл.изм.")]
    DateTime LastChangerDate { set; get; }
}
