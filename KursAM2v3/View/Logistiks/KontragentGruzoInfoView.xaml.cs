using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Logistiks;
using KursDomain.References;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for KontragentGruzoInfoView.xaml
    /// </summary>
    public partial class KontragentGruzoInfoView
    {
        public KontragentGruzoInfoView()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GridControl_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            var ctx = DataContext as KontragentGruzoInfoWindowViewModel;
            if (ctx == null) return;
            var row = e.NewItem as Kontragent;
            if (row == null) return;
            ctx.LoadActualGruzoInfo(row.DocCode);
            gridGruzo.RefreshData();
        }
    }
}
