using Newtonsoft.Json;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Orleans.Serialization;
using System.Data.Entity.Core.Objects;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    public class SimpleSQLServerStorage : IStorageProvider
    {
        private SqlConnectionStringBuilder sqlconnBuilder;

        private const string CONNECTION_STRING = "ConnectionString";

        string myConnectionString = @"metadata=.\Model1.csdl|.\Model1.ssdl|.\Model1.msl;provider=System.Data.SqlClient;provider connection string="";data source=.;initial catalog=test;integrated security=True;multipleactiveresultsets=True;App=EntityFramework""";

        private string serviceId;
        private Newtonsoft.Json.JsonSerializerSettings jsonSettings;

        /// <summary> Name of this storage provider instance. </summary>
        /// <see cref="IProvider#Name"/>
        public string Name { get; private set; }

        /// <summary> Logger used by this storage provider instance. </summary>
        /// <see cref="IStorageProvider#Log"/>
        public Logger Log { get; private set; }


        /// <summary> Initialization function for this storage provider. </summary>
        /// <see cref="IProvider#Init"/>
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            serviceId = providerRuntime.ServiceId.ToString();

            if (!config.Properties.ContainsKey(CONNECTION_STRING) ||
                string.IsNullOrWhiteSpace(config.Properties[CONNECTION_STRING]))
            {
                throw new ArgumentException("Specify a value for", CONNECTION_STRING);
            }
            var connectionString = config.Properties[CONNECTION_STRING];
            sqlconnBuilder = new SqlConnectionStringBuilder(connectionString);

            //a validation of the connection would be wise to perform here
            await new SqlConnection(sqlconnBuilder.ConnectionString).OpenAsync();


            jsonSettings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };


            Log = providerRuntime.GetLogger("StorageProvider.SimpleSQLServerStorage." + serviceId);
        }

        // Internal method to initialize for testing
        internal void InitLogger(Logger logger)
        {
            Log = logger;
        }

        /// <summary> Shutdown this storage provider. </summary>
        /// <see cref="IStorageProvider#Close"/>
        public Task Close()
        {
            //the using of the dbContext of the async methods finally should be disposing the connection
            return TaskDone.Done;
        }

        /// <summary> Read state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#ReadStateAsync"/>
        public async Task ReadStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();

            if (Log.IsVerbose3)
            {
                Log.Verbose3((int) SimpleSQLServerProviderErrorCodes.SimpleSQLServerProvide_ReadingData,
                    "Reading: GrainType={0} Pk={1} Grainid={2} from DataSource={3}",
                    grainType, primaryKey, grainReference, this.sqlconnBuilder.DataSource + "." + this.sqlconnBuilder.InitialCatalog);
            }

            var data = new Dictionary<string, object>();
            //    RedisValue value = await redisDatabase.StringGetAsync(primaryKey);
            using (var db = new KeyValueDbContext(this.sqlconnBuilder.ConnectionString))
            {
                var value = await db.KeyValuesBinary.Where(s => s.GrainKeyId.Equals(primaryKey)).Select(s => s.BinaryContent).SingleOrDefaultAsync();
                if(value !=null)
                    data = SerializationManager.DeserializeFromByteArray<Dictionary<string, object>>(value);
            }
            grainState.SetAll(data);

            grainState.Etag = Guid.NewGuid().ToString();
        }

        /// <summary> Write state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#WriteStateAsync"/>
        public async Task WriteStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();
            if (Log.IsVerbose3)
            {
                Log.Verbose3((int) SimpleSQLServerProviderErrorCodes.SimpleSQLServerProvide_WritingData,
                    "Writing: GrainType={0} PrimaryKey={1} Grainid={2} ETag={3} to Database={4}",
                    grainType, primaryKey, grainReference, grainState.Etag, this.sqlconnBuilder.DataSource + "." + this.sqlconnBuilder.InitialCatalog);
            }
            var data = grainState.AsDictionary();

            byte[] payload = SerializationManager.SerializeToByteArray(data);

            //await redisDatabase.StringSetAsync(primaryKey, json);
            var kvb = new KeyValueBinary()
            {
                BinaryContent = payload,
                GrainKeyId = primaryKey,
            };
            //var value = await db.GetObjectContext().SaveOrUpdate(kvb);
            var entity = new KeyValueBinary() { GrainKeyId = primaryKey };
            using (var db = new KeyValueDbContext(this.sqlconnBuilder.ConnectionString))
            {
                db.KeyValuesBinary.Attach(entity);
                db.KeyValuesBinary.Add(entity);
                await db.SaveChangesAsync();
            }
        }

        /// <summary> Clear state data function for this storage provider. </summary>
        /// <remarks>
        /// </remarks>
        /// <see cref="IStorageProvider#ClearStateAsync"/>
        public async Task ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();
            if (Log.IsVerbose3)
            {
                Log.Verbose3((int) SimpleSQLServerProviderErrorCodes.SimpleSQLServerStorageProvider_ClearingData,
                    "Clearing: GrainType={0} Pk={1} Grainid={2} ETag={3} DeleteStateOnClear={4} from Table={5}",
                    grainType, primaryKey, grainReference, grainState.Etag, this.sqlconnBuilder.DataSource + "." + this.sqlconnBuilder.InitialCatalog);
            }
            //remove from cache
            //redisDatabase.KeyDelete(primaryKey);
            var entity = new KeyValueBinary() { GrainKeyId = primaryKey };
            using (var db = new KeyValueDbContext(this.sqlconnBuilder.ConnectionString))
            {
                db.KeyValuesBinary.Attach(entity);
                db.KeyValuesBinary.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }

    public static class Helper
    {
        public static void SaveOrUpdate<TEntity>    (this ObjectContext context, TEntity entity)    where TEntity : class
        {
            ObjectStateEntry stateEntry = null;
            context.ObjectStateManager
                .TryGetObjectStateEntry(entity, out stateEntry);

            var objectSet = context.CreateObjectSet< TEntity>();
            if (stateEntry == null || stateEntry.EntityKey.IsTemporary)
            {
                objectSet.AddObject(entity);
            }

            else if (stateEntry.State == EntityState.Detached)
            {
                objectSet.Attach(entity);
                context.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
            }
        }
    }




}
