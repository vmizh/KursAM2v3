using System.Data.SqlClient;

namespace KursRepozit.Auxiliary
{
    public class BaseConstants
    {
        public static string KursSystemDBConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = "172.16.0.1",
            InitialCatalog = "KursSystem",
            UserID = "sa",
            Password = "CbvrfFhntvrf65"
        }.ToString();
    }
}