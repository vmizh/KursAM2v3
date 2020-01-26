using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for SkladOstatki.xaml
    /// </summary>
    public partial class SkladOstatki : ILayout
    {
        public SkladOstatki()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += (o, e) => { LayoutManager.Save(); };
            Loaded += (operGridControl, e) => { LayoutManager.Load(); };
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }
    }
}