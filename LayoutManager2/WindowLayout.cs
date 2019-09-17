using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace LayoutManager
{
    public class WindowLayout : BaseLayout
    {
        [DataMember]
        public double Top { set; get; }

        [DataMember]
        public double Left { set; get; }

        [DataMember]
        public double Height { set; get; }

        [DataMember]
        public double Width { set; get; }

        [DataMember]
        public WindowStartupLocation StartLocation { set; get; }

        [DataMember]
        public WindowState State { set; get; }

        public override void SaveToByte(ref byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            var ser =
                new DataContractSerializer(typeof(WindowLayout));
            var ms = new MemoryStream();
            ser.WriteObject(ms,this);
            bytes = ms.ToArray();
            ms.Close();

        }

        public override void SaveToByte(DependencyObject obj, ref byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override void RestoreFromByte(byte[] layout)
        {
            var ms = new MemoryStream(layout);
            var r = XmlReader.Create(ms);
            var p =
                new DataContractSerializer(typeof(WindowLayout)).ReadObject(r) as
                    WindowLayout;
            Top = p.Top;
            Left = p.Left;
            Height = p.Height;
            Width = p.Width;
            StartLocation = p.StartLocation;
            State = p.State;
        }

        public override void RestoreFromByte(DependencyObject obj, byte[] layout)
        {
            throw new NotImplementedException();
        }
    }
}