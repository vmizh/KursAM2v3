using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DevExpress.Mvvm;
using JetBrains.Annotations;

namespace Core.ViewModel.Base
{
    public interface ISimpleObject
    {
        string Name { set; get; }
        string Note { set; get; }
    }

    [DataContract]
    public abstract class RSViewModelBase : ViewModelBase, ISimpleObject, INotifyPropertyChanged,
        IComparable<RSViewModelBase>,
        IComparable, IEquatable<RSViewModelBase>
    {
        protected int myCode;
        private decimal myDocCode;
        private Guid myId;
        public string myName;
        protected string myNote;
        private RSViewModelBase myParent;
        private decimal? myParentDC;
        private Guid? myParentId;
        private Guid myRowId;

        // ReSharper disable once InconsistentNaming
        public RowStatus myState = RowStatus.NotEdited;
        private RSWindowViewModelBase myWindowViewModel;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once CollectionNeverQueried.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public IList<string> NotifyProtocol = new List<string>();
        private string myDescription;

        [Display(AutoGenerateField = false)]
        public RSWindowViewModelBase WindowViewModel
        {
            get => myWindowViewModel;
            set
            {
                if (Equals(myWindowViewModel, value)) return;
                myWindowViewModel = value;
            }
        }

        //[Display(Name = "Родитель", Description = "Родительский объект",AutoGenerateField = false), ReadOnly(true)]

        [Display(AutoGenerateField = false)]
        public RSViewModelBase Parent
        {
            get => myParent;
            set
            {
                if (Equals(myParent, value)) return;
                myParent = value;
            }
        }

        [Display(AutoGenerateField = false)] 
        public virtual bool IsNewDocument => myState == RowStatus.NewRow;

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
        [Display(AutoGenerateField = false)]
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

                if (myState != RowStatus.NotEdited && Parent != null && Parent.State == RowStatus.NotEdited)
                    Parent.State = RowStatus.Edited;
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
            return myDocCode == other.DocCode && myId.Equals(other.Id) && myCode.Equals(other.Code) &&
                   myRowId.Equals(other.RowId);
        }


        /// <summary>
        ///     Событие изменения значения свойства представления
        /// </summary>
        public new virtual event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
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

        [DataMember]
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

        public virtual void Initialize()
        {
        }

        /// <summary>
        ///     Метод обработки события изменения значения свойства представления
        /// </summary>
        /// <param name="propertyName">Идентификатор свойства</param>
        [NotifyPropertyChangedInvocator("propertyName")]
        public new virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (propertyName != "State")
                if (State == RowStatus.NotEdited)
                    State = RowStatus.Edited;
            if (Parent != null && myState != RowStatus.NotEdited)
                if (Parent.State != RowStatus.NewRow)
                    Parent.State = RowStatus.Edited;
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
            return Equals((RSViewModelBase) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (myDocCode.GetHashCode() * 397) ^ myId.GetHashCode();
            }
        }
    }
}