using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    class KeyValueJson
    {
        public int KeyValueJsonId { get; set; }
        public string JsonContent { get; set; }

        public int StorageNameId { get; set; }
        public virtual StorageName StorageName { get; set; }
    }
}
