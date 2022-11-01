using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Systems;

public class KursUser : RSViewModelBase, IDataErrorInfo
{
    #region Fields

    private Users myEntity;

    #endregion

    #region Constructors

    public KursUser(Users entity)
    {
        Entity = entity ?? DefaultValue();
    }

    private Users DefaultValue()
    {
        return new Users
        {
            Id = Guid.NewGuid()
        };
    }

    #endregion

    #region Properties

    public Users Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => Entity.Name;
        set
        {
            if (Entity.Name == value) return;
            Entity.Name = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    public string FullName
    {
        get => Entity.FullName;
        set
        {
            if (Entity.FullName == value) return;
            Entity.FullName = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAdmin
    {
        get => Entity.IsAdmin;
        set
        {
            if (Entity.IsAdmin == value) return;
            Entity.IsAdmin = value;
            RaisePropertyChanged();
        }
    }

    public bool IsTester
    {
        get => Entity.IsTester;
        set
        {
            if (Entity.IsTester == value) return;
            Entity.IsTester = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => Entity.IsDeleted;
        set
        {
            if (Entity.IsDeleted == value) return;
            Entity.IsDeleted = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    #endregion

    #region Commands

    #endregion

    #region IDataErrorInfo

    public string this[string columnName] => "Не определено";

    [Display(AutoGenerateField = false)] public string Error { get; } = "";

    #endregion
}
