using System.Windows.Input;

namespace KursDomain.ViewModel.Base2;

public interface IDialogCommands
{
    ICommand OkCommand { get; }
    ICommand CancelCommand { get; }
    ICommand ResetLayoutCommand { get; }

}
