using System.Windows;
using Core.ViewModel.Base;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for SelectDialogView.xaml
    /// </summary>
    public partial class SelectDialogView2
    {
        public SelectDialogView2()
        {
            InitializeComponent();
            //DataContextChanged += SelectDialogView_DataContextChanged;
            //Closing += SelectDialogView_Closing;
        }

        //public LayoutManager.LayoutManager LayoutManager { get; set; }
        //public string LayoutManagerName { get; set; }
        //public void SaveLayout()
        //{
        //    LayoutManager.Save();
        //}

        //private void SelectDialogView_Closing(object sender, CancelEventArgs e)
        //{
        //    LayoutManager.Save();
        //}

        //private void SelectDialogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = DataContext as IDataUserControl;
        //    LayoutManager = new LayoutManager.LayoutManager($"{GetType().Name}.{DataContext.GetType()}", this,
        //        ctrl?.LayoutControl);
        //    LayoutManager.Load();
        //}

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                DialogResult = true;
                ctx.DialogResult = true;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                DialogResult = false;
                ctx.DialogResult = false;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}