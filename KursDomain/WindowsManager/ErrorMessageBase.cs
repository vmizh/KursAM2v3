using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;

namespace KursDomain.WindowsManager.WindowsManager;

public abstract class ErrorMessageBase
{
    public DbContext EntityFrameworkContext { set; get; }
    public bool ShowInConsole { set; get; } = true;
    public bool ShowWindow { set; get; }
    public bool ShowUIWindow { set; get; }
    public bool ShowInDatabase { set; get; } = true;
    public string FormName { set; get; }

    public string Note { get; set; }

    public virtual void WriteErrorMessage(string messageBoxText, string note = null)
    {
        Console.WriteLine(messageBoxText);
        if (ShowInDatabase)
        {
            var id = Guid.NewGuid();
            EntityFrameworkContext.Database.ExecuteSqlCommand(
                "INSERT INTO ErrorMessages (Id, Host, UserName, ErrorText, Note, FormName, Moment) VALUES ({0}, {1}, {2},{3}, {4}, {5}, {6})",
                id, Environment.MachineName, GlobalOptions.UserInfo.NickName, messageBoxText, note ?? Note,
                FormName,
                DateTime.Now);
        }
    }

    public static string GetExceptionErrorText(Exception ex)
    {
        var errText = new StringBuilder(ex.Message);
        if (ex.InnerException != null)
        {
            var exx = ex.InnerException;
            while (exx != null)
            {
                errText.Append("\n Внутрення ошибка:\n");
                errText.Append(exx.Message);
                errText.Append("===================\n");
                exx = exx.InnerException;
            }
        }

        return errText.ToString();
    }

    public virtual void WriteErrorMessage(Exception ex, string note = null)
    {
        var errText = GetExceptionErrorText(ex);
        Console.WriteLine(errText);
        if (ShowInDatabase)
        {
            var id = Guid.NewGuid();
            EntityFrameworkContext.Database.ExecuteSqlCommand(
                "INSERT INTO ErrorMessages (Id, Host, UserName, ErrorText, Note, FormName, Moment) VALUES ({0}, {1}, {2},{3}, {4}, {5}, {6})",
                id, Environment.MachineName, GlobalOptions.UserInfo.NickName, errText, note ?? Note,
                FormName,
                DateTime.Now);
        }
    }

    public virtual void WriteErrorMessage(string messageBoxText, string caption, string note = null)
    {
        WriteErrorMessage(messageBoxText, note);
        if (ShowWindow) DXMessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        if (ShowUIWindow)
            WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                messageBoxText,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.None, MessageBoxOptions.None,
                FloatingMode.Adorner);
    }

    public virtual void WriteErrorMessage(Exception ex, string caption, string note = null)
    {
        WriteErrorMessage(GetExceptionErrorText(ex), caption, note);
    }
}
