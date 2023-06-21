using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Logistiks.UC
{
    /// <summary>
    ///     Interaction logic for SelectNomenklSimpleUC.xaml
    /// </summary>
    public partial class SelectNomenklSimpleUC : ILayout
    {
        public SelectNomenklSimpleUC()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, gridControlNomenkl);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void ButtonEdit_KeyDown(object sender, KeyEventArgs e)
        {
            var ctx = DataContext as NomenklSelectedSimpleDialogViewModel;
            if (ctx == null) return;
            if (e.Key == Key.Enter)
                ctx.SearchExecute(null);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}
