using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Base
{
    /// <summary>
    /// Interaction logic for TableBaseUC.xaml
    /// </summary>
    public partial class TableBaseUC : UserControl
    {
        public TableBaseUC()
        {
            InitializeComponent();
        }

        private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}
