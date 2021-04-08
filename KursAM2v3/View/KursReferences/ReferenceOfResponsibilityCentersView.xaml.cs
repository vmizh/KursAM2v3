using System;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Common;
using Core.WindowsManager;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using KursAM2.ViewModel.Reference;
using LayoutManager;

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

        

        public Project dropProject { set; get; } 

        private void TreeListDragDropManager_Drop(object sender, TreeListDropEventArgs e)
        {
            if (!(e.TargetNode.Content is Project t) || dropProject == null) return;
            dropProject.ParentId = t.Id;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var prj = ctx.Projects.FirstOrDefault(_ => _.Id == dropProject.Id);
                    {
                        if (prj != null)
                        {
                            prj.ParentId = t.Id;
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

        }

        private void TreeListDragDropManager_DragOver(object sender, TreeListDragOverEventArgs e)
        {
            if (!(DataContext is ProjectReferenceWindowViewModel dtx)) return;
            dropProject = dtx.CurrentProject;
        }
    }
}