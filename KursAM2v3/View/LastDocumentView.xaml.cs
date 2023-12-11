using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for LastDocumentView.xaml
    /// </summary>
    public partial class LastDocumentView 
    {
        public LastDocumentView()
        {
            InitializeComponent(); 
            
        }

        private void GridRows_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
            e.Column.Name = e.Column.FieldName;
            switch (e.Column.Name)
            {
                case "Description":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        TextWrapping = TextWrapping.Wrap,
                        IsEnabled = false,
                        VerticalContentAlignment = VerticalAlignment.Stretch
                    };
                    break;
                case "LastOpen":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        DisplayFormat = "f",
                        MaskUseAsDisplayFormat = true
                    };
                    break;
            }
        }
    }
}
