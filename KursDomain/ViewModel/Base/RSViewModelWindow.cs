using System;
using System.Text;
using System.Windows;
using DevExpress.Xpf.WindowsUI;

namespace Core.ViewModel.Base
{
    [Obsolete("��������. ����������� ����� RSWindowViewModelBase")]
    public abstract class RSViewModelWindow : RSViewModelBase
    {
        public virtual Window Form { set; get; }

        public static void ShowError(Exception ex)
        {
            var errText = new StringBuilder(ex.Message);
            if (ex.InnerException != null)
            {
                errText.Append("\n ��������� ������:\n");
                errText.Append(ex.InnerException.Message);
            }

            WinUIMessageBox.Show(errText.ToString(), "������");
        }

        protected virtual void CloseWindow(object form)
        {
            var frm = form as Window;
            frm?.Close();
        }

        protected virtual void ShowWindow()
        {
            Form.Show();
        }

        public abstract void RefreshData();
        public abstract void SaveData();
    }
}
