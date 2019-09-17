using System.Diagnostics;
using System.Windows;

namespace KursStart
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            KursStart();
        }



        private void KursStart()
        {
            Process.Start("cmd");
            Close();
        }
    }
}