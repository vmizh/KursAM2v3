using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Prism.Events;

namespace KursDomain.ViewModel.Base2;

public interface IDialog<T> : IDialogCommands
{
    UserControl FormControl { set; get; }
    string Title { get; set; }

    Task InitializingAsync();

    ICollection<T> Items { set; get; }

    T CurrentItem { get; set; }

    IEventAggregator EventAggregator { get; }
}
