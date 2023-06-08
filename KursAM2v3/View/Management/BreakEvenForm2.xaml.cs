using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for BreakEvenForm2.xaml
    /// </summary>
    public partial class BreakEvenForm2
    {
        public BreakEvenForm2()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;

            EventManager.RegisterClassHandler(typeof(GridColumn), DXSerializer.AllowPropertyEvent,
                new AllowPropertyEventHandler((d, e) =>
                {
                    if (!e.Property.Name.Contains("Header")) return;
                    e.Allow = false;
                    e.Handled = true;
                }));
        }


        private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void GridCurrency_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = "Crs_" + e.Column.FieldName;
        }
    }
}
