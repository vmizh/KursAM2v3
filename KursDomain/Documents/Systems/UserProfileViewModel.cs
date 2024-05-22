using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace KursDomain.Documents.Systems;

public class UserProfileViewModel : RSViewModelBase, IEntity<UserProfile>
{
    private UserProfile myEntity;

    public UserProfileViewModel()
    {
        Entity = new UserProfile { Id = Guid.NewGuid() };
    }

    public List<decimal> BankAccess { set; get; } = new List<decimal>();
    public List<decimal> CashAccess { set; get; } = new List<decimal>();

    public UserProfileViewModel(UserProfile entity)
    {
        Entity = entity ?? new UserProfile { Id = Guid.NewGuid() };
    }

    public int UserId
    {
        get => Entity.UserId;
        set
        {
            if (Entity.UserId == value) return;
            Entity.UserId = value;
            RaisePropertyChanged();
        }
    }

    public string OptionName
    {
        get => Entity.OptionName;
        set
        {
            if (Entity.OptionName == value) return;
            Entity.OptionName = value;
            RaisePropertyChanged();
        }
    }

    public string OptionValue
    {
        get => Entity.OptionValue;
        set
        {
            if (Entity.OptionValue == value) return;
            Entity.OptionValue = value;
            RaisePropertyChanged();
        }
    }

    public EXT_USERS EXT_USERS
    {
        get => Entity.EXT_USERS;
        set
        {
            if (Entity.EXT_USERS == value) return;
            Entity.EXT_USERS = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public UserProfile Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public UserProfile DefaultValue()
    {
        return new UserProfile
        {
            Id = Guid.NewGuid()
        };
    }

    public List<UserProfile> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual UserProfile Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual UserProfile Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public UserProfile Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public UserProfile Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(UserProfile doc)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(UserProfile ent)
    {
        Id = ent.Id;
        UserId = ent.UserId;
        OptionName = ent.OptionName;
        OptionValue = ent.OptionValue;
        Note = ent.Note;
        EXT_USERS = ent.EXT_USERS;
    }

    public void UpdateTo(UserProfile ent)
    {
        ent.Id = Id;
        ent.UserId = UserId;
        ent.OptionName = OptionName;
        ent.OptionValue = OptionValue;
        ent.Note = Note;
        ent.EXT_USERS = EXT_USERS;
    }
}
