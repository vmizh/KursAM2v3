using System.ComponentModel;
using System.Windows;
using KursAM2.ViewModel.Personal;
using LayoutManager;

namespace KursAM2.View.Personal
{
    public partial class PayRollVedomost : ILayout
    {
        public PayRollVedomost()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += PayRollVedomost_Loaded;
            Closing += PayRollVedomost_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void PayRollVedomost_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PayRollVedomost_Loaded(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as PayRollVedomostWindowViewModel;
            if (ctx == null) return;
            ctx.CurrentEmployee = ctx.Employee;
            LayoutManager.Load();
        }

        private void MenuItemEmployeeExport_OnClick(object sender, RoutedEventArgs e)
        {
            tableViewEmp.ShowPrintPreview(Application.Current.MainWindow);
        }

        private void MenuItemNachExport_OnClick(object sender, RoutedEventArgs e)
        {
            tableViewNach.ShowPrintPreview(Application.Current.MainWindow);
        }
    }
}