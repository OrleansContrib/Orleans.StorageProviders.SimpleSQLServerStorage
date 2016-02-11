using Orleans;
using System.Threading.Tasks;

namespace SimpleGrainInterfaces
{
    public interface IMyGrain: IGrainWithIntegerKey
    {
        Task SaveSomething();
    }
}