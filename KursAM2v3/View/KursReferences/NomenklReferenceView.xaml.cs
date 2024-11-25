using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for NomenklReferenceView.xaml
    /// </summary>
    public partial class NomenklReferenceView
    {
        public NomenklReferenceView()
        {
            InitializeComponent();
        }

        private void treeListViewCategory_FocusedRowChanged(object sender,
            CurrentItemChangedEventArgs currentItemChangedEventArgs)
        {
            
            //Name.AllowEditing = DefaultBoolean.False;
        }

        private void TreeListViewCategory_OnMouseDoubleClick(object sender,
            MouseButtonEventArgs mouseButtonEventArgs)
        {
            //Name.AllowEditing = DefaultBoolean.True;
            treeListViewCategory.ShowEditor(false);
        }

        private void treeListViewCategory_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(Keyboard.FocusedElement is TextBox)) return;
            if (((e.Key == Key.Right &&
                  ((TextBox)Keyboard.FocusedElement).CaretIndex == ((TextBox)Keyboard.FocusedElement).Text.Length) ||
                 (e.Key == Key.Left &&
                  ((TextBox)Keyboard.FocusedElement).CaretIndex == 0)) &&
                Equals(treeListCategory.CurrentColumn, treeListCategory.Columns[treeListCategory.Columns.Count - 1]))
                e.Handled = true;
        }


        private void tableViewNomenkl_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != "Currency") return;
            if (!(e.Row is Nomenkl r)) return;
            if (r.NomenklNumber.Length <= 5 ||
                r.NomenklNumber.Substring(r.NomenklNumber.Length - 4, 3) != ((IName)r.Currency).Name)
                r.NomenklNumber += " " + ((IName)r.Currency).Name;
        }
    }
}
