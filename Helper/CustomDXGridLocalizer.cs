using DevExpress.Xpf.Grid;

namespace Helper
{
    public class CustomDXGridLocalizer : GridControlLocalizer {
        protected override void PopulateStringTable() {
            base.PopulateStringTable();
            // Changes the caption of the menu item used to invoke the Total Summary Editor.
            //AddString(GridControlStringId.MenuFooterCustomize, "Customize Totals");

            //// Changes the Total Summary Editor's default caption.
            //AddString(GridControlStringId.TotalSummaryEditorFormCaption, "Totals Editor");

            //// Changes the default caption of the tab page that lists total summary items.
            //AddString(GridControlStringId.SummaryEditorFormItemsTabCaption, "Summary Items");
            //AddString(GridControlStringId.C, "Customize Totals");
            AddString(GridControlStringId.ColumnChooserCaption, "Выбор колонок");
            AddString(GridControlStringId.MenuColumnShowColumnChooser, "Выбор колонок");

        }
    }
}
