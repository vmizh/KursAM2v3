using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Signature
{
    /// <summary>
    ///     Interaction logic for Signature.xaml
    /// </summary>
    public partial class Signature
    {
        public Signature()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void DataControlBase_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {

        }
    }
}
