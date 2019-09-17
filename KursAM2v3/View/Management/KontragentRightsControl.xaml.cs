using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Bars;
using KursAM2.ViewModel.Personal;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for KontragentRightsControl.xaml
    /// </summary>
    public partial class KontragentRightsControl
    {
        private readonly string myLayoutFileName =
            $"{Environment.CurrentDirectory}\\Layout\\KontragentRightsControl.xml";

        private readonly PREmpUserRightViewModel myUser = new PREmpUserRightViewModel();

        public KontragentRightsControl()
        {
            InitializeComponent();
            gridUsers.DataContext = myUser;
            Loaded += KontragentRightsControl_Loaded;
            Closing += KontragentRightsControl_Closing;
            barButtonItem2.ItemClick += barButtonItem2_ItemClick;
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private void CreateContextMenu()
        {
            var ctxDel = new BarButtonItem
            {
                Content = "Удалить выбранные строки",
                Glyph = new BitmapImage(new Uri("pack://application:,,/Images/element_delete_big.png")),
                GlyphSize = GlyphSize.Large
            };
            var ctxNew = new BarButtonItem
            {
                Content = "Добавить контрагентов",
                Glyph = new BitmapImage(new Uri("pack://application:,,/Images/element_new_big.png")),
                GlyphSize = GlyphSize.Large
            };
            ctxNew.ItemClick += ctxNew_ItemClick;
            ctxDel.ItemClick += ctxDel_ItemClick;
            //gridKontr.dataGridContextMenu.ItemLinks.Add(new BarItemLinkSeparator());
            //gridKontr.dataGridContextMenu.ItemLinks.Add(ctxNew);
            //gridKontr.dataGridContextMenu.ItemLinks.Add(ctxDel);
        }

        private void ctxDel_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private void ctxNew_ItemClick(object sender, ItemClickEventArgs e)
        {
            var dlg = new KontargentDialogSelect();
            dlg.Owner = this;
            dlg.ShowDialog();
            // ReSharper disable once RedundantJumpStatement
            if (dlg.DialogResult != null && !(bool) dlg.DialogResult) return;
            //var usr = gridUsers.gridControl1.CurrentItem as PREmpUserRight;
            //if (usr == null) return;
            //try
            //{
            //    var transaction = new TransactionScope(TransactionScopeOption.Required);
            //    Entities ctx = new Entities();

            //    //foreach (
            //    //    var item in
            //    //        dlg.EmpSelected.Source.Where(
            //    //            item => !usr.Employee.Any(t => t.DocCode == item.Persona.DocCode.Value)))
            //    //{
            //    //    usr.Employee.Add(new Persona { DocCode = item.DocCode, TabelNumber = item.TabelNumber, Name = item.Name });
            //    //    ctx.AddToEMP_USER_RIGHTS(new EMP_USER_RIGHTS { EMP_DC = item.Persona.DocCode.Value, USER = usr.Name });
            //    //}
            //    //gridEmployee.gridControl1.RefreshData();

            //    ctx.SaveChanges();
            //    transaction.Complete();
            //}
            //catch (Exception ex)
            //{
            //    var s = new StringBuilder();
            //    s.Append(ex.Message);
            //    if (ex.InnerException != null)
            //        s.Append("\n " + ex.InnerException.Message);
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void KontragentRightsControl_Closing(object sender, CancelEventArgs e)
        {
            SaveLayout(myLayoutFileName);
        }

        private void KontragentRightsControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLayout(myLayoutFileName);
            CreateContextMenu();
            myUser.Load();
        }

        #region Layout

        private void LoadLayout(string fname)
        {
            //if (File.Exists(fname))
            //{
            //    using (var fs = File.OpenRead(fname))
            //    {
            //        var r = XmlReader.Create(fs);
            //        var p =
            //            new DataContractSerializer(typeof (UsersRightToPayRoll.FormInfo)).ReadObject(r) as
            //                UsersRightToPayRoll.FormInfo;
            //        if (p == null) return;
            //        Height = p.FormHeight;
            //        Width = p.FormWidth;
            //        Left = p.FormLeft;
            //        Top = p.FormTop;
            //        WindowStartupLocation = p.FormStartLocation;
            //        WindowState = p.FormState;
            //        MemoryStream ms;
            //        ms = new MemoryStream(p.GridKontr);
            //        //gridKontr.gridControl1.RestoreLayoutFromStream(ms);
            //        ms.Close();

            //        ms = new MemoryStream(p.GridUser);
            //        //gridUsers.gridControl1.RestoreLayoutFromStream(ms);
            //        ms.Close();

            //        ms = new MemoryStream(p.DockLayout);
            //        dockManager.RestoreLayoutFromStream(ms);
            //        ms.Close();
            //    }
            //}
            //gridKontr.gridControl1.SelectionMode = MultiSelectMode.Row;
        }

        private void SaveLayout(string fname)
        {
            //var saveLayout = new UsersRightToPayRoll.FormInfo
            //{
            //    FormHeight = Height,
            //    FormWidth = Width,
            //    FormLeft = Left,
            //    FormTop = Top,
            //    FormStartLocation = WindowStartupLocation,
            //    FormState = WindowState
            //};
            //var ms = new MemoryStream();
            ////gridUsers.gridControl1.SaveLayoutToStream(ms);
            //saveLayout.GridUser = ms.ToArray();
            //ms.Close();

            //ms = new MemoryStream();
            ////gridKontr.gridControl1.SaveLayoutToStream(ms);
            //saveLayout.GridKontr = ms.ToArray();
            //ms.Close();

            //ms = new MemoryStream();
            //dockManager.SaveLayoutToStream(ms);
            //saveLayout.DockLayout = ms.ToArray();

            //var writer = new FileStream(fname, FileMode.Create);
            //var ser =
            //    new DataContractSerializer(typeof (UsersRightToPayRoll.FormInfo));
            //ser.WriteObject(writer, saveLayout);
            //writer.Close();
        }

        #endregion
    }
}