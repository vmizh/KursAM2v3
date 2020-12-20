using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

//using System.Data;

namespace Core.ViewModel.Base
{
    public interface IEntity<T>
    {
        bool IsAccessRight { set; get; }
        
        List<T> LoadList();

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