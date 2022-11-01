using System.Data.EntityClient;
using System.Data.SqlClient;

namespace KursDomain.DBContext;

public class KursDBContext
{
    public KursDBContext(string connectionString)
    {
        var entityConn = new EntityConnectionStringBuilder
        {
            Provider = "System.Data.SqlClient",
            ProviderConnectionString = connectionString,
            Metadata =
                @"res://*/ALFAMEDIA.csdl|res://*/ALFAMEDIA.ssdl|res://*/ALFAMEDIA.msl"
        }.ToString();
        Context = new KursContext.KursContext(entityConn);
    }

    public KursDBContext(string server, string dbname, string user, string pw)
    {
        var sqlBuild = new SqlConnectionStringBuilder();
        sqlBuild.DataSource = server;
        sqlBuild.InitialCatalog = dbname;
        sqlBuild.UserID = user;
        sqlBuild.Password = pw;
        sqlBuild.IntegratedSecurity = false;

        var entityConn = new EntityConnectionStringBuilder
        {
            Provider = "System.Data.SqlClient",
            ProviderConnectionString = sqlBuild.ConnectionString,
            Metadata =
                @"res://*/ALFAMEDIA.csdl|res://*/ALFAMEDIA.ssdl|res://*/ALFAMEDIA.msl"
        }.ToString();
        Context = new KursContext.KursContext(entityConn);
    }

    public KursContext.KursContext Context { get; }
}
