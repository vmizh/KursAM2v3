using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.Projects;

public class ProjectLinkItem : RSViewModelBase
{
    [Display(AutoGenerateField = false)] public Project Project { set; get; }

    #region Fields

    private bool myIsSelected;

    #endregion

    #region Properties

    [Display(AutoGenerateField = false)] public override Guid Id => Project.Id;

    [Display(AutoGenerateField = true, Name = "Наименование", Order = 3)]
    public override string Name => Project.Name;

    [Display(AutoGenerateField = false)] public override Guid? ParentId => Project.ParentId;

    [Display(AutoGenerateField = true, Name = "Закрыт", Order = 3)]
    [ReadOnly(true)]
    public bool IsClosed { set; get; }
    [Display(AutoGenerateField = true, Name = "Начало", Order = 4)]
    [ReadOnly(true)]
    public DateTime? DateStart { set; get; }
    [Display(AutoGenerateField = true, Name = "Конец", Order = 5)]
    [ReadOnly(true)]
    public DateTime? DateEnd { set; get; }
    [Display(AutoGenerateField = true, Name = "Ответственный", Order = 6)]
    [ReadOnly(true)]
    public Employee Employee { set; get; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 7)]
    [ReadOnly(true)]
    public string Note { set; get; }

    [Display(AutoGenerateField = true, Name = "Выбран", Order = 2)]
    public bool IsSelected
    {
        set
        {
            if (myIsSelected == value) return;
            myIsSelected = value;
            RaisePropertyChanged();
        }
        get => myIsSelected;
    }

    #endregion
}
