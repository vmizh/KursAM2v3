using System.Windows;
using System.Windows.Controls;
using KursDomain.Menu;

namespace KursDomain.Control
{
    /// <summary>
    ///     Interaction logic for WindowMenu.xaml
    /// </summary>
    public partial class WindowMenu
    {
        public WindowMenu()
        {
            InitializeComponent();
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var menu = sender as Button;
            if (!(menu?.DataContext is MenuButtonInfo d) || d.SubMenu.Count == 0) return;
            d.MenuOpen(this);
        }
    }
}
