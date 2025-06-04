using System.Windows;

namespace CKursDomain.WindowsManager.WindowsManager;

public interface IWindowManager
{
    MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button);
    void ShowMessageBox(string messageBoxText, string caption);

    // ReSharper disable once InconsistentNaming
    MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image, MessageBoxResult result, MessageBoxOptions options);
}
