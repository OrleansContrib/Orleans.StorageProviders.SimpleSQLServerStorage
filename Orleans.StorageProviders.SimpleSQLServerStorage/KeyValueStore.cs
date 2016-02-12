using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    class KeyValueStore
    {
        [Key]
        public string GrainKeyId { get; set; }
        public byte[] BinaryContent { get; set; }
        public string JsonContext { get; set; }

        //public int StorageNameId { get; set; }
        //public virtual StorageName StorageName { get; set; }
    }
}
