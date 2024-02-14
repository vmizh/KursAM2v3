using System.Windows.Input;

namespace KursDomain.ViewModel.Base2;

public interface IFormCommands
{
    ICommand CloseWindowCommand { get; }
    ICommand DocumentOpenCommand { get; }
    ICommand PrintCommand { get; }
    ICommand DocNewCommand { get; }
    ICommand DocNewEmptyCommand { get; }
    ICommand DocNewCopyRequisiteCommand { get; }
    ICommand DocNewCopyCommand { get; }
    ICommand RefreshDataCommand { get; }
    ICommand SaveDataCommand { get; }
    ICommand ResetLayoutCommand { get; }
    ICommand ShowHistoryCommand { get; }
    ICommand DoсDeleteCommand { get; }
    ICommand UndoCommand { get; }
}
