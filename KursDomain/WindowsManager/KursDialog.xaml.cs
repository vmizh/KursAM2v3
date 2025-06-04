using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;

namespace KursDomain.WindowsManager.WindowsManager;

/// <summary>
///     Interaction logic for KursDialog.xaml
/// </summary>
public partial class KursDialog : ThemedWindow
{
    public KursDialog()
    {
        InitializeComponent();
        RoundCorners = true;
        Loaded += KursDialog_Loaded;
    }

    private void KursDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Set the default button to the "OK" button
        if (DataContext is KursDialogViewModel vm) FocusManager.SetFocusedElement(this, vm.Buttons[0]);
    }

    private void ThemedWindow_Loaded(object sender, RoutedEventArgs e)
    {
    }
}
