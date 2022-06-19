using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
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
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void tableViewEmployee_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridEmp.CurrentItem is PersonaDialogSelect item)
                AddToSelect(item);
        }

        private void EmployeeAddToSelect_OnClick(object sender, RoutedEventArgs e)
        {
            if (gridEmp.CurrentItem is PersonaDialogSelect item)
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
