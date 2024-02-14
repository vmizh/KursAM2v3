using DevExpress.Mvvm.DataAnnotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KursDomain.ViewModel.Base2;

[POCOViewModel]
public class ViewModelBase : DevExpress.Mvvm.ViewModelBase, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
