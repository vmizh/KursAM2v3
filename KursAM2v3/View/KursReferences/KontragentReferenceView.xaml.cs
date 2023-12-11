using System;
using System.Linq;
using System.Windows;
using Core;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using KursDomain;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.References;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for KontragentReferenceView.xaml
    /// </summary>
    public partial class KontragentReferenceView : ILayout
    {
        public KontragentReferenceView()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Loaded += MutualAccountingSearchView_Loaded;
            Unloaded += MutualAccountingSearchView_Unloaded;
            CheckBox.Visibility = Visibility.Hidden;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void MutualAccountingSearchView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void MutualAccountingSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void TreeListView_OnCellValueChanged(object sender, TreeListCellValueChangedEventArgs e)
        {
            if (!(e.Row is KontragentGroup row)) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    if (row.Id != -1)
                    {
                        var item = ctx.UD_43.FirstOrDefault(_ => _.EG_ID == row.Id);
                        // ReSharper disable once PossibleNullReferenceException
                        item.EG_PARENT_ID = row.ParentId;
                        item.EG_NAME = row.Name;
                    }
                    else
                    {
                        var egId = ctx.UD_43.Max(_ => _.EG_ID) + 1;
                        var item = new UD_43
                        {
                            EG_ID = egId,
                            EG_PARENT_ID = row.ParentId,
                            EG_NAME = row.Name
                        };
                        row.Id = egId;
                        ctx.UD_43.Add(item);
                    }

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
        }

        private void TreeGroups_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void KontragentsGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (!(DataContext is KontragentReferenceWindowViewModel dtx)) return;
        //    if (ToggleButton.IsChecked == true)
        //    {
        //        treeGroups.IsEnabled = false;
        //        dtx.GetAllKontragent();
        //        dtx.CurrentGroup = null;
        //        CheckBox.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        treeGroups.IsEnabled = true;
        //        dtx.RefreshData(null);
        //        CheckBox.Visibility = Visibility.Hidden;
        //    }
        //}
    }
}
