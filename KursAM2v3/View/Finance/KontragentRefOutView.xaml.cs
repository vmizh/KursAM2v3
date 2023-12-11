using System;
using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;

namespace KursAM2.View.Finance
{
    /// <summary>
    ///     Interaction logic for KontragentRefOutView.xaml
    /// </summary>
    public partial class KontragentRefOutView
    {
        private readonly string LayoutFileName =
            $"{Environment.CurrentDirectory}\\Layout\\{"KontragentRefOutView"}.xml";

        public KontragentRefOutView()
        {
            InitializeComponent(); 
            
            Loaded += SearchBaseView_Loaded;
            Closing += SearchBaseView_Closing;
        }

        private void SearchBaseView_Closing(object sender, CancelEventArgs e)
        {
        }

        private void SearchBaseView_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
