using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KursAM2C.Models.DataModel;

namespace KursAM2C.Models
{
    public static class Globals
    {
        public static List<MenuModel> MenuItem { set; get; } = new List<MenuModel>();
        public static List<Link> Links { set; get; } = new List<Link>
        {
            new Link
            {
                Name = "Справочник сотрудников",
                StrLink = "/Employees/Employees"
            }
        };
    }
}