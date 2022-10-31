using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.ViewModel.Base
{
    public interface IViewModel<T>
    {
        ObservableCollection<T> Source { set; get; }
        ObservableCollection<T> SourceAll { set; get; }
        List<T> DeletedItems { set; get; }
    }
}