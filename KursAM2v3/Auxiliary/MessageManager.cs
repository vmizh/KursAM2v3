using System;
using System.Text;
using DevExpress.Mvvm;

namespace KursAM2.Auxiliary
{
    public class MessageManager
    {
        public static void ErrorShow(IDialogService service, Exception ex)
        {
            if (service != null)
            {
                var errText = new StringBuilder(ex.Message);
                var ex1 = ex;
                while (ex1.InnerException != null)
                {
                    ex1 = ex1.InnerException;
                    errText.Append(ex1.Message + "\n");
                    if (ex1.InnerException != null)
                        errText.Append(ex1.InnerException.Message);
                }
                var vm = new ErrorServiceViewModel
                {
                    dialogServiceText = errText.ToString()
                };
                service.ShowDialog(MessageButton.YesNo, "Ошибка", vm);
            }
        }

        public static MessageResult StringMessageShow(IDialogService service, string text, string caption = "Сообщение")
        {
            if (service != null)
            {
                var vm = new ErrorServiceViewModel
                {
                    dialogServiceText = text
                };
                return service.ShowDialog(MessageButton.YesNo, caption, vm);
            }

            return MessageResult.None;
        }
    }
}