using System.ComponentModel;
using System.Windows;
using KursAM2.View.Personal;
using KursAM2.ViewModel.Reconcilation;
using LayoutManager;

namespace KursAM2.View.Reconciliation
{
    /// <summary>
    ///     Interaction logic for ActOfReconciliation.xaml
    /// </summary>
    public partial class ActOfReconciliation : ILayout
    {
        public ActOfReconciliation()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            //DataContext = new AOFViewModel();
            Closing += ActOfReconciliation_Closing;
            Loaded += ActOfReconciliation_Loaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void ActOfReconciliation_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void ActOfReconciliation_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            gridResponsibleTableView.ShowPrintPreview(Application.Current.MainWindow);
        }

        private void MenuItem1_OnClick(object sender, RoutedEventArgs e)
        {
            gridCorporateTableView.ShowPrintPreview(Application.Current.MainWindow);
        }

        private void MenuItem2_OnClick(object sender, RoutedEventArgs e)
        {
            gridActsTableView.ShowPrintPreview(Application.Current.MainWindow);
        }

        private void SetRespocibleOnCorporates_OnClick(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as AOFViewModel;
            if (ctx?.ResponsibleSelectedCorporates == null || ctx.ResponsibleSelectedCorporates.Count == 0) return;
            var dlg = new DialogSelectPersona {Owner = this};
            dlg.ShowDialog();
            // ReSharper disable once RedundantJumpStatement
            if (dlg.DialogResult != null && !(bool) dlg.DialogResult) return;
            //TODO Обязательно поправить - связано с диалогом выбора сотрудника
            //var pers = dlg.EmpSelected;
            //if (pers == null || pers.Source.Count == 0) return;

            //foreach (var c in ctx.ResponsibleSelectedCorporates)
            //{
            //    GlobalOptions.Entities.Database.ExecuteSqlCommand(
            //        string.Format("UPDATE SD_43 set OTVETSTV_LICO = \'{0}\'" + " where doc_code = {1}",
            //            pers.Source.First().TabelNumber, c.DocCode));
            //}
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is AOFViewModel ctx)) return;
            ctx.Responsibles.Clear();
            ctx.PeriodChanged(null);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}