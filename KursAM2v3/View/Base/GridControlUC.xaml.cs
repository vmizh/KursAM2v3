using System.Windows.Controls;
using DevExpress.Xpf.Core;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for GridControlUC.xaml
    /// </summary>
    public partial class GridControlUC : UserControl
    {
        public GridControlUC()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }
    }
}
