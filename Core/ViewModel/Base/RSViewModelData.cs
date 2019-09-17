using System;
using System.ComponentModel;

namespace Core.ViewModel.Base
{
    public abstract class RSViewModelData : RSViewModelBase
    {
        public event EventHandler<StateEventArgs> StateChanged;

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, new StateEventArgs(this, State));
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Parent.State != RowStatus.NotEdited) return;
            Parent.State = RowStatus.Edited;
            Parent.RaisePropertyChanged();
        }

        public override void RaisePropertyChanged(string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
            if (State != RowStatus.NotEdited) return;
            State = RowStatus.Edited;
            OnStateChanged();
        }
    }
}