using Core;
using Core.EntityViewModel;
using Core.WindowsManager;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using KursAM2.ViewModel.Reference;
using LayoutManager;
using System;
using System.Linq;
using System.Windows;
using Core.ViewModel.Common;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for ReferenceOfResponsibilityCentersView.xaml
    /// </summary>
    public partial class ReferenceOfResponsibilityCentersView : ILayout
    {


        public ReferenceOfResponsibilityCentersView()
        {
            InitializeComponent();

            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, treeListControl);
            Loaded += ReferenceOfResponsibilityCentersView_Loaded;
            Unloaded += ReferenceOfResponsibilityCentersView_Unloaded;
            
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void ReferenceOfResponsibilityCentersView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void ReferenceOfResponsibilityCentersView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void TreeListControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
           
        }

        public CentrOfResponsibility  dropCenter { set; get; }

        private void TreeListDragDropManager_Drop(object sender, TreeListDropEventArgs e)
        {
            if (!(e.TargetNode.Content is SD_40ViewModel t) || dropCenter == null) return;
            dropCenter.CENT_PARENT_DC = t.DOC_CODE;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == dropCenter.DOC_CODE);
                    {
                        if (centr != null)
                        {
                            centr.CENT_PARENT_DC = t.DOC_CODE;
                        }
                    }
                    ctx.SaveChanges();
                    tx.Commit();
                }

                catch (Exception ex)
                {
                    tx.Rollback();
                    WindowManager.ShowError(ex);
                }
            }
            treeListControl.RefreshData();
        }

        private void TreeListDragDropManager_DragOver(object sender, TreeListDragOverEventArgs e)
        {
            if (!(DataContext is ReferenceOfResponsibilityCentersWindowViewModel dtx)) return;
            dropCenter = dtx.CurrentCenter;
        }
    }
}