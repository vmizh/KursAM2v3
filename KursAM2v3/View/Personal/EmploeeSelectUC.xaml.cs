using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Personal;
using KursDomain.References;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for EmploeeSelectUC.xaml
    /// </summary>
    public partial class EmploeeSelectUC
    {
        public EmploeeSelectUC()
        {
            InitializeComponent(); 
            
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
            var item = gridSelect.CurrentItem as Employee;
            if (item != null)
                RemoveFromSelected(item);
        }

        private void tableViewSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = gridSelect.CurrentItem as Employee;
            if (item != null)
                RemoveFromSelected(item);
        }
    }
}
