using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for LastUsersDocumentView.xaml
    /// </summary>
    public partial class LastUsersDocumentView 
    {
        public LastUsersDocumentView()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
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
