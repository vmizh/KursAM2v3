using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Personal;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for PersonalPaysView.xaml
    /// </summary>
    public partial class PersonalPaysView : ILayout
    {
        private readonly PersonalNachRowViewModel myDataRows = new PersonalNachRowViewModel();
        private readonly PersonalNachViewModel myEmpRows = new PersonalNachViewModel();
        private List<NachEmployeeForPeriod> myPeriodList = new List<NachEmployeeForPeriod>();

        public PersonalPaysView()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Loaded += PersonalPaysView_Loaded;
            Closing += PersonalPaysView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void treePeriods_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            var ctx = DataContext as EmployeePayWindowViewModel;
            if (ctx == null) return;
            var item = e.NewItem as NachEmployeeForPeriod;
            if (item == null) return;
            ctx.UpdateDocumentsForPeriod(item);
        }

        private void PersonalPaysView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PersonalPaysView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void GridEmployee_OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
        }
    }
}
