using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    class KeyValueDbContext : DbContext
    {
        public DbSet<KeyValueBinary> KeyValues { get; set; }
    }
}
