using System.Collections.Generic;
using DevExpress.Models.DataModel;

namespace DevExpress.Models
{
    internal static class Globals
    {
        public static List<MenuModel> MenuItem { set; get; } = new List<MenuModel>();
        public static List<Link> Links { set; get; } = new List<Link>
        {
            new Link
            {
                Name = "Кассовая книга",
                StrLink = "/CashBook"
            }
        };
    }
}