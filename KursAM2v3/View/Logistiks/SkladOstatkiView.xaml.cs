using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.ViewModel.Logistiks;
using KursDomain;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for SkladOstatkiView.xaml
    /// </summary>
    public partial class SkladOstatkiView : ThemedWindow
    {
        public SkladOstatkiView()
        {
            InitializeComponent();
        }

        private void NomenklskladGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void NomenklskladOperGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            switch (e.Column.FieldName)
            {
                case "NomQuantity":
                    e.Column.SortIndex = 5;
                    break;
            }
        }

        private void InvoiceClientGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void nomenklskladOperGridControl_LayoutUpdated(object sender, System.EventArgs e)
        {
            var col = KursGridControlHelper.GetColumnForField(nomenklskladOperGridControl, "SenderReceiverName");
            if (col == null) return;
            var iniFileName = Application.Current.Properties["DataPath"] + "\\User.ini";
            var iniFile = new IniFileManager(iniFileName);
            iniFile.Write("Layout", "SkaldOstatkiReceiverName", col.VisibleIndex.ToString());
        }
    }
}
