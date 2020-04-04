using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Распределение накладных расходов на закупки товаров
    /// </summary>
    public partial class PurchaseInvoicesOverheadsView : ILayout
    {
        public PurchaseInvoicesOverheadsView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += PurchaseInvoicesOverheadsView_Loaded;
            Closing += PurchaseInvoicesOverheadsView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
 
        private void PurchaseInvoicesOverheadsView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PurchaseInvoicesOverheadsView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = sender as FrameworkElement;
            if (src == null) return;
            switch (src.Name)
            {
                case "Plateji":
                    groupKontrInfo.SelectedTabIndex = 0;
                    break;
                case "LicSchet":
                    groupKontrInfo.SelectedTabIndex = 1;
                    break;
            }
            e.Handled = true;
        }
    }
}