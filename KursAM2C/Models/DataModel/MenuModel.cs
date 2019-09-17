using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace KursAM2C.Models
{
    public class MenuModel
    {
        public int Id { set; get; }
        public int? ParentId { set; get; }
        public string Name { set; get; }
        public int? OrderBy { set; get; }
        public string StrLink { set; get; }
    }
}