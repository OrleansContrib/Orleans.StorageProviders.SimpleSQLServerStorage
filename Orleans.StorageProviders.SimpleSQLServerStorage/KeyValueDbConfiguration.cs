using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    internal class KeyValueDbConfiguration : DbConfiguration
    {
        public KeyValueDbConfiguration(): base()
        {
            SetProviderServices(
                SqlProviderServices.ProviderInvariantName,            
                SqlProviderServices.Instance);

            SetDefaultConnectionFactory(new System.Data.Entity.Infrastructure.SqlConnectionFactory());
        }
    }
}