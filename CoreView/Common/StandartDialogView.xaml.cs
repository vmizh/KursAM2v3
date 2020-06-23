using System.ComponentModel;
using System.Windows;


namespace CoreView.Common
{
    /// <summary>
    ///     Interaction logic for StandartDialogView.xaml
    /// </summary>
    public partial class StandartDialogView
    {
        public StandartDialogView(LayoutManager.LayoutManager layoutManager) : this()
        {
            MyLayoutManager = layoutManager;
        }

        public StandartDialogView()
        {
            InitializeComponent();
            Loaded += DebitorCreditorView_Loaded;
            Closing += DebitorCreditorView_Closing;
        }

        public LayoutManager.LayoutManager MyLayoutManager { get; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (gridDocs.CurrentItem != null)
                DialogResult = true;
        }

        private void DebitorCreditorView_Closing(object sender, CancelEventArgs e)
        {
            MyLayoutManager.Save();
        }

        private void DebitorCreditorView_Loaded(object sender, RoutedEventArgs e)
        {
            MyLayoutManager.Load();
        }
    }
}