using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI.Navigation;
using KursAM2.View.Behaviors;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for ManagementBalansCompareView.xaml
    /// </summary>
    public partial class ManagementBalansCompareView : ILayout
    {
        public ManagementBalansCompareView()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Closing += ManagementBalansCompareView_Closing;
            Loaded += ManagementBalansCompareView_Loaded;
        }

        public object CurrentDetailView { set; get; }
        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void ManagementBalansCompareView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void ManagementBalansCompareView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //throw new System.NotImplementedException();
        }


        public void NavigateTo(Type view)
        {
            INavigator navigator = new FrameBalansNavigator();
            navigator.Navigate(view);
        }

        public void ResetCurrencyFields()
        {
        }

        private void NavigationFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            CurrentDetailView = e.Content;
        }
    }
}
