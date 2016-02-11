using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    class KeyValueBinary
    {
        public int KeyValueBinaryId { get; set; }
        public string BinaryContent { get; set; }

        public int StorageNameId { get; set; }
        public virtual StorageName StorageName { get; set; }
    }
}
