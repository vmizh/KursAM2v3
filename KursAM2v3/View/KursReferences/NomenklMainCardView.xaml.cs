using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using KursAM2.ViewModel.Reference.Nomenkl;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for NomenklMainCardView.xaml
    /// </summary>
    public partial class NomenklMainCardView : ILayout
    {
        public NomenklMainCardView()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += NomenklMainCardView_Loaded;
            Closing += NomenklMainCardView_Closing;
            DataContextChanged += NomenklMainCardView_DataContextChanged;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void NomenklMainCardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //var ctx = DataContext as MainCardWindowViewModel;
            //if (ctx?.NomenklCategoryDC != null)
            //{
            //    ctx.NomenklCategory =
            //        ctx.NomenklCategoryCollection.Single(_ => _.DocCode == ctx.NomenklCategoryDC);
            //}
        }

        private void NomenklMainCardView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void NomenklMainCardView_Loaded(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as MainCardWindowViewModel;
            if (ctx == null) return;
            ctx.RaisePropertyChanged("NomenklMain");
            LayoutManager.Load();
        }

        private void TextEdit_Validate(object sender, ValidationEventArgs e)
        {
            e.IsValid = !string.IsNullOrEmpty(((string) e.Value)?.Replace(" ", string.Empty));
            if (!e.IsValid)
                e.ErrorContent = @"Поле не может быть пустым";
        }
    }
}
