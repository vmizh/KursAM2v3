using System;
using System.Text;
using System.Windows;
using Core.WindowsManager;
using DevExpress.Xpf.Core;

namespace Core
{
    public static class Errors
    {
        public static void ShowError(Exception ex)
        {
            var WindowManager = new WindowManager();
            var errText = new StringBuilder(ex.Message);
            if (ex.InnerException != null)
            {
                errText.Append("\n Внутрення ошибка:\n");
                errText.Append(ex.InnerException.Message);
            }
            WindowManager.ShowWinUIMessageBox(errText.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK, MessageBoxOptions.None);
            DXMessageBox.Show(errText.ToString(), "Ошибка");
        }
    }
}