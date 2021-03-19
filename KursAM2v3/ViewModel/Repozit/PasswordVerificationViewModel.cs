using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.DataAccess;
using Helper;

namespace KursAM2.ViewModel.Repozit
{
    public class PasswordVerificationViewModel : RSWindowViewModelBase
    {
        public PasswordVerificationViewModel()
        {
            WindowName = "Проверка пароля";
        }
        public string OldPassword
        {
            get => myOldPassword;
            set
            {
                if (value == myOldPassword) return;
                myOldPassword = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsOkAllow()
        {
            if (OldPassword != null)
                return true;
            return false;
        }
        private string myOldPassword;

        public override void Ok(object obj)
        {
            if (string.IsNullOrWhiteSpace(OldPassword))
            {
                WindowManager.ShowMessage(null, "Пароль не может состоять из пробелов.",
                    "Ошибка",
                    MessageBoxImage.Error);
                return;
            }
            
            CloseWindow(Form);
        }

        public override void Cancel(object obj)
        {
            CloseWindow(Form);
        }
    }
}
