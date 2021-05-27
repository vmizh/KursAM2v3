using System;
using System.ComponentModel;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Dogovora
{
    public class DogovorClientFactViewModel : RSViewModelBase,IDataErrorInfo
    {
        #region Fields

        private DogovorClientFact myEntity;

        #endregion 
        
        #region Constructors

        public DogovorClientFactViewModel()
        {
            Entity = DefaultValue();
        }

        public DogovorClientFactViewModel(DogovorClientFact entity, DogovorClientRowViewModel parent = null)
        {
            Entity = entity ?? DefaultValue();
            Parent = parent;
        }

        #endregion

        #region Properties

        public DogovorClientFact Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
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

        #endregion

        #region Methods

        private DogovorClientFact DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion

        #region Commands

        #endregion

        #region IDataErrorInfo
        public string this[string columnName] => "Не определено";

        public string Error { get; }

        #endregion
    }
}