using System.Windows.Controls;
using Prism.Events;

namespace KursDomain.ViewModel.Base2;

public interface IForm : IFormCommands
{
    UserControl FormControl { set; get; }
    string Title { get; set; }
    bool HasChanges { set; get; }

    void Show();
    void Close();

    void LoadReferncesAsync();
}


