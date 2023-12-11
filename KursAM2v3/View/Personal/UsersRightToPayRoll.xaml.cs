using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Core.WindowsManager;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Personal;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for UsersRightToPayRoll.xaml
    /// </summary>
    public partial class UsersRightToPayRoll
    {
        private readonly PREmpEmpRightViewModel _employe = new PREmpEmpRightViewModel();
        private readonly PREmpUserRightViewModel _user = new PREmpUserRightViewModel();

        public UsersRightToPayRoll()
        {
            InitializeComponent(); 
            
            gridUsers.DataContext = _user;
            gridEmployee.DataContext = _employe;
            Loaded += UsersRightToPayRollLoaded;
            Closing += UsersRightToPayRollClosing;

            //gridUsers.gridControl1.CurrentItemChanged += GridControl1CurrentItemChanged;
        }

        private void UsersRightToPayRollClosing(object sender, CancelEventArgs e)
        {
        }

        private void UsersRightToPayRollLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateContextMenu();
                _user.Load();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void BarButtonItem2ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            Close();
        }

        private void CreateContextMenu()
        {
            var ctxDel = new BarButtonItem
            {
                Content = "Удалить выбранные строки",
                Glyph = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/element_delete_big.png")),
                GlyphSize = GlyphSize.Large
            };
            var ctxNew = new BarButtonItem
            {
                Content = "Добавить работников",
                Glyph = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/element_new_big.png")),
                GlyphSize = GlyphSize.Large
            };
            ctxNew.ItemClick += CtxNewItemClick;
            ctxDel.ItemClick += CtxDelItemClick;
            //gridEmployee.dataGridContextMenu.ItemLinks.Add(new BarItemLinkSeparator());
            //gridEmployee.dataGridContextMenu.ItemLinks.Add(ctxNew);
            //gridEmployee.dataGridContextMenu.ItemLinks.Add(ctxDel);
        }

        private void CtxDelItemClick(object sender, ItemClickEventArgs e)
        {
            //if (gridEmployee.gridControl1.SelectedItems.Count == 0) return;
            //var usr = gridUsers.gridControl1.SelectedItem as PREmpUserRight;
            //if (usr == null) return;
            //var d = (gridEmployee.gridControl1.SelectedItems.Cast<object>()
            //    .Select(item => ((Persona) item).DocCode)
            //    .Where(docCode => docCode.HasValue)
            //    .Select(docCode => docCode.Value)).ToList();
            //using (var transaction = new TransactionScope())
            //{
            //    try
            //    {
            //        foreach (
            //            var delItem in
            //                d.Select(
            //                    dc =>
            //                        GlobalOptions.Entities.EMP_USER_RIGHTS.Single(
            //                            t => t.EMP_DC == dc && t.USER == usr.Name))
            //                    .ToList
            //                    ())
            //        {
            //            GlobalOptions.Entities.EMP_USER_RIGHTS.Remove(delItem);
            //        }
            //        GlobalOptions.Entities.SaveChanges();
            //        transaction.Complete();
            //        foreach (var item in (from object item in gridEmployee.gridControl1.SelectedItems
            //            select ((Persona) item)))
            //        {
            //            usr.Employee.Remove(item);
            //        }
            //        gridEmployee.gridControl1.RefreshData();
            //    }
            //    catch (Exception ex)
            //    {
            //        ErrorMessage.ShowError(ex);
            //    }
            //}
        }

        private void CtxNewItemClick(object sender, ItemClickEventArgs e)
        {
            //var isChanged = false;

            // ReSharper disable once UseObjectOrCollectionInitializer
            var dlg = new DialogSelectPersona();
            dlg.Owner = this;
            dlg.ShowDialog();
            // ReSharper disable once RedundantJumpStatement
            if (dlg.DialogResult != null && !(bool) dlg.DialogResult) return;
            //var usr = gridUsers.gridControl1.CurrentItem as PREmpUserRight;
            //if (usr == null) return;

            //using (var transaction = new TransactionScope(TransactionScopeOption.Required))
            //{
            //    try
            //    {
            //        foreach (var item in dlg.EmpSelected.Source.Where(item => !usr.Employee.Any(
            //            t => item.Persona.DocCode != null && t.DocCode == item.Persona.DocCode.Value)))
            //        {
            //            usr.Employee.Add(new Persona
            //            {
            //                DBTable = item.Persona.DBTable,
            //                DocCode = item.DocCode,
            //                TabelNumber = item.TabelNumber,
            //                Name = item.Name
            //            });
            //            if (item.Persona.DocCode != null)
            //                GlobalOptions.Entities.EMP_USER_RIGHTS.Add(new EMP_USER_RIGHTS
            //                {
            //                    EMP_DC = item.Persona.DocCode.Value,
            //                    USER = usr.Name
            //                });
            //            isChanged = true;
            //        }
            //        gridEmployee.gridControl1.RefreshData();
            //        if (isChanged)
            //        {
            //            GlobalOptions.Entities.SaveChanges();
            //            transaction.Complete();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        var s = new StringBuilder();
            //        s.Append(ex.Message);
            //        if (ex.InnerException != null)
            //            s.Append("\n " + ex.InnerException.Message);
            //        MessageBox.Show(ex.Message);
            //    }
            //}
        }

        #region Layout

        private void LoadLayout(string fname)
        {
            //gridEmployee.gridControl1.SelectionMode = MultiSelectMode.Row;
        }

        private void SaveLayout(string fname)
        {
        }

        #endregion
    }
}
