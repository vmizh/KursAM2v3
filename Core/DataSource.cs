using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace Core
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DataSource
    {
        public Guid Id { set; get; }
        public int Order { set; get; }
        public string Name { set; get; }
        public string ShowName { set; get; }
        public string Server { set; get; }
        public string DBName { set; get; }
        public string User { set; get; }
        public string Password { set; get; }
        public Brush Color { set; get; }

        public string GetConnectionString()
        {
            var strConn = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = DBName,
                UserID = User,
                Password = Password
            };
            return strConn.ConnectionString;
        }

        public string GetConnectionString(string user, string pwd)
        {
            User = user;
            Password = pwd;
            var strConn =
                new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    InitialCatalog = DBName,
                    UserID = User,
                    Password = Password,
                    ConnectTimeout = 0
                };
            return strConn.ConnectionString;
        }

        public override string ToString()
        {
            return ShowName;
        }
    }
}
