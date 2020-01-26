using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Core;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.LayoutControl;

namespace LayoutManager
{
    public class LayoutManager : LayoutManagerBase
    {
        public readonly MemoryStream BaseLayout = new MemoryStream();
        public LayoutManager(string fname, Window win, DependencyObject ctrl, bool isWindowOnly = false)
        {
            FileName = fname;
            Win = win;
            IsWindowOnly = isWindowOnly;
            LayoutControl = ctrl;
            if (LayoutControl != null && !(LayoutControl is DataLayoutControl))
            {
                DXSerializer.Serialize(LayoutControl, BaseLayout, "Kurs", null);
                //BaseLayout.Position = 0;
                //var sr = new StreamReader(BaseLayout);
                //string myStr = sr.ReadToEnd();
            }
        }

        public override void ResetLayout()
        {
            try
            {
                if (IsWindowOnly)
                    if (Win != null)
                    {
                        Win.Height = WinState.FormHeight;
                        Win.Width = WinState.FormWidth;
                        Win.Left = WinState.FormLeft;
                        Win.Top = WinState.FormTop;
                        Win.WindowStartupLocation = WinState.FormStartLocation;
                        Win.WindowState = WinState.FormState;
                    }
                if (LayoutControl == null) return;
                BaseLayout.Position = 0;
                DXSerializer.Deserialize(LayoutControl, BaseLayout, "Kurs", null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public LayoutManager(string fname, DependencyObject ctrl)
        {
            FileName = fname;
            Win = null;
            LayoutControl = ctrl;
        }
    }
}