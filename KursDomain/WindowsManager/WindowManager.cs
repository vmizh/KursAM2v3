using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using KursDomain;
using KursDomain.WindowsManager;

namespace Core.WindowsManager;

public class WindowManager : IWindowManager
{
    public static KursDialogResult YesNo = KursDialogResult.Yes | KursDialogResult.No;
    public static KursDialogResult YesNoCancel = KursDialogResult.Yes | KursDialogResult.No | KursDialogResult.Cancel;
    public static KursDialogResult SaveCancel = KursDialogResult.Save | KursDialogResult.Cancel;
    public static KursDialogResult SaveNotSaveCancel = KursDialogResult.Save | KursDialogResult.NotSave | KursDialogResult.Cancel;
    public static KursDialogResult Confirm = KursDialogResult.Confirm;

    public WindowManager()
    {
        DialogResultNames.Add(KursDialogResult.Yes,"Да");
        DialogResultNames.Add(KursDialogResult.Save,"Сохранить");
        DialogResultNames.Add(KursDialogResult.NotSave,"Не сохранять");
        DialogResultNames.Add(KursDialogResult.Cancel,"Отмена");
        DialogResultNames.Add(KursDialogResult.No,"Нет");
        DialogResultNames.Add(KursDialogResult.Confirm,"ОК");
    }

    [Flags]
    public enum KursDialogResult
    {
        Save = 1,
        NotSave = 2,
        Cancel = 4,
        Yes = 8,
        No = 16,
        Confirm = 32
    }

    public readonly Dictionary<KursDialogResult, string> DialogResultNames = new Dictionary<KursDialogResult, string>();

    public MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button)
    {
        return DXMessageBox.Show(messageBoxText, caption, button);
    }

    public void ShowMessageBox(string messageBoxText, string caption)
    {
        DXMessageBox.Show(messageBoxText, caption);
    }

    public KursDialogResult ShowKursDialog(string text,string titleText, Brush titleTextColor,  KursDialogResult result)
    { 
        var dlg = new KursDialogViewModel(text,titleText, titleTextColor, result, DialogResultNames);
        dlg.Show();
        return dlg.DialogResult;
    }

    public MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image, MessageBoxResult result, MessageBoxOptions options)
    {
        return WinUIMessageBox.Show(
            Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            messageBoxText,
            caption,
            button,
            image,
            result, options,
            FloatingMode.Adorner
        );
    }

    public MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button,
        MessageBoxImage image)
    {
        return DXMessageBox.Show(messageBoxText, caption, button, image);
    }

    public MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image)
    {
        return WinUIMessageBox.Show(
            Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            messageBoxText,
            caption,
            button,
            image,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner
        );
    }

    public MessageBoxResult ShowWinUIMessageBox(Window win, string messageBoxText, string caption,
        MessageBoxButton button, MessageBoxImage image)
    {
        return WinUIMessageBox.Show(
            win,
            messageBoxText,
            caption,
            button,
            image,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner
        );
    }

    public MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption, MessageBoxButton yesNo)
    {
        return WinUIMessageBox.Show(
            Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            messageBoxText,
            caption,
            MessageBoxButton.OK,
            MessageBoxImage.Information,
            MessageBoxResult.OK, MessageBoxOptions.None,
            FloatingMode.Adorner
        );
    }

    public static void ShowFunctionNotReleased(string text = null)
    {
        WinUIMessageBox.Show(
            Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            "Функция не реализована.",
            $"Системное сообщение. {text}",
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner
        );
    }

    public static void ShowError(Window win, Exception ex)
    {
        var errText = new StringBuilder(ex.Message);
        var inEx = ex;
        while (inEx.InnerException != null)
        {
            errText.Append("\n Внутрення ошибка:\n");
            errText.Append(inEx.InnerException.Message);
            inEx = inEx.InnerException;
        }

        using (var errCtx = GlobalOptions.KursSystem())
        {
            errCtx.Errors.Add(new Errors
            {
                Id = Guid.NewGuid(),
                DbId = GlobalOptions.DataBaseId,
                Host = Environment.MachineName,
                UserId = GlobalOptions.UserInfo.KursId,
                ErrorText = errText.ToString()
            });
        }

        //LoggerHelper.WriteError(ex);
        WinUIMessageBox.Show(win ?? Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            errText.ToString(),
            "Ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner);
    }

    public static void ShowMessage(Window win, string message, string caption, MessageBoxImage image)
    {
        WinUIMessageBox.Show(win ?? Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            message,
            caption,
            MessageBoxButton.OK,
            image,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner);
    }

    public static void ShowMessage(string message, string caption, MessageBoxImage image)
    {
        ShowMessage(null, message, caption, image);
    }

    private static string GetCallForExceptionThisMethod(MethodBase methodBase, Exception e)
    {
        var trace = new StackTrace(e);
        StackFrame previousFrame = null;

        // ReSharper disable once PossibleNullReferenceException
        foreach (var frame in trace.GetFrames())
        {
            if (frame.GetMethod() == methodBase) break;

            previousFrame = frame;
        }

        return previousFrame?.GetMethod().Name;
    }

    public static void ShowError(Exception ex, string ext_text = null, string caption = null)
    {
        var winActive = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
        var methodname = GetCallForExceptionThisMethod(null, ex);
        var errText = StringBuilder(ex);
        var ex1 = ex;
        while (ex1.InnerException != null)
        {
            ex1 = ex1.InnerException;
            errText.Append(ex1.Message + "\n");
            if (ex1.InnerException != null)
                errText.Append(ex1.InnerException.Message);
        }

        if (!string.IsNullOrEmpty(ext_text))
        {
            errText.Append("\n");
            errText.Append("---------\n");
            errText.Append(ext_text);
        }

        if (GlobalOptions.UserInfo != null)
            using (var errCtx = GlobalOptions.KursSystem())
            {
                errCtx.Errors.Add(new Errors
                {
                    Id = Guid.NewGuid(),
                    DbId = GlobalOptions.DataBaseId,
                    Host = Environment.MachineName,
                    UserId = GlobalOptions.UserInfo.KursId,
                    ErrorText = winActive + "/" + (methodname ?? "Метод не указан") + ": " + errText,
                    Moment = DateTime.Now
                });
                errCtx.SaveChanges();
            }

        if (Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive) != null)
            WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                errText.ToString(),
                caption ?? "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.None, MessageBoxOptions.None,
                FloatingMode.Adorner);
        else
            MessageBox.Show(errText.ToString(),
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
    }

    public static void ShowDBError(Exception ex)
    {
        if (ex.InnerException == null)
            WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                ex.Message,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.None, MessageBoxOptions.None,
                FloatingMode.Adorner);
        else
            ShowDBError(ex.InnerException);
    }

    private static StringBuilder StringBuilder(Exception ex)
    {
        var errText = new StringBuilder(ex.Message);
        return ex.InnerException == null
            ? errText
            : errText.Append($"{errText} \n Внутрення ошибка:\n {StringBuilder(ex.InnerException)}");
    }

    public MessageBoxResult ShowWinUIMessageBox(string messageBoxText, string caption)
    {
        return WinUIMessageBox.Show(
            Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
            messageBoxText,
            caption,
            MessageBoxButton.OK,
            MessageBoxImage.Exclamation,
            MessageBoxResult.None, MessageBoxOptions.None,
            FloatingMode.Adorner);
    }
}
