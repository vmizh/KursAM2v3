using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using Data;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using Helper;

namespace LayoutManager
{
    public class LayoutManager : LayoutManagerBase
    {
        public readonly Dictionary<DependencyObject, MemoryStream> BaseControlLayouts =
            new Dictionary<DependencyObject, MemoryStream>();

        public readonly MemoryStream BaseLayout = new MemoryStream();

        public readonly List<LayoutManagerGridAutoGenerationColumns> ControlLayouts
            = new List<LayoutManagerGridAutoGenerationColumns>();

        public WindowsScreenState BaseWin;

        public LayoutManager(string fname, Window win, DependencyObject ctrl,
            List<LayoutManagerGridAutoGenerationColumns> ctrlLayouts = null, bool isWindowOnly = false)
        {
            FileName = fname;
            Win = win;
            SetWindowState();
            IsWindowOnly = isWindowOnly;
            LayoutControl = ctrl;
            if (LayoutControl != null && !(LayoutControl is DataLayoutControl))
                DXSerializer.Serialize(LayoutControl, BaseLayout, "Kurs", null);
            if (ctrlLayouts != null && ctrlLayouts.Count > 0)
                foreach (var l in ctrlLayouts)
                {
                    ControlLayouts.Add(l);
                    var m = new MemoryStream();
                    ((DataControlBase) l.LayoutControl).SaveLayoutToStream(m);
                    BaseControlLayouts.Add(l.LayoutControl, m);
                }
        }

        public LayoutManager(string fname, DependencyObject ctrl)
        {
            FileName = fname;
            Win = null;
            LayoutControl = ctrl;
        }

        private void SetWindowState()
        {
            BaseWin = new WindowsScreenState
            {
                FormHeight = Win.Height,
                FormWidth = Win.Width,
                FormLeft = Win.Left,
                FormTop = Win.Top,
                FormStartLocation = Win.WindowStartupLocation,
                FormState = Win.WindowState
            };
        }

        public void RegistrControlLayouts(LayoutManagerGridAutoGenerationColumns layout)
        {
            if (BaseControlLayouts.ContainsKey(layout.LayoutControl))
                return;
            ControlLayouts.Add(layout);
            var m = new MemoryStream();
            ((DataControlBase) layout.LayoutControl).SaveLayoutToStream(m);
            BaseControlLayouts.Add(layout.LayoutControl, m);
        }

        public override void ResetLayout()
        {
            try
            {
                if (BaseWin == null) return;
                Win.Height = BaseWin.FormHeight;
                Win.Width = BaseWin.FormWidth;
                Win.Left = BaseWin.FormLeft;
                Win.Top = BaseWin.FormTop;
                Win.WindowStartupLocation = BaseWin.FormStartLocation;
                Win.WindowState = BaseWin.FormState;
                BaseLayout.Position = 0;
                if (LayoutControl != null) DXSerializer.Deserialize(LayoutControl, BaseLayout, "Kurs", null);
                BaseLayout.Position = 0;
                if (BaseControlLayouts.Count <= 0) return;
                foreach (var l in BaseControlLayouts)
                {
                    l.Value.Position = 0;
                    //((DataControlBase)l.Key).BeginInit();
                    ((DataControlBase) l.Key).RestoreLayoutFromStream(l.Value);
                    //((DataControlBase)l.Key).EndInit();
                }

                //var connString = new SqlConnectionStringBuilder
                //{
                //    DataSource = "172.16.0.1",
                //    InitialCatalog = "KursSystem",
                //    UserID = "sa",
                //    Password = "CbvrfFhntvrf65"
                //}.ToString();
                //using (var ctx = new KursSystemEntities(connString))
                //{
                //    if (CurrentUser.UserInfo == null) return;
                //    var w = Win != null ? Win.GetType().Name : "Control";
                //    var l = ctx.FormLayout.FirstOrDefault(_ => _.UserId == CurrentUser.UserInfo.KursId
                //                                               && _.FormName == w && _.ControlName == FileName);
                //    if (l == null) return;
                //    ctx.FormLayout.Remove(l);
                //    ctx.SaveChanges();
                //}
            }
#pragma warning disable 168
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception ex)
#pragma warning restore 168
            {
            }
        }
    }
}