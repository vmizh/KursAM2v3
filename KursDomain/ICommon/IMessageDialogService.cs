namespace KursDomain.ICommon;

public interface IMessageDialogService
{
    MessageDialogResult ShowOkCancelDialog(string text, string title);
    void ShowInfoDialog(string text);
}
