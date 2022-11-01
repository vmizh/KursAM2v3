using System;
using System.ComponentModel;
using KursDomain.ICommon;

namespace Core.ViewModel.Base;

public abstract class RSViewModelData : RSViewModelBase
{
    public event EventHandler<StateEventArgs> StateChanged;

    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke(this, new StateEventArgs(this, State));
    }

    private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (Parent is RSViewModelBase p)
        {
            if (p.State != RowStatus.NotEdited) return;
            p.State = RowStatus.Edited;
            p.RaisePropertyChanged();
        }
    }

    public override void RaisePropertyChanged(string propertyName = null)
    {
        base.RaisePropertyChanged(propertyName);
        if (State != RowStatus.NotEdited) return;
        State = RowStatus.Edited;
        OnStateChanged();
    }
}

public class StateEventArgs : EventArgs
{
    public StateEventArgs(RSViewModelData row, RowStatus state)
    {
        Row = row;
        State = state;
    }

    public RSViewModelData Row { get; }
    public RowStatus State { get; }
}
