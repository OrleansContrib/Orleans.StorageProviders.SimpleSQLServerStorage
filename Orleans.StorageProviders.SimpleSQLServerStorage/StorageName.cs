using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    class StorageName
    {
        public int StorageNameId { get; set; }
        public string Name { get; set; }

        public virtual List<KeyValueBinary> BinaryKV { get; set; }
        public virtual List<KeyValueJson> JsonKV { get; set; }

    }
}
