using System;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;

namespace Core.WindowsManager
{
    public class WindowManager : IWindowManager
    {
        public MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button)
        {
            return DXMessageBox.Show(messageBoxText, caption, button);
        }

        public void ShowMessageBox(string messageBoxText, string caption)
        {
            DXMessageBox.Show(messageBoxText, caption);
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

        public static void ShowFunctionNotReleased()
        {
            WinUIMessageBox.Show(
                Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                "������� �� �����������.",
                "��������� ���������.",
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
                errText.Append("\n ��������� ������:\n");
                errText.Append(inEx.InnerException.Message);
                inEx = inEx.InnerException;
            }
            using (var errCtx = GlobalOptions.KursSystem())
            {
                errCtx.Errors.Add(new Data.Errors
                {
                    Id = Guid.NewGuid(),
                    DbId = GlobalOptions.DataBaseId,
                    Host = Environment.MachineName,
                    UserId = GlobalOptions.UserInfo.KursId,
                    ErrorText = errText.ToString()
                });
            }
            WinUIMessageBox.Show(win ?? Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                errText.ToString(),
                "������",
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

        public static void ShowError(Exception ex)
        {
            var errText = StringBuilder(ex);
            if (ex.InnerException != null)
            {
                var ex1 = ex.InnerException;
                errText.Append(ex1.Message + "\n");
                if (ex1.InnerException != null)
                    errText.Append(ex1.InnerException.Message);
            }
            using (var errCtx = GlobalOptions.KursSystem())
            {
                errCtx.Errors.Add(new Data.Errors
                {
                    Id = Guid.NewGuid(),
                    DbId = GlobalOptions.DataBaseId,
                    Host = Environment.MachineName,
                    UserId = GlobalOptions.UserInfo.KursId,
                    ErrorText = errText.ToString(),
                    Moment = DateTime.Now
                });
                errCtx.SaveChanges();
            }
            if (Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive) != null)
                WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                    errText.ToString(),
                    "������",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.None, MessageBoxOptions.None,
                    FloatingMode.Adorner);
            else
                MessageBox.Show(errText.ToString(),
                    "������",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }

        public static void ShowDBError(Exception ex)
        {
            if (ex.InnerException == null)
                WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                    ex.Message,
                    "������",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.None, MessageBoxOptions.None,
                    FloatingMode.Adorner);
            else
                ShowDBError(ex.InnerException);
        }

        public static void ShowError(Exception ex, string caption)
        {
            var errText = StringBuilder(ex);
            WinUIMessageBox.Show(Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                errText.ToString(),
                caption,
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.None, MessageBoxOptions.None,
                FloatingMode.Adorner);
        }

        private static StringBuilder StringBuilder(Exception ex)
        {
            var errText = new StringBuilder(ex.Message);
            return ex.InnerException == null
                ? errText
                : errText.Append($"{errText} \n ��������� ������:\n {StringBuilder(ex.InnerException)}");
        }

        public void ShowWinUIMessageBox(string messageBoxText, string caption)
        {
            WinUIMessageBox.Show(
                Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                messageBoxText,
                caption,
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation,
                MessageBoxResult.None, MessageBoxOptions.None,
                FloatingMode.Adorner);
        }
    }
}