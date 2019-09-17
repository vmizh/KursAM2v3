using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core;

namespace KursAM2C.Models.Source
{
    public static class MenuSource
    {
        public static void GetMenuItem()
        {

            using (var ctx = GlobalOptions.GetEntities())
            {
                var rightForRoleItem = ctx.USER_FORMS_RIGHT.Where(_ =>
                    _.USER_NAME.ToUpper() == HttpContext.Current.User.Identity.Name.ToUpper()).ToList();

                foreach (var item in ctx.MAIN_DOCUMENT_ITEM.ToList())
                {
                    var linkItem = Globals.Links.FirstOrDefault(_ => _.Name == item.NAME);
                    if (linkItem == null) continue;
                    if (rightForRoleItem.All(_ => _.FORM_ID != item.ID)) continue;
                    Globals.MenuItem.Add(new MenuModel
                    {
                        Name = item.NAME,
                        Id = item.ID,
                        OrderBy = item.ORDERBY,
                        ParentId = item.GROUP_ID,
                        StrLink = linkItem.StrLink
                    });
                }

                foreach (var item in ctx.MAIN_DOCUMENT_GROUP.ToList())
                {
                    if (Globals.MenuItem.All(_ => _.ParentId != item.ID)) continue;
                    Globals.MenuItem.Add(new MenuModel
                    {
                        Name = item.NAME,
                        Id = item.ID,
                        OrderBy = item.ORDERBY,
                        ParentId = null
                    });
                }
            }
        }
    }
}