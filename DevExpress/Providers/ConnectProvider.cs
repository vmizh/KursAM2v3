using System.Data.SqlClient;
using Core;
using Helper;

namespace DevExpress.Providers
{
    public class ConnectProvider
    {
        public static void InitializationConnect(string psw, string login)
        {
            GlobalOptions.SqlConnectionString = GetConnectionString(psw, login);
            GlobalOptions.GetEntities();
            if (GlobalOptions.UserInfo == null)
            {
                GlobalOptions.UserInfo = new User
                {
                    Id = 6
                };
                var reffer = new MainReferences();
                reffer.Reset();
            }
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