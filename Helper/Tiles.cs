using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace Helper
{
    public delegate void OpenDocument();

    [DataContract]
    public class TileGroup
    {
        [DataMember] public List<TileItem> TileItems = new List<TileItem>();

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Image Picture { set; get; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string Notes { set; get; }
    }

    [DataContract]
    public class TileItem
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public Image Picture { get; set; }

        [DataMember]
        public OpenDocument FormOpen { set; get; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string Notes { set; get; }
    }
}