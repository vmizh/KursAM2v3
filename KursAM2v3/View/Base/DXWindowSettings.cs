using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Helper;

namespace KursAM2.View.Base
{
    public class DXWindowSettings
    {
        public static void Settings()
        {
            GridControlLocalizer.Active = new CustomDXGridLocalizer();
            EditorLocalizer.Active = new CustomEditorsLocalizer();

            EventManager.RegisterClassHandler(typeof(GridColumn), DXSerializer.AllowPropertyEvent,
                new AllowPropertyEventHandler((d, e) =>
                {
                    if (!e.Property.Name.Contains("Header")) return;
                    e.Allow = false;
                    e.Handled = true;
                }));
        }
       
    }
}
