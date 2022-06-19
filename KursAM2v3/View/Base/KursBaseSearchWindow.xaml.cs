using System.Windows;
using System.Windows.Controls;
using Core.Menu;
using DevExpress.Xpf.Core;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for KursBaseWindow.xaml
    /// </summary>
    public partial class KursBaseSearchWindow
    {
        public KursBaseSearchWindow()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        //public ILayoutSerializationService LayoutSerializationService
        //    => this.GetService<ILayoutSerializationService>();

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var menu = sender as Button;
            if (!(menu?.DataContext is MenuButtonInfo d) || d.SubMenu.Count == 0) return;
            d.MenuOpen(this);
        }
    }
}
