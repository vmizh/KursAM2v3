using System;
using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for ActOfReconciliation.xaml
    /// </summary>
    public partial class ShopRentabelnost : ILayout
    {
        private readonly string LayoutFileName = $"{Environment.CurrentDirectory}\\Layout\\{"ShopRentabelnost"}.xml";

        public ShopRentabelnost()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            //DataContext = new ShopRentabelnostViewModel();
            Closing += ShopRentabelnost_Closing;
            Loaded += ShopRentabelnost_Loaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void ShopRentabelnost_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void ShopRentabelnost_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}
