using System;
using Core.ViewModel.Base;

namespace Core.ViewModel
{
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
}