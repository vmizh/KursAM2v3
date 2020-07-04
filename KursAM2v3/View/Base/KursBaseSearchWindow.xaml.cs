using System.Windows;
using System.Windows.Controls;
using Core.Menu;

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
            //LayoutManagerName = layoutName;
            //LayoutManager = new global::Helper.LayoutManager(LayoutSerializationService, 
            //    LayoutManagerName, ctrlName);
            //Loaded += KursBaseWindow_Loaded;
            //Closing += KursBaseWindow_Closing;
        }

        //public ILayoutSerializationService LayoutSerializationService
        //    => this.GetService<ILayoutSerializationService>();
        public string LayoutManagerName { get; set; }

        //private void KursBaseWindow_Closing(object sender, CancelEventArgs e)
        //{
        //    LayoutManager.Save();
        //}

        //private void KursBaseWindow_Loaded(object sender, RoutedEventArgs e)
        //{

        //    LayoutManager.Load();
        //}

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var menu = sender as Button;
            if (!(menu?.DataContext is MenuButtonInfo d) || d.SubMenu.Count == 0) return;
            d.MenuOpen(this);
        }
    }
}