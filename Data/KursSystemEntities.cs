using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace Data
{
    public partial class KursSystemEntities : DbContext
    {
        public KursSystemEntities(string connectionString,
            bool validationMode = true,
            bool lazyLoadingMode = true,
            bool autoDetectMode = true)
            : base(new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = connectionString,
                Metadata =
                    @"res://*/KursSystem.csdl|res://*/KursSystem.ssdl|res://*/KursSystem.msl"
            }.ToString())
        {
            Configuration.ValidateOnSaveEnabled = validationMode;
            Configuration.LazyLoadingEnabled = lazyLoadingMode;
            Configuration.AutoDetectChangesEnabled = autoDetectMode;
        }
    }
}