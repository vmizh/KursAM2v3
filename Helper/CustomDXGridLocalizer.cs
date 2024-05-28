using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;

namespace Helper
{
    public class CustomDXGridLocalizer : GridControlLocalizer {
        protected override void PopulateStringTable() {
            base.PopulateStringTable();
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
            AddString(GridControlStringId.DefaultTotalSummaryFormatStringInSameColumn_Avg, "n2");
            AddString(GridControlStringId.DefaultTotalSummaryFormatStringInSameColumn_Sum, "n2");
            AddString(GridControlStringId.DefaultTotalSummaryFormatStringInSameColumn_Count, "n0");
            AddString(GridControlStringId.MenuFooterAverage, "Среднее");
            AddString(GridControlStringId.MenuFooterCount, "Кол-во");
            AddString(GridControlStringId.MenuFooterCustom, "Вручную");
            AddString(GridControlStringId.MenuFooterCustomize, "Настройка");
            AddString(GridControlStringId.MenuFooterMax, "Максимум");
            AddString(GridControlStringId.MenuFooterMin, "Минимум");
            AddString(GridControlStringId.MenuFooterSum, "Сумма");
        }
    }

    public class CustomEditorsLocalizer : EditorLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();
            AddString(EditorStringId.Today,"Сегодня");
            AddString(EditorStringId.Clear,"Очистить");
            AddString(EditorStringId.Apply,"Принять");
            AddString(EditorStringId.FilterCriteriaToStringFunctionStartsWith,"Начиная с");
            AddString(EditorStringId.FilterCriteriaToStringFunctionContains,"Содержит");
            AddString(EditorStringId.FilterCriteriaToStringFunctionEndsWith, "Заканчивается на");
            AddString(EditorStringId.FilterCriteriaToStringFunctionIsNullOrEmpty, "null или пусто");

            AddString(EditorStringId.FilterClauseBeginsWith,"Начиная с");
            AddString(EditorStringId.FilterClauseAnyOf,"Любой из");
            AddString(EditorStringId.FilterClauseBetween, "Между");
            AddString(EditorStringId.FilterClauseBetweenAnd, "Между и");
            AddString(EditorStringId.FilterClauseBetweenDates,"Между датами");
            AddString(EditorStringId.FilterClauseContains,"Содержит");
            AddString(EditorStringId.FilterClauseDoesNotContain, "Не содержит");
            AddString(EditorStringId.FilterClauseDoesNotEqual, "Не равно");
            AddString(EditorStringId.FilterClauseEndsWith, "Оканчивается на");
            AddString(EditorStringId.FilterClauseEquals, "Равно");
            AddString(EditorStringId.FilterClauseGreater, "Больше");
            AddString(EditorStringId.FilterClauseGreaterOrEqual, "Больше или равно");
            AddString(EditorStringId.FilterClauseIsNotNull, "Не null");
            AddString(EditorStringId.FilterClauseIsNull, "Равно null");
            AddString(EditorStringId.FilterClauseLess, "Меньше");
            AddString(EditorStringId.FilterClauseLessOrEqual, "Меньше или равно");
            AddString(EditorStringId.FilterClauseLike, "Похоже на");
            AddString(EditorStringId.FilterClauseNotLike, "Не похоже на");
        }
    }

    public class CustomDXMessageBoxLocalizer : DXMessageBoxLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();
            AddString(DXMessageBoxStringId.Yes, "Да");
            AddString(DXMessageBoxStringId.No, "Нет");
            AddString(DXMessageBoxStringId.Cancel, "Отмена");
        }
    }
}
