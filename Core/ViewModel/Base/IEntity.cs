using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

//using System.Data;

namespace Core.ViewModel.Base
{
    public interface IEntity<T>
    {
        bool IsAccessRight { set; get; }
        //T Entity { set; get; }
        //T Load(decimal dc);
        //T Load(Guid id);
        //void Save(T doc);

        //void Save();
        //void Delete();
        //void Delete(Guid id);
        //void Delete(decimal dc);

        //void UpdateFrom(T ent);
        //void UpdateTo(T ent);
        //T DefaultValue();
        List<T> LoadList();

        //EntityLoadCodition LoadCondition { set; get; }
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