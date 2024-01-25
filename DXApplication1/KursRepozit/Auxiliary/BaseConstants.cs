using System.Data.SqlClient;

namespace KursRepozit.Auxiliary
{
    public class BaseConstants
    {
        public static string KursSystemDBConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = "172.16.1.1",
            InitialCatalog = "KursSystem",
            UserID = "KursUser",
            Password = "KursUser"
        }.ToString();
    }
}
