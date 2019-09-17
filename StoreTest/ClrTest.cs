using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace StoreTest
{
    public class ClrTest
    {
        [SqlProcedure]
        public static void SendReaderToClient()
        {
            using (var connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                var command = new SqlCommand("select @@version", connection);
                var r = command.ExecuteReader();
                SqlContext.Pipe?.Send(r);
            }
        }
    }
}