using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.StockHolder;

namespace KursAM2.View.StockHolder
{
    /// <summary>
    ///     Interaction logic for StockHolderReestrView.xaml
    /// </summary>
    public partial class StockHolderReestrView : ThemedWindow
    {
        public StockHolderReestrView()
        {
            InitializeComponent();
        }

        private void GridControlStockHolder_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            var dtx = DataContext as StockHolderReestrWindowViewModel;
            switch (e.Column.FieldName)
            {
                case "Employee":
                    if (dtx != null)
                    {
                        var btnEmpAdd = new ButtonInfo
                        {
                            Command = dtx.SelectEmployeeCommand,
                            GlyphKind = GlyphKind.Regular
                        };
                        var btnEmpDel = new ButtonInfo
                        {
                            Command = dtx.DeleteEmployeeCommand,
                            GlyphKind = GlyphKind.Cancel
                        };
                        var bkEdit = new ButtonEditSettings
                        {
                            TextWrapping = TextWrapping.Wrap,
                            IsTextEditable = false,
                            AllowDefaultButton = false
                        };
                        bkEdit.Buttons.Add(btnEmpAdd);
                        bkEdit.Buttons.Add(btnEmpDel);
                        e.Column.EditSettings = bkEdit;
                    }
                    break;
                case "Kontragent":
                    if (dtx != null)
                    {
                        var btnKontrAdd = new ButtonInfo
                        {
                            Command = dtx.SelectKontragentCommand,
                            GlyphKind = GlyphKind.Regular
                        };
                        var btnKontrDel = new ButtonInfo
                        {
                            Command = dtx.DeleteKontragentCommand,
                            GlyphKind = GlyphKind.Cancel
                        };
                        var bkEdit = new ButtonEditSettings
                        {
                            TextWrapping = TextWrapping.Wrap,
                            IsTextEditable = false,
                            AllowDefaultButton = false
                        };
                        bkEdit.Buttons.Add(btnKontrAdd);
                        bkEdit.Buttons.Add(btnKontrDel);
                        e.Column.EditSettings = bkEdit;
                    }

                    break;
                case "Note":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        AcceptsReturn = true,
                        SelectAllOnMouseUp = true,
                        TextWrapping = TextWrapping.Wrap,
                    };
                    break;
            }
        }
    }
}