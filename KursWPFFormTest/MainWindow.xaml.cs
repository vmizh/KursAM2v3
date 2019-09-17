using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using Core;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Invoices;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Logistiks.Warehouse;
using LayoutManager;

namespace KursWPFFormTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "VMIZHPC",
                InitialCatalog = "EcoOndol",
                UserID = "sysadm",
                Password = "19655691"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            GlobalOptions.UserInfo = new Helper.User
            {
                Name = "sysadm",
                NickName = "sysadm"
            };
            GlobalOptions.MainReferences = new MainReferences();
            GlobalOptions.MainReferences.Reset();
            //while (!MainReferences.IsReferenceLoadComplete)
            //{
            //}
            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),

            };
        }

        private void InvoiceFormButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new InvoiceForm();
            ((ILayout)form).LayoutManager.Load();
            var ctx = new ProviderWindowViewModel {Document = InvoicesManager.NewProvider()};
            form.Show();
            
            form.DataContext = ctx;
            
        }

        private void StorageOrderInButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new OrderInView();
            ((ILayout)form).LayoutManager.Load();
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),form.Name));
            form.Show();

            form.DataContext = ctx;
        }

        private void InvoiceClientButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new InvoiceClient();
            ((ILayout)form).LayoutManager.Load();
            var ctx = new ClientWindowViewModel { Document = InvoicesManager.NewClient(), Form = form};
            form.Show();

            form.DataContext = ctx;
        }

        private void StorageOrderOutButton_Click(object sender, RoutedEventArgs e)
        { 
            var form = new OrderOutView();
            var erManager = new StandartErrorManager(GlobalOptions.GetEntities(), form.Name);
            var orderManager = new WarehouseManager(erManager);
            ((ILayout) form).LayoutManager.Load();
            var ctx = new OrderOutWindowViewModel(erManager)
                {Document = orderManager.NewOrderOut()};
            form.Show();

            form.DataContext = ctx;
        }
    }
}