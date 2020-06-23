using System.Windows;
using System.Windows.Controls;
using Core.Menu;
using LayoutManager;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for KursBaseWindow.xaml
    /// </summary>
    public partial class KursBaseWindow : ILayout
    {
        public KursBaseWindow()
        {
            InitializeComponent();
            Loaded += KursBaseWindow_Loaded;
            Closing += KursBaseWindow_Closing;
        }

        private void KursBaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void KursBaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager = new global::LayoutManager.LayoutManager(LayoutManagerName,this);
            LayoutManager.Load();
        }

        public global::LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var menu = sender as Button;
            if (!(menu?.DataContext is MenuButtonInfo d) || d.SubMenu.Count == 0) return;
            d.MenuOpen(this);
        }
    }
}