using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;

namespace Data
{
    public partial class ALFAMEDIAEntities
    {
        public ALFAMEDIAEntities(string connectionString,
            bool validationMode = true,
            bool lazyLoadingMode = true,
            bool autoDetectMode = true)
            : base(new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = connectionString,
                Metadata =
                    @"res://*/ALFAMEDIA.csdl|res://*/ALFAMEDIA.ssdl|res://*/ALFAMEDIA.msl"
            }.ToString())
        {
            Configuration.ValidateOnSaveEnabled = validationMode;
            Configuration.LazyLoadingEnabled = lazyLoadingMode;
            Configuration.AutoDetectChangesEnabled = autoDetectMode;
#if DEBUG
            Database.Log = e => { Debug.WriteLine(e); };
#endif
        }
    }
}