using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace Helper
{
    public class GridControlHelper
    {
        private readonly GridControl myGridControl;

        public GridControlHelper(GridControl grid)
        {
            myGridControl = grid;
        }

        public GridColumn GenerateColumn(string colName, string header, string fieldName, bool isReadOnly)
        {
            var col = new GridColumn
            {
                Name = colName,
                Header = header,
                FieldName = fieldName,
                ReadOnly = isReadOnly
            };
            myGridControl.Columns.Add(col);
            return col;
        }

        public GridColumn GenerateNumericColumn(string colName, string header, string fieldName, bool isReadOnly, int precision)
        {
            var col = GenerateColumn(colName, header, fieldName, isReadOnly);
            var editor = new CalcEditSettings
            {
                Precision = precision,
                DisplayFormat = "n2",
                MaskUseAsDisplayFormat = true
            };
            col.EditSettings = editor;

            return col;
        }
    }
}