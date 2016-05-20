using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGrains
{
    public interface ISimpleSQLStorageTestGrain : IGrainWithGuidKey
    {
        Task<int> GetValue();
        Task DoWrite(int val);
        Task<int> DoRead();
        Task DoDelete();
    }

    public interface ISimpleSQLStorageGenericGrain<T> : IGrainWithIntegerKey
    {
        Task<T> GetValue();
        Task DoWrite(T val);
        Task<T> DoRead();
        Task DoDelete();
    }

    public interface ISimpleSQLStorageTestGrain_GuidExtendedKey : IGrainWithGuidCompoundKey
    {
        Task<string> GetExtendedKeyValue();
        Task<int> GetValue();
        Task DoWrite(int val);
        Task<int> DoRead();
        Task DoDelete();
    }

    public interface ISimpleSQLStorageTestGrain_LongKey : IGrainWithIntegerKey
    {
        Task<int> GetValue();
        Task DoWrite(int val);
        Task<int> DoRead();
        Task DoDelete();
    }

    public interface ISimpleSQLStorageTestGrain_LongExtendedKey : IGrainWithIntegerCompoundKey
    {
        Task<string> GetExtendedKeyValue();
        Task<int> GetValue();
        Task DoWrite(int val);
        Task<int> DoRead();
        Task DoDelete();
    }

    public interface IPersistenceErrorGrain : IGrainWithGuidKey
    {
        Task<int> GetValue();
        Task DoWrite(int val);
        Task DoWriteError(int val, bool errorBeforeWrite);
        Task<int> DoRead();
        Task<int> DoReadError(bool errorBeforeRead);
    }

}
