using System.Runtime.Serialization;
using System.Windows;

namespace LayoutManager
{
    [DataContract]
    public abstract class BaseLayout
    {
        
        [DataMember]
        public string Version { set; get; }

        public abstract void SaveToByte(ref byte[] bytes);
        public abstract void SaveToByte(DependencyObject obj, ref byte[] bytes);
        public abstract void RestoreFromByte(byte[] layout);
        public abstract void RestoreFromByte(DependencyObject obj, byte[] layout);

    }
}