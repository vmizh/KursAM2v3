using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Core;
using KursDomain.Menu;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for KursBaseWindow.xaml
    /// </summary>
    public partial class KursBaseWindow
    {
        public KursBaseWindow()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var menu = sender as Button;
            if (!(menu?.DataContext is MenuButtonInfo d) || d.SubMenu.Count == 0) return;
            d.MenuOpen(this);
        }
    }
}
