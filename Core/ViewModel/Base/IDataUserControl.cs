using System.Windows;

namespace Core.ViewModel.Base
{
    public interface IDataUserControl
    {
        DependencyObject LayoutControl { get; }
    }
}