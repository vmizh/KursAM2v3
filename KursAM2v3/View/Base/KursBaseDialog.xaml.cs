using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for KursBaseDialog.xaml
    /// </summary>
    public partial class KursBaseDialog : ILayout
    {
        public KursBaseDialog()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            DataContextChanged += KursBaseDialog_DataContextChanged;
            Closing += KursBaseDialog_Closing;
        }

        private void KursBaseDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutManager?.Save();
        }

        private void KursBaseDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                LayoutManagerName = DataContext.GetType() + "_Dialog";
                LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),LayoutManagerName,this,null,null);
                LayoutManager.Load();
            }
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            throw new System.NotImplementedException();
        }
    }
}
