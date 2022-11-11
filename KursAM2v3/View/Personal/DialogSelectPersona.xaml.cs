using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Personal;
using KursDomain.References;

namespace KursAM2.View.Personal
{
    public partial class DialogSelectPersona
    {
        public DialogSelectPersona()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            Loaded += DialogSelectPersona_Loaded;
            Closing += DialogSelectPersona_Closing;
        }

        private void DialogSelectPersona_Closing(object sender, CancelEventArgs e)
        {
        }

        private void DialogSelectPersona_Loaded(object sender, RoutedEventArgs e)
        {
        }

        
        private void tableViewEmployee_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridEmp.CurrentItem is Employee item)
                AddToSelect(item);
        }

        private void EmployeeAddToSelect_OnClick(object sender, RoutedEventArgs e)
        {
            if (gridEmp.CurrentItem is Employee item)
                AddToSelect(item);
        }

        private void AddToSelect(Employee item)
        {
            var ctx = DataContext as PersonalDialogSelectWindowViewModel;
            ctx?.AddToSelected(item);
        }

        private void RemoveFromSelected(Employee item)
        {
            var ctx = DataContext as PersonalDialogSelectWindowViewModel;
            ctx?.RemoveFromSelected(item);
        }

        private void RemoveFromSelect_OnClick(object sender, RoutedEventArgs e)
        {
            if (gridSelect.CurrentItem is Employee item)
                RemoveFromSelected(item);
        }

        private void tableViewSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridSelect.CurrentItem is Employee item)
                RemoveFromSelected(item);
        }

        private void BarButtonItemRefresh_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var ctx = DataContext as PersonalDialogSelectWindowViewModel;
            ctx?.RefreshData(null);
        }

        private void BarButtonItemOK_OnItemClick(object sender, ItemClickEventArgs e)
        {
            DialogResult = true;
        }

        private void BarButtonItemCancel_OnItemClick(object sender, ItemClickEventArgs e)
        {
            DialogResult = false;
        }
    }
}
