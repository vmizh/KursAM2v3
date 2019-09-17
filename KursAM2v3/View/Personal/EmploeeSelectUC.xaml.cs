using System.Windows;
using System.Windows.Input;
using KursAM2.ViewModel.Personal;

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
            var item = gridEmp.CurrentItem as PersonaDialogSelect;
            if (item != null)
                AddToSelect(item);
        }

        private void EmployeeAddToSelect_OnClick(object sender, RoutedEventArgs e)
        {
            var item = gridEmp.CurrentItem as PersonaDialogSelect;
            if (item != null)
                AddToSelect(item);
        }

        private void AddToSelect(PersonaDialogSelect item)
        {
            var ctx = DataContext as PersonalDialogSelectWindowViewModel;
            ctx?.AddToSelected(item);
        }

        private void RemoveFromSelected(PersonaDialogSelect item)
        {
            var ctx = DataContext as PersonalDialogSelectWindowViewModel;
            ctx?.RemoveFromSelected(item);
        }

        private void RemoveFromSelect_OnClick(object sender, RoutedEventArgs e)
        {
            var item = gridSelect.CurrentItem as PersonaDialogSelect;
            if (item != null)
                RemoveFromSelected(item);
        }

        private void tableViewSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = gridSelect.CurrentItem as PersonaDialogSelect;
            if (item != null)
                RemoveFromSelected(item);
        }
    }
}