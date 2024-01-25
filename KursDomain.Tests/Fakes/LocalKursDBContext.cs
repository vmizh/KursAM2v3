using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace KursDomain.Tests.Fakes
{

    public class LocalKursDBContext
    {
        public KursContext.KursContext Context { get; }

        public LocalKursDBContext()
        {
            SqlConnectionStringBuilder sqlBuild = new SqlConnectionStringBuilder
            {
                DataSource = "localhost",
                InitialCatalog = "AlfaTest",
                UserID = "KursUser",
                Password = "KursUser",
                IntegratedSecurity = false
            };

            var entityConn = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuild.ConnectionString,
                Metadata =
                    @"res://*/ALFAMEDIA.csdl|res://*/ALFAMEDIA.ssdl|res://*/ALFAMEDIA.msl"
            }.ToString();
            Context = new KursContext.KursContext(entityConn);
        }
    }
}
