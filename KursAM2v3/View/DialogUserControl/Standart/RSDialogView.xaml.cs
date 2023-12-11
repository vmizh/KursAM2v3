using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Core;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;
using KursAM2.ViewModel.Finance.DistributeNaklad;
using KursDomain;


namespace KursAM2.View.DialogUserControl.Standart
{
    /// <summary>
    /// Interaction logic for RSDialogView.xaml
    /// </summary>
    public partial class RSDialogView : ThemedWindow
    {
        public void saveLog(string str)
        {
            using (var context = GlobalOptions.KursSystem())
            {
                context.Errors.Add(new Data.Errors
                {
                    Id = Guid.NewGuid(),
                    DbId = GlobalOptions.DataBaseId,
                    Host = Environment.MachineName,
                    UserId = GlobalOptions.UserInfo.KursId,
                    ErrorText = "Log RSDialogView -> " + str,
                    Moment = DateTime.Now
                });
                context.SaveChanges();
            }
        }

        public RSDialogView()
        {
            InitializeComponent(); 
            Activated += ThemedWindow_Activated;
            Initialized += RSDialogView_Initialized;
            LayoutUpdated += RSDialogView_LayoutUpdated;
            Loaded += RSDialogView_Loaded;
        }

        private void RSDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            saveLog("RSDialogView Loaded");
        }

        private void RSDialogView_LayoutUpdated(object sender, EventArgs e)
        {
            saveLog("RSDialogView LayoutUpdated");
        }

        private void RSDialogView_Initialized(object sender, EventArgs e)
        {
            saveLog("RSDialogView Initialized");
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void ThemedWindow_Activated(object sender, EventArgs e)
        {
            saveLog("RSDialogView activated");
        }
    }
}
