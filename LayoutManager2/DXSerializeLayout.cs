using System;
using System.IO;
using System.Windows;
using DevExpress.Xpf.Core.Serialization;

namespace LayoutManager
{
    public class DXSerializeLayout : BaseLayout
    {
        public override void SaveToByte(ref byte[] bytes)
        {
            throw new NotImplementedException();
            
        }

       
        public override void SaveToByte(DependencyObject obj, ref byte[] bytes)
        {
            var ms = new MemoryStream();
            var options = new DXOptionsLayout {LayoutVersion = Version};
            DXSerializer.Serialize(obj, ms, "Kurs", options);
        }

        public override void RestoreFromByte(byte[] layout)
        {
            throw new NotImplementedException();
        }

        public override void RestoreFromByte(DependencyObject obj, byte[] layout)
        {
            var ms = new MemoryStream(layout);
            var options = new DXOptionsLayout(); 
            DXSerializer.Deserialize(obj, ms, "Kurs", options);
            ms.Close();
        }
    }
}