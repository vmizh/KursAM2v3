using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Data;

namespace Helper
{
    public class CurrentUser
    {
        public static User UserInfo { set; get; }
    }

    [DataContract]
    public class User
    {

        [DataMember] public List<UserGroup> Groups = new List<UserGroup>();
        [DataMember] public List<TileGroup> MainTileGroups = new List<TileGroup>();
        [DataMember] public List<UserProfile> Profile = new List<UserProfile>();

        [DataMember]
        public ObservableCollection<UserMenuFavorites> MenuFavorites { set; get; }
            = new ObservableCollection<UserMenuFavorites>();

        [DataMember]
        public int Id { set; get; }

        [DataMember]
        public Guid KursId { set; get; }

        [DataMember]
        public string NickName { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public int? TabelNumber { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public int? IsCanConnect { get; set; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string Notes { set; get; }
    }

    [DataContract]
    public class UserGroup
    {
        [DataMember] public List<User> Users = new List<User>();

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string Notes { set; get; }

        [DataMember]
        public int Id { set; get; }
    }
}