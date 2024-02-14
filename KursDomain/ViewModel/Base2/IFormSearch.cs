using System;
using System.Collections.ObjectModel;

namespace KursDomain.ViewModel.Base2;

public interface IFormSearch<T> : IFormSearchCommands
{
  
    string WindowName { get; set; }

    void Show();
    void Close();

    bool IsCanSearch { get; set; }
    string SearchText { get; set; }

    DateTime DateStart { get; set; }
    DateTime DateEnd { get; set; }

    ObservableCollection<T> Documents { get; set; }
    ObservableCollection<T> SelectedDocuments { get; set; }

    T CurrentDocument { get; set; }

}
