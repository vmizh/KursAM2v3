using System.Runtime.Serialization;
using System.Windows;

namespace LayoutManager
{
    [DataContract]
    public class WindowsScreenState
    {
        [DataMember]
        public double FormTop { set; get; }

        [DataMember]
        public double FormLeft { set; get; }

        [DataMember]
        public double FormHeight { set; get; }

        [DataMember]
        public double FormWidth { set; get; }

        [DataMember]
        public WindowStartupLocation FormStartLocation { set; get; }

        [DataMember]
        public WindowState FormState { set; get; }

        [DataMember]
        public byte[] Layout { set; get; }

        [DataMember(IsRequired = false)]
        public bool IsWindow { set; get; }
    }
}