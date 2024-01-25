using System.Windows.Controls;
using DevExpress.Xpf.Grid;

namespace KursDomain.ViewModel.Base2;

public interface IForm
{
    UserControl FormControl { set; get; }
    string Title { get; set; }

    void Show();
    void Close();

    void LoadReferncesAsync();
}
