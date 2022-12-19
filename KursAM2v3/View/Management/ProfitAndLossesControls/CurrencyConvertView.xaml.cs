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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LayoutManager;

namespace KursAM2.View.Management.ProfitAndLossesControls
{
    /// <summary>
    /// Interaction logic for CurrencyConvertView.xaml
    /// </summary>
    public partial class CurrencyConvertView : ILayout
    {
        public CurrencyConvertView()
        {
            InitializeComponent();
            Loaded += CurrencyConvertViewUI_Loaded;
            Unloaded += BalansCompareFinanseUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager("KursAM2.View.Management.ProfitAndLossesControls.CurrencyConvertView",
                GridControlCurrencyConvert);
        }

        private void BalansCompareFinanseUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void CurrencyConvertViewUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load(false);

        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }
    }
}
