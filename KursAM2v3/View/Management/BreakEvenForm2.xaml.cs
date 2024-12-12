using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Helper;

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
           
        }


        private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        
    }
}
