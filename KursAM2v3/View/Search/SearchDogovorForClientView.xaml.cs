using System.ComponentModel;
using System.Windows;

namespace KursAM2.View.Search
{
    /// <summary>
    ///     Interaction logic for SearchDogovorForClientView.xaml
    /// </summary>
    public partial class SearchDogovorForClientView
    {
        public SearchDogovorForClientView()
        {
            InitializeComponent();
            Loaded += SearchBaseView_Loaded;
            Closing += SearchBaseView_Closing;
        }

        private void SearchBaseView_Closing(object sender, CancelEventArgs e)
        {
        }

        private void SearchBaseView_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}