using System.Windows.Input;

namespace KursDomain.ViewModel.Base2;

public interface IFormSearchCommands
{
    ICommand CloseWindowCommand { get; }
    ICommand DocumentOpenCommand { get; }
    ICommand DocNewCommand { get; }
    ICommand DocNewEmptyCommand { get; }
    ICommand DocNewCopyRequisiteCommand { get; }
    ICommand DocNewCopyCommand { get; }
    ICommand RefreshDataCommand { get; }
    ICommand ResetLayoutCommand { get; }
}
