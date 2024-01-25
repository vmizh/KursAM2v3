using System.Windows.Input;

namespace KursDomain.ViewModel.Base2;

public interface ILayout
{
    string LayoutName { set; get; }

    public ICommand OnWindowClosingCommand { get; }
    public ICommand OnInitializeCommand { get; }
    public ICommand OnWindowLoadedCommand { get; }
}
