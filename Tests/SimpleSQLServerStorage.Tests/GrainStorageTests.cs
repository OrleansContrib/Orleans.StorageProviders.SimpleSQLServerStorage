using System;
using Orleans.TestingHost;
using System.Diagnostics;
using System.IO;
using SimpleGrainInterfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Orleans;
using Orleans.Streams;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Xunit.Abstractions;
using Xunit;

namespace SimpleSQLServerStorage.Tests
{
    //[DeploymentItem("ClientConfigurationForTesting.xml")]
    //[DeploymentItem("OrleansConfigurationForTesting.xml")]
    //[DeploymentItem("OrleansProviders.dll")]
    //[DeploymentItem("Orleans.StorageProviders.SimpleSQLServerStorage.dll")]
    //[DeploymentItem("SimpleGrains.dll")]
    //// EF is creating this file for us     [DeploymentItem("basic.mdf")]
    //[TestClass]
    public class GrainStorageTests : IDisposable
    {
        #region Orleans Stuff
        private static TestCluster testingCluster;

        public Logger Logger
        {
            get { return GrainClient.Logger; }
        }


        private readonly TimeSpan _timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

        private readonly ITestOutputHelper output;

        public GrainStorageTests(ITestOutputHelper output)
        {
            this.output = output;

            testingCluster = CreateTestCluster();
            testingCluster.Deploy();
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Optional. 
                    // By default, the next test class which uses TestignSiloHost will
                    // cause a fresh Orleans silo environment to be created.
                    testingCluster.StopAllSilos();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FacilityGrainTest() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion



        //[ClassInitialize]
        //public static void SetUp(TestContext context)
        //{
        //    testingCluster = CreateTestCluster(context);
        //    testingCluster.Deploy();
        //}

        //[ClassCleanup]
        //public static void ClassCleanup()
        //{
        //    // Optional. 
        //    // By default, the next test class which uses TestignSiloHost will
        //    // cause a fresh Orleans silo environment to be created.
        //    testingCluster.StopAllSilos();
        //}

        //[TestInitialize]


        public static TestCluster CreateTestCluster()
        {
            var options = new TestClusterOptions(3);

            //options.ClusterConfiguration.AddMemoryStorageProvider("Default");
            //options.ClusterConfiguration.AddSimpleMessageStreamProvider(STREAMPROVIDERNAME);

            options.ClusterConfiguration.Globals.ClientDropTimeout = TimeSpan.FromSeconds(5);
            options.ClusterConfiguration.Defaults.DefaultTraceLevel = Orleans.Runtime.Severity.Verbose3;
            options.ClusterConfiguration.Defaults.TraceLevelOverrides.Add(new Tuple<string, Severity>("StreamConsumerExtension", Severity.Verbose3));

            options.ClusterConfiguration.AddMemoryStorageProvider("memtester");
            options.ClusterConfiguration.AddSimpleSQLStorageProvider("basic",
                string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "basic.mdf")), "true");

            //options.ClientConfiguration.AddSimpleMessageStreamProvider(STREAMPROVIDERNAME);
            options.ClientConfiguration.DefaultTraceLevel = Orleans.Runtime.Severity.Verbose3;

            return new TestCluster(options);
        }
        #endregion



        [Fact]
        public async Task TestMethodGetAGrainTest()
        {
            var g = testingCluster.GrainFactory.GetGrain<IMyGrain>(0);

            await g.SaveSomething(1, "ff", Guid.NewGuid(), DateTime.Now, new int[] { 1, 2, 3, 4, 5 });
        }



        [Fact]
        public async Task TestGrains()
        {
            var rnd = new Random();
            var rndId1 = rnd.Next();
            var rndId2 = rnd.Next();



            // insert your grain test code here
            var grain = testingCluster.GrainFactory.GetGrain<IMyGrain>(rndId1);

            var thing4 = new DateTime();
            var thing3 = Guid.NewGuid();
            var thing1 = 1;
            var thing2 = "ggggggggggggggggg";
            var things = new List<int> { 5, 6, 7, 8, 9 };

            await grain.SaveSomething(thing1, thing2, thing3, thing4, things);

            Assert.Equal(thing1, await grain.GetThing1());
            Assert.Equal(thing2, await grain.GetThing2());
            Assert.Equal(thing3, await grain.GetThing3());
            Assert.Equal(thing4, await grain.GetThing4());
            var res = await grain.GetThings1();
            Assert.Equal(things, res.ToList());
        }



        [Fact]
        public async Task StateTestGrainTest()
        {
            var rnd = new Random();
            var rndId1 = rnd.Next();
            var rndId2 = rnd.Next();



            // insert your grain test code here
            var grain = testingCluster.GrainFactory.GetGrain<IStateTestGrain>(rndId1);

            var thing4 = new DateTime();
            var thing3 = Guid.NewGuid();
            var thing1 = 1;
            var thing2 = "ggggggggggggggggg";
            var things = new List<int> { 5, 6, 7, 8, 9 };

            await grain.SaveSomething(thing1, thing2, thing3, thing4, things);

            Assert.Equal(thing1, await grain.GetThing1());
            Assert.Equal(thing2, await grain.GetThing2());
            Assert.Equal(thing3, await grain.GetThing3());
            Assert.Equal(thing4, await grain.GetThing4());
            var res = await grain.GetThings1();
            Assert.Equal(things, res.ToList());
        }

        [Fact]
        public async Task ClearStateTest()
        {
            var rnd = new Random();
            var rndId1 = rnd.Next();

            var grain = testingCluster.GrainFactory.GetGrain<IStateTestGrain>(rndId1);

            await grain.SaveSomething(5, "ggg", Guid.NewGuid(), DateTime.Now, new int[] { 1, 2, 2, 3 });

            await grain.ClearTheState();
        }


    }
}
