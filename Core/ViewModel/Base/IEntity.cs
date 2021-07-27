using System;
using System.Collections.ObjectModel;

//using System.Data;

namespace Core.ViewModel.Base
{
    public interface IEntity<T>
    {
        T Entity { set; get; }
    }

    public interface IEntityDocument<T, R> : IEntity<T>
    {
        ObservableCollection<R> Rows { set; get; }
        ObservableCollection<R> DeletedRows { set; get; }
        bool DeletedRow(R row);
        bool DeletedRow(Guid id);
        bool DeletedRow(int id);
    }
}