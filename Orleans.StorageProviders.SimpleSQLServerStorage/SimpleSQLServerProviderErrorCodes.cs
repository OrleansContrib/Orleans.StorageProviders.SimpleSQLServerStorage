using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.SimpleSQLServerStorage
{
    internal enum SimpleSQLServerProviderErrorCodes
    {
        ProvidersBase = 400000,
        
        SimpleSQLServerProvideBase                      = ProvidersBase + 100,
        SimpleSQLServerProvide_DataNotFound             = SimpleSQLServerProvideBase + 1,
        SimpleSQLServerProvide_ReadingData              = SimpleSQLServerProvideBase + 2,
        SimpleSQLServerProvide_WritingData              = SimpleSQLServerProvideBase + 3,
        SimpleSQLServerProvide_Storage_Reading          = SimpleSQLServerProvideBase + 4,
        SimpleSQLServerProvide_Storage_Writing          = SimpleSQLServerProvideBase + 5,
        SimpleSQLServerProvide_Storage_DataRead         = SimpleSQLServerProvideBase + 6,
        SimpleSQLServerProvide_WriteError               = SimpleSQLServerProvideBase + 7,
        SimpleSQLServerProvide_DeleteError              = SimpleSQLServerProvideBase + 8,
        SimpleSQLServerProvide_InitProvider             = SimpleSQLServerProvideBase + 9,
        SimpleSQLServerProvide_ParamConnectionString    = SimpleSQLServerProvideBase + 10,
    }
}
