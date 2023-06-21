using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Personal;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for PayRollTypeReference.xaml
    /// </summary>
    public partial class PayRollTypeReference : ILayout
    {
        private readonly PayrollTypeWindowViewModel myData = new PayrollTypeWindowViewModel();

        public PayRollTypeReference()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            //DataContext = myData;
            Loaded += PayRollTypeReference_Loaded;
            Closing += PayRollTypeReference_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }

        private void PayRollTypeReference_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PayRollTypeReference_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
