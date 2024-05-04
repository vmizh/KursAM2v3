using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.Services;
using Prism.Events;

namespace Core.ViewModel.Base;

[DataContract]
public abstract class RSViewModelBase2 : ISimpleObject, INotifyPropertyChanged,
    IComparable<RSViewModelBase2>,
    IComparable, IEquatable<RSViewModelBase2>
{

    #region Fields

    protected int myCode;
    private string myDescription;
    private decimal myDocCode;
    private Guid myId;
    public string myName;
    protected string myNote;
    protected object myParent;
    private decimal? myParentDC;
    private Guid? myParentId;
    private Guid myRowId;

    protected IEventAggregator EventAggregator;
    protected IMessageDialogService MessageDialogService;

    #endregion

    #region Constructors

    protected RSViewModelBase2(IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
    {
        EventAggregator = eventAggregator;
        MessageDialogService = messageDialogService;
    }

    public RSViewModelBase2()
    {
        EventAggregator = new EventAggregator();
        MessageDialogService = new MessageDialogService();
    }

    #endregion
    

    // ReSharper disable once InconsistentNaming
    public RowStatus myState = RowStatus.NotEdited;
    //private RSWindowViewModelBase myWindowViewModel;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once CollectionNeverQueried.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public IList<string> NotifyProtocol = new List<string>();

    

    [Display(AutoGenerateField = false)]
    public virtual object Parent
    {
        get => myParent;
        set
        {
            if (Equals(myParent, value)) return;
            myParent = value;
        }
    }

    [Display(AutoGenerateField = false)] public virtual bool IsNewDocument => myState == RowStatus.NewRow;

    [Display(AutoGenerateField = false)]
    public virtual decimal? ParentDC
    {
        get => myParentDC;
        set
        {
            if (myParentDC == value) return;
            myParentDC = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public virtual string StringId
    {
        set => Id = Guid.Parse(value);
        get => Id.ToString();
    }

    [Display(AutoGenerateField = true)]
    public virtual RowStatus State
    {
        set
        {
            myState = value;

            if (Parent is RSViewModelBase2 p)
                if (myState != RowStatus.NotEdited && p.State == RowStatus.NotEdited)
                    p.State = RowStatus.Edited;

            //RaisePropertyChanged();
        }
        get => myState;
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual Guid Id
    {
        get => myId;
        set
        {
            if (myId == value) return;
            myId = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual Guid RowId
    {
        get => myRowId;
        set
        {
            if (myRowId == value) return;
            myRowId = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual Guid? ParentId
    {
        get => myParentId;
        set
        {
            if (myParentId == value) return;
            myParentId = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual int Code
    {
        get => myCode;
        set
        {
            if (myCode == value) return;
            myCode = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual decimal DocCode
    {
        get => myDocCode;
        set
        {
            if (myDocCode == value) return;
            myDocCode = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual string Description
    {
        get => myDescription;
        set
        {
            if (myDescription == value) return;
            myDescription = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public string dialogServiceText { set; get; }

    public virtual int CompareTo(object obj)
    {
        var o = obj as RSViewModelBase2;
        if (o == null) return -1;
        return string.Compare(Name, o.Name, StringComparison.Ordinal);
    }

    public virtual int CompareTo(RSViewModelBase2 other)
    {
        return string.Compare(other.Name, Name, StringComparison.Ordinal);
    }

    public bool Equals(RSViewModelBase2 other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return myDocCode == other.DocCode && myId == other.Id && myCode == other.Code &&
               myRowId == other.RowId;
    }


    /// <summary>
    ///     Событие изменения значения свойства представления
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual string Name
    {
        get => myName;
        set
        {
            if (myName == value) return;
            myName = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    [Display(AutoGenerateField = false)]
    public virtual string Note
    {
        get => myNote;
        set
        {
            if (myNote == value) return;
            myNote = value;
            RaisePropertyChanged();
        }
    }

    public virtual bool IsCorrect()
    {
        return false;
    }

    public virtual object ToJson()
    {
        return null;
    }

    public virtual void Initialize()
    {
    }

    public void RaisePropertiesChanged()
    {
        var props = GetType().GetProperties();
        foreach (var p in props) RaisePropertyChanged(p.Name);
    }

    /// <summary>
    ///     Метод обработки события изменения значения свойства представления
    /// </summary>
    /// <param name="propertyName">Идентификатор свойства</param>
    [NotifyPropertyChangedInvocator("propertyName")]
    public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        NotifyProtocol.Add(propertyName);
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RSViewModelBase2)obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        unchecked
        {
            return (myDocCode.GetHashCode() * 397) ^ myId.GetHashCode();
        }
    }

    public virtual void RaisePropertyAllChanged()
    {
        foreach (var prop in GetType().GetProperties()) RaisePropertyChanged(prop.Name);
    }
}
