using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LayoutManager
{
    public abstract class BaseLayoutManager
    {
        [DataMember]
        public string SystemDirectory { set; get; }

        [DataMember]
        public string UserDirectory { set; get; }

        [DataMember]
        public Dictionary<string, byte[]> Controls = new Dictionary<string, byte[]>();

    }
}