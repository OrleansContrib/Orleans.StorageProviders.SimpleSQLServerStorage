using SimpleGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGrain
{
    public class StreamerGrain : IStreamerGrain
    {
        public Task Produce()
        {
            throw new NotImplementedException();
        }
    }
}
