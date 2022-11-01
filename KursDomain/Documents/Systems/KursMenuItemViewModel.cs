using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Systems;

public class KursMenuItemViewModel : RSViewModelBase
{
    #region Constructor

    public KursMenuItemViewModel(KursMenuItem entityKursMenuItem)
    {
        Entity = entityKursMenuItem;
    }

    #endregion

    #region Fields

    private bool myIsSelectedItem;
    private string myGroupName;

    #endregion

    #region Properties

    [Display(AutoGenerateField = false)] public KursMenuItem Entity { get; set; }

    [DisplayName("Статус")]
    public bool IsSelectedItem
    {
        get => myIsSelectedItem;
        set
        {
            if (myIsSelectedItem == value)
                return;
            myIsSelectedItem = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public new int Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value)
                return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }


    [Display(AutoGenerateField = false)]
    public int GroupId
    {
        get => Entity.GroupId;
        set
        {
            if (Entity.GroupId == value)
                return;
            Entity.GroupId = value;
            RaisePropertyChanged();
        }
    }

    [Display(Name = "Группа")]
    public string GroupName
    {
        get => myGroupName;
        set
        {
            if (myGroupName == value)
                return;
            myGroupName = value;
            RaisePropertyChanged();
        }
    }

    [Display(Name = "Наименование")]
    public override string Name
    {
        get => Entity.Name;
        set
        {
            if (Entity.Name == value)
                return;
            Entity.Name = value;
            RaisePropertyChanged();
        }
    }

    [DisplayName("Примечание")]
    [Display(AutoGenerateField = false)]
    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value)
                return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    [DisplayName("Ссылка на меню")]
    [Display(AutoGenerateField = false)]
    public int? OrderBy
    {
        get => Entity.OrderBy;
        set
        {
            if (Entity.OrderBy == value)
                return;
            Entity.OrderBy = value;
            RaisePropertyChanged();
        }
    }

    [DisplayName("Поддерживает подписи")]
    [Display(AutoGenerateField = true)]
    public bool IsSignature
    {
        get => Entity.IsSign;
        set
        {
            if (Entity.IsSign == value)
                return;
            Entity.IsSign = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public new string Code
    {
        get => Entity.Code;
        set
        {
            if (Entity.Code == value)
                return;
            Entity.Code = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public byte[] Picture
    {
        get => Entity.Picture;
        set
        {
            if (Entity.Picture == value)
                return;
            Entity.Picture = value;
            RaisePropertyChanged();
        }
    }

    #endregion
}
