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
public abstract class RSViewModelBase : ISimpleObject, INotifyPropertyChanged,
    IComparable<RSViewModelBase>,
    IComparable, IEquatable<RSViewModelBase>
{

    public DateTime timeLoad { set; get; } = DateTime.Now;
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
            switch (myState)
            {
                case RowStatus.NewRow:
                    return;
                default:
                    myState = value;
                    break;
            }

            if (Parent is RSViewModelBase p)
                if (myState != RowStatus.NotEdited && p.State == RowStatus.NotEdited)
                    p.State = RowStatus.Edited;

            RaisePropertyChanged();
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
        var o = obj as RSViewModelBase;
        if (o == null) return -1;
        return string.Compare(Name, o.Name, StringComparison.Ordinal);
    }

    public virtual int CompareTo(RSViewModelBase other)
    {
        return string.Compare(other.Name, Name, StringComparison.Ordinal);
    }

    public bool Equals(RSViewModelBase other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return myDocCode == other.DocCode && myId == other.Id && myCode == other.Code &&
               myRowId == other.RowId && other.timeLoad == timeLoad;
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
        if (propertyName != "State")
            if (myState == RowStatus.NotEdited)
            {
                myState = RowStatus.Edited;
                RaisePropertyChanged("State");
            }

        if (Parent is RSViewModelBase p)
            if (myState != RowStatus.NotEdited)
            {
                if (p.myState != RowStatus.NewRow) p.myState = RowStatus.Edited;
                p.RaisePropertyChanged("State");
            }

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
        return Equals((RSViewModelBase)obj);
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

    protected RSViewModelBase(IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
    {
        EventAggregator = eventAggregator;
        MessageDialogService = messageDialogService;
    }

    public RSViewModelBase()
    {
        EventAggregator = new EventAggregator();
        MessageDialogService = new MessageDialogService();
    }

    #endregion
}
