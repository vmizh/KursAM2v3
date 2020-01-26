using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace PeriodManager.View
{
    /// <summary>
    ///     Interaction logic for SetPeriodCloseForGroupView.xaml
    /// </summary>
    public partial class SetPeriodCloseForGroupView : ILayout
    {
        //private LayoutManager.LayoutManager myLayoutManager;

        public SetPeriodCloseForGroupView()
        {
            InitializeComponent();
            LayoutManager = new global::LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += SetPeriodCloseForGroupView_Closing;
            Loaded += SetPeriodCloseForGroupView_Loaded;
        }

        private void SetPeriodCloseForGroupView_Loaded(object sender, RoutedEventArgs e)
        {
            //var viewModelBase = DataContext as KursViewModelBase;
            //if (viewModelBase != null)
            //    myLayoutManager =
            //        new LayoutManagerNew(viewModelBase.LayoutName ?? "KursAM2.SetPeriodCloseForGroupViewModel.xml",
            //            this,
            //            new List<string>
            //            {
            //                "mainLayoutControl",
            //                "grid"
            //            });
            //myLayoutManager.RestoreLayout();
        }

        private void SetPeriodCloseForGroupView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }
    }
}