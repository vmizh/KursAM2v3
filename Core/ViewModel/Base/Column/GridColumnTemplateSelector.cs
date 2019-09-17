using System.Windows;
using System.Windows.Controls;

namespace Core.ViewModel.Base.Column
{
    public class GridColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var column = (Column) item;
            if (column != null)
                return
                    (DataTemplate)
                    ((System.Windows.Controls.Control) container).FindResource(column.Settings + "ColumnTemplate");
            return null;
        }
    }
}