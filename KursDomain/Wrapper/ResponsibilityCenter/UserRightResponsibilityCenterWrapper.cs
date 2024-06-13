using System;
using System.ComponentModel.DataAnnotations;
using Data;
using KursDomain.ICommon;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.ResponsibilityCenter;

public class UserRightResponsibilityCenterWrapper : BaseWrapper<UserRightsResponsibilityCenter>, IEquatable<UserRightResponsibilityCenterWrapper>
{
    private bool myIsSelected;
    private string myUserName;
    private string myUserFullName;

    public UserRightResponsibilityCenterWrapper(UserRightsResponsibilityCenter model, IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService) : base(model, eventAggregator, messageDialogService)
    {
    }

    public UserRightResponsibilityCenterWrapper(UserRightsResponsibilityCenter model) : base(model)
    {

    }
    
    #region Properties

    [Display(AutoGenerateField = false)]
    public override Guid Id
    {
        set => SetValue(value);
        get => GetValue<Guid>();
    }

    [Display(AutoGenerateField = false)]
    public Guid UserId
    {
        set => SetValue(value);
        get => GetValue<Guid>();
    }

    public bool IsSelected
    {
        get => myIsSelected;
        set
        {
            if (value == myIsSelected) return;
            myIsSelected = value;
            RaisePropertyChanged();
        }
    }

    public string UserName
    {
        get => myUserName;
        set
        {
            if (value == myUserName) return;
            myUserName = value;
            RaisePropertyChanged();
        }
    }

    public string UserFullName
    {
        get => myUserFullName;
        set
        {
            if (value == myUserFullName) return;
            myUserFullName = value;
            RaisePropertyChanged();
        }
    }

    #endregion

   
    public bool Equals(UserRightResponsibilityCenterWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode;
    }
}
