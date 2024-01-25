using System.Windows.Input;

namespace KursDomain.ViewModel.Base2;

public interface IDialogOperation
{
    ICommand OkCommand { get; }
    ICommand CancelCommand { get; }
    ICommand RefreshDataCommand { get; }
    ICommand ResetLayoutCommand { get; }
    ICommand DoneCommand { get; }
    ICommand CloseWindowCommand { get; }
}
