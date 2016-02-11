using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.TestingHost;
using System.Diagnostics;
using System.IO;
using SimpleGrainInterfaces;
using System.Threading.Tasks;

namespace SimpleSQLServerStorage.Tests
{
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Orleans.StorageProviders.SimpleSQLServerStorage.dll")]
    [DeploymentItem("SimpleGrains.dll")]
    [TestClass]
    public class GrainStorageTests : TestingSiloHost
    {
        private readonly TimeSpan timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

        public GrainStorageTests()
            : base(new TestingSiloOptions
            {
                StartFreshOrleans = true,
                SiloConfigFile = new FileInfo("OrleansConfigurationForTesting.xml"),
            },
            new TestingClientOptions()
            {
                ClientConfigFile = new FileInfo("ClientConfigurationForTesting.xml")
            })
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            StopAllSilos();
        }



        [TestMethod]
        public async Task TestMethodGetAGrainTest()
        {
            var g = GrainFactory.GetGrain<IMyGrain>(0);

            await g.SaveSomething();
        }
    }
}
