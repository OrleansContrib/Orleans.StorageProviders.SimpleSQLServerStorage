using Orleans;
using Orleans.Providers;
using SimpleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGrains
{
    [StorageProvider(ProviderName="basic")]
    public class MyGrain : Grain<MyGrainState>, IMyGrain
    {
        
        public Task SaveSomething()
        {
            State.Thing1 = State.Thing1 + 1;
            State.Thing2 = State.Thing2 + "1";
            return this.WriteStateAsync();
        }
    }
}
