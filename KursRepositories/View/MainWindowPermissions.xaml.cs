using DevExpress.Xpf.Core;
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
using System.Windows.Shapes;
using DevExpress.Xpf.Grid;
using KursRepositories.ViewModels;


namespace KursRepositories.View
{
    /// <summary>
    /// Interaction logic for MainWindowPermissions.xaml
    /// </summary>
    public partial class MainWindowPermissions : ThemedWindow
    {
        public MainWindowPermissions()
        {
            InitializeComponent();
            DataContext = new MainWindowPermissionsViewModel();

        }

        private void RolesGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            foreach (GridColumn column in RolesGridControl.Columns)
            {
                if (column.FieldName == "IsSelectedItem")
                    column.Visible = false;

            }
        }
    }
}
