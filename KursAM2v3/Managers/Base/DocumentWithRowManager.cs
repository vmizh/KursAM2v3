using System;
using System.Collections.ObjectModel;

namespace KursAM2.Managers.Base
{
    public abstract class DocumentWithRowManager<T, R> : DocumentManager<T> where T : class
    {
        public abstract R AddRow(ObservableCollection<R> rows, R row, short dolg);
        public abstract R DeleteRow(ObservableCollection<R> rows, ObservableCollection<R> deletedrows, R row);
        public abstract Tuple<bool, string> IsRowChecked(R row);
    }
}
