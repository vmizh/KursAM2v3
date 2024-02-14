using DevExpress.Xpf.Core;
using KursDomain.ViewModel.Base2;

namespace KursDomain.View.Base;

/// <summary>
///     Interaction logic for BaseDialogWindow.xaml
/// </summary>
public partial class BaseDialogWindow : ThemedWindow
{
    public BaseDialogWindow()
    {
        InitializeComponent();
        Loaded += BaseDialogWindow_Loaded;
    }

    private async void BaseDialogWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is IInitialize init)
        {
            await init.InitializingAsync();
        } 
    }
}
