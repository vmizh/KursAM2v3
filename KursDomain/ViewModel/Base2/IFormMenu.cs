using KursDomain.Menu;
using System.Collections.ObjectModel;

namespace KursDomain.ViewModel.Base2;

public interface IFormMenu
{
    ObservableCollection<MenuButtonInfo> RightMenuBar { set; get; }

    ObservableCollection<MenuButtonInfo> LeftMenuBar { set; get; }
    
}
