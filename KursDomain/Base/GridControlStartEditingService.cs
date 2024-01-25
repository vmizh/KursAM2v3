using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace KursDomain.Base;

public class GridControlStartEditingService : ServiceBase, IStartEditingService
{
    public GridControlStartEditingService()
    {
        
    }
    public static readonly DependencyProperty GridControlProperty =
        DependencyProperty.Register(nameof(GridControl), typeof(GridControl), typeof(GridControlStartEditingService),
            new PropertyMetadata(null));

    public GridControl GridControl
    {
        get => (GridControl)GetValue(GridControlProperty);
        set => SetValue(GridControlProperty, value);
    }

    public void StartEditing(object dataItem)
    {
        GridControl.CurrentItem = dataItem;
        var view = (TableView)GridControl.View;
        GridControl.CurrentColumn = view.VisibleColumns.First();
        GridControl.Dispatcher.BeginInvoke((Action)view.ShowEditor, DispatcherPriority.Input);
    }

    public void StartEditing(object dataItem, string fieldName)
    {
        GridControl.CurrentItem = dataItem;
        GridControl.SelectedItem = dataItem;
        var view = (TableView)GridControl.View;
        ColumnBase col = null;
        foreach (var c in GridControl.Columns)
            if (c.FieldName == fieldName)
                col = c;
        if (col == null)
            col = view.VisibleColumns.First();
        GridControl.CurrentColumn = col;
        GridControl.Dispatcher.BeginInvoke((Action)view.ShowEditor, DispatcherPriority.Input);
    }
}
