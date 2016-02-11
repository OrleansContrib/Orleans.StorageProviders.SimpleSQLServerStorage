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


            //SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            //SetDefaultConnectionFactory(new LocalDbConnectionFactory("v11.0"));

            //SetProviderServices("System.Data.SqlClient", () => SqlProviderServices.GetProviderServices();
            //SetDefaultConnectionFactory(new SqlConnectionFactory("v11.0"));
        }
    }
}