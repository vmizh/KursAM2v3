using System;
using System.Collections.ObjectModel;

namespace Core.ViewModel.Base
{
    public interface ISearchWindowViewModel<T> where T : class
    {
        DateTime DateStart { set; get; }
        DateTime DateEnd { set; get; }
        ObservableCollection<T> Documents { set; get; }
        ObservableCollection<T> SelectDocuments { set; get; }

        T CurrentDocument { set; get; }
    }
}