using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Systems;

internal class KursMenuGroupViewModel : RSViewModelBase
{
    public KursMenuGroup Entity { get; set; }

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

    public int OrderBy
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

    public new int? ParentId
    {
        get => Entity.ParentId;
        set
        {
            if (Entity.ParentId == value)
                return;
            Entity.ParentId = value;
            RaisePropertyChanged();
        }
    }
}
