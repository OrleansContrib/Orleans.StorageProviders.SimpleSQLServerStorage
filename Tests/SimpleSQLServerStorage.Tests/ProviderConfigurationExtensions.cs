using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSQLServerStorage.Tests
{
    public static class ProviderConfigurationExtensions
    {
        /// <summary>
        /// Adds a storage provider of type <see cref="Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage"/>
        /// </summary>
        /// <param name="config">The cluster configuration object to add provider to.</param>
        /// <param name="providerName">The provider name.</param>
        /// <param name="connectionString">SqlClient connection string</param>
        /// <param name="UseJsonFormat">true, false, or both</param>
        public static void AddSimpleSQLStorageProvider(
            this ClusterConfiguration config,
            string providerName,
            string connectionString,
            string UseJsonFormat)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));

            var properties = new Dictionary<string, string>
            {
                { "ConnectionString" , connectionString },
                { "TableName", "basic"},
                { "UseJsonFormat", UseJsonFormat }

            };

            config.Globals.RegisterStorageProvider<Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage>(providerName, properties);
        }
    }
}
