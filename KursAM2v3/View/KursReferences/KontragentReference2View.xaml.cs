using System.ComponentModel;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using KursAM2.ViewModel.Reference.Kontragent;
using KursDomain;
using KursDomain.References;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for KontragentReference2View.xaml
    /// </summary>
    public partial class KontragentReference2View
    {
        public KontragentReference2View()
        {
            InitializeComponent();
        }


        private void CustomTreeListDragDropManager_Drop(object sender, TreeListDropEventArgs e)
        {
            if (e.DraggedRows[0] is KontragentViewModel kontr)
            {
                var target = e.TargetNode.Content as KontragentGroupViewModel;
                kontr.EG_ID = target?.EG_ID;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var item = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == kontr.DocCode);
                    if (item != null)
                    {
                        item.EG_ID = target?.EG_ID;
                        ctx.SaveChanges();
                    }
                }

                if (DataContext is KontragentReferenceWindowViewModel vm)
                    vm.CurrentGroup = target;
                e.DraggedRows[0] = target;
                return;
            }

            if (e.DraggedRows[0] is TreeListNode l)
                if (l.Content is KontragentGroupViewModel group)
                {
                    var target = e.TargetNode.Content as KontragentGroupViewModel;
                    var x = e.TargetNode.ActualLevel;
                    var x1 = l.ActualLevel;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var item = ctx.UD_43.FirstOrDefault(_ => _.EG_ID == group.DocCode);
                        if (item != null)
                        {
                            item.EG_PARENT_ID = x != x1 ? null : target?.EG_ID;
                            ctx.SaveChanges();
                        }
                    }
                }
        }

        private void GridControlKontragent_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void CustomGridDragDropManager_Drop(object sender, GridDropEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void TreeListCategory_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}
