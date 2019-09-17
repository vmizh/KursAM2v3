using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LayoutManager
{
    [DataContract(Name = "LayoutGridColumnItem")]
    public class LayoutGridColumnItem
    {
        [DataMember]
        public int Order { set; get; }
        [DataMember]
        public double Width { set; get; }
        [DataMember]
        public string FieldName { set; get; }
        [DataMember]
        public bool IsReadOnly { set; get; }
        [DataMember]
        public bool IsVisible { set; get; }
    }
    // [DataContract]
    public class LayoutGridColumnItems : List<LayoutGridColumnItem>
    {
    }
}