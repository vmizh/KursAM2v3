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
            AddString(GridControlStringId.MenuColumnSortAscending,"По возрастанию");
            AddString(GridControlStringId.MenuColumnSortDescending,"По убыванию");
            AddString(GridControlStringId.MenuColumnBestFit, "Автоширина");
            AddString(GridControlStringId.MenuColumnBestFitColumns, "Автоширина для всех");
            AddString(GridControlStringId.MenuColumnClearFilter, "Очистить фильтр");
            AddString(GridControlStringId.MenuColumnClearSorting, "Убрать сортировку");
            AddString(GridControlStringId.MenuColumnFilterEditor, "Изменить фильтр");
            AddString(GridControlStringId.MenuColumnShowSearchPanel, "Показать панель поиска");
            AddString(GridControlStringId.MenuColumnGroup, "Сгруппировать по колонке");
            AddString(GridControlStringId.MenuColumnHideGroupPanel, "Скрыть панель группировки");
            AddString(GridControlStringId.MenuColumnShowGroupPanel, "Показать панель группировки");
            AddString(GridControlStringId.GridGroupPanelText, "Панель группировки");
            AddString(GridControlStringId.MenuGroupPanelFullExpand, "Раскрыть все");
            AddString(GridControlStringId.MenuGroupPanelFullCollapse, "Свернуть все");
            AddString(GridControlStringId.MenuGroupPanelClearGrouping, "Очистить группировку");


        }
    }
}
