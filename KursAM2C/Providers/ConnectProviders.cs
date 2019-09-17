using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.WebPages;
using Core;
using KursAM2C.Models;
using KursAM2C.Models.Source;
using Microsoft.Ajax.Utilities;

namespace KursAM2C.Providers
{
    public static class ConnectProviders
    {
        public static void InitializationConnect()
        {
            if ( !GlobalOptions.SqlConnectionString.IsEmpty()
                && !(HttpContext.Current.User.Identity.IsAuthenticated)) return;
            GlobalOptions.SqlConnectionString = GetConnectionString("sysadm", "19655691");

            //SourceDownload
            if (Globals.MenuItem.Count == 0)
            MenuSource.GetMenuItem();
        }
        public static string GetConnectionString(string user, string pwd)
        {
            var strConn = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
                InitialCatalog = "EcoOndol",
                UserID = user,
                Password = pwd
            };
            return strConn.ConnectionString;
        }
    }
}