using System.Windows;
using KursDomain.ICommon;

namespace KursDomain.Services;

public class MessageDialogService : IMessageDialogService
{
    public MessageDialogResult ShowOkCancelDialog(string text,string title)
    {
        var result = MessageBox.Show(text, title, MessageBoxButton.OKCancel);
        return result == MessageBoxResult.OK
            ? MessageDialogResult.OK
            : MessageDialogResult.Cancel;
    }

    public void ShowInfoDialog(string text)
    {
        MessageBox.Show(text, "Info");
    }
}
