using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;

namespace Helper
{
    public class CommitHelper
    {
        public static readonly DependencyProperty CommitOnValueChangedProperty =
            DependencyProperty.RegisterAttached("CommitOnValueChanged", typeof (bool), typeof (CommitHelper),
                new PropertyMetadata(CommitOnValueChangedPropertyChanged));

        public static void SetCommitOnValueChanged(GridColumnBase element, bool value)
        {
            element.SetValue(CommitOnValueChangedProperty, value);
        }

        public static bool GetCommitOnValueChanged(GridColumnBase element)
        {
            return (bool) element.GetValue(CommitOnValueChangedProperty);
        }

        private static void CommitOnValueChangedPropertyChanged(DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            var col = source as GridColumnBase;
            // ReSharper disable once PossibleNullReferenceException
            if (col.View == null)
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    new Action<GridColumnBase, bool>(
                        ToggleCellValueChanging), col, (bool) e.NewValue);
            else
                ToggleCellValueChanging(col, (bool) e.NewValue);
        }

        private static void ToggleCellValueChanging(GridColumnBase col, bool subscribe)
        {
            if (col.View == null)
                return;

            if (subscribe)
            {
                var view = col.View as TreeListView;
                if (view != null)
                    view.CellValueChanging += TreeCellValueChanging;
                else
                    ((GridViewBase) col.View).CellValueChanging += GridCellValueChanging;
            }
            else
            {
                if (col.View is TreeListView)
                    ((TreeListView) col.View).CellValueChanging -= TreeCellValueChanging;
                else
                    ((GridViewBase) col.View).CellValueChanging -= GridCellValueChanging;
            }
        }

        private static void TreeCellValueChanging(object sender, TreeListCellValueChangedEventArgs e)
        {
            if ((bool) e.Column.GetValue(CommitOnValueChangedProperty))
                ((DataViewBase) sender)?.PostEditor();
        }

        private static void GridCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            //if((bool)e.Column.GetValue(CommitOnValueChangedProperty))
            ((DataViewBase) sender)?.PostEditor();
        }
    }
}