using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    [DbConfigurationType(typeof(KeyValueDbConfiguration))]
    class KeyValueDbContext : DbContext
    {
        public KeyValueDbContext(string connString)
        : base(connString)
        { }


        //public DbSet<StorageName> StorageName { get; set; }
        public DbSet<KeyValueBinary> KeyValuesBinary { get; set; }
        //public DbSet<KeyValueJson> KeyValuesJson { get; set; }
    }
}
