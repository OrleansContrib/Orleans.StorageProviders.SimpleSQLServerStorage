using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGrainInterfaces
{
    public interface IStreamerGrain : IGrainWithGuidKey
    {
        Task Produce();

    }
}
