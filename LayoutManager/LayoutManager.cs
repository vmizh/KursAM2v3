using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;

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
                    ((DataControlBase)l.LayoutControl).SaveLayoutToStream(m);
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
            ((DataControlBase)layout.LayoutControl).SaveLayoutToStream(m);
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
                if (BaseControlLayouts.Count > 0)
                    foreach (var l in BaseControlLayouts)
                    {
                        l.Value.Position = 0;
                        ((DataControlBase)l.Key).RestoreLayoutFromStream(l.Value);
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}