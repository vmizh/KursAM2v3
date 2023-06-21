using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomenklInnerMove.xaml
    /// </summary>
    public partial class NomenklInnerMove : ILayout
    {
        public NomenklInnerMove()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Closing += (o, e) => { LayoutManager.Save(); };
            Loaded += (operGridControl, e) => { LayoutManager.Load(); };
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }
    }
}
