using System;
using System.Data;
using System.Data.SqlClient;

namespace GenerateDataContract
{
    public class DataBaseContext
    {
        public SqlConnection Connect { set; get; }
        public string Source { set; get; }
        public string DataBaseName { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public string Name { set; get; }
        public ConnectionState State => Connect?.State ?? ConnectionState.Closed;

        public void Open(string source, string dbname, string uname, string pwd)
        {
            var strConn = new SqlConnectionStringBuilder
            {
                DataSource = source,
                InitialCatalog = dbname,
                UserID = uname,
                Password = pwd,
                IntegratedSecurity = false
            };
            try
            {
                Console.WriteLine(strConn.ConnectionString);
                Connect = new SqlConnection(strConn.ConnectionString);
                Connect.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Open()
        {
            var strConn = new SqlConnectionStringBuilder
            {
                DataSource = Source,
                InitialCatalog = DataBaseName,
                UserID = UserName,
                Password = Password,
                IntegratedSecurity = false
            };
            try
            {
                Console.WriteLine(strConn.ConnectionString);
                Connect = new SqlConnection(strConn.ConnectionString);
                Connect.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Close()
        {
            try
            {
                Connect?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}