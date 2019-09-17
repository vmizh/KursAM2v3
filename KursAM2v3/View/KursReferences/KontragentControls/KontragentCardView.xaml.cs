using System.Diagnostics.CodeAnalysis;
using System.Windows;
using LayoutManager;
using ILayout = DevExpress.Xpf.Charts.Native.ILayout;

namespace KursAM2.View.KursReferences.KontragentControls
{
    /// <summary>
    ///     Interaction logic for KontragentCardView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public partial class KontragentCardView : ILayout
    {
        public KontragentCardView()
        {
            InitializeComponent();
            SizeChanged += delegate { SetSizeOfFrame(); };
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += KontragentCardView_OnLoaded;
            Loaded += MutualAccountingSearchView_Loaded;
            Unloaded += MutualAccountingSearchView_Unloaded;
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public bool Visible { get; }
        public Rect Bounds { get; }
        public Rect ClipBounds { get; }
        public Size Size { get; }
        public Point Location { get; }
        public double Angle { get; }

        private void MutualAccountingSearchView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void MutualAccountingSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void SetSizeOfFrame()
        {
            var n = tileBarMenu.ActualWidth + 120;
            if(ActualWidth - n > 0) 
                frame.Width = ActualWidth - n;
        }

        private void KontragentCardView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ChangedTab("mainTab");
        }

        public void ChangedTab(string tabName)
        {
            switch (tabName)
            {
                case nameof(mainTab):
                    frame.Navigate(new KontragentMainUserControl());
                    break;
                case "contactTab":
                    frame.Navigate(new KontragentContactUserControl());
                    break;
                case "uchetTab":
                    frame.Navigate(new KontragentUchetInfoUserControl());
                    break;
                case "personaTab":
                    frame.Navigate(new KontragentPersonaUserControl());
                    break;
                case "bankTab":
                    frame.Navigate(new KontragentBankUserControl());
                    break;
                case "gruzoTab":
                    frame.Navigate(new KontragentGruzoUserControl());
                    break;
            }
        }
    }
}