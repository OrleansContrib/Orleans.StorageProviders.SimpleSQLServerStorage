using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace SimpleSQLServerStorage.Tests
{
    [DeploymentItem("ClientConfigurationForTesting.xml")]
    [DeploymentItem("OrleansConfigurationForTesting.xml")]
    [DeploymentItem("OrleansProviders.dll")]
    [DeploymentItem("Orleans.StorageProviders.SimpleSQLServerStorage.dll")]
    [DeploymentItem("SimpleGrains.dll")]
    // EF is creating this file for us     [DeploymentItem("basic.mdf")]
    [TestClass]
    public class GrainStorageTests
    {

        const string STREAMPROVIDERNAME = @"streamprovidername";




        #region Orleans Stuff
        private static TestCluster testingCluster;

        public Logger Logger
        {
            get { return GrainClient.Logger; }
        }


        private readonly TimeSpan _timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        public GrainStorageTests()
        {
        }


        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            testingCluster = CreateTestCluster(context);
            testingCluster.Deploy();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            testingCluster.StopAllSilos();
        }


        public static TestCluster CreateTestCluster(TestContext context)
        {
            var options = new TestClusterOptions(10);

            //options.ClusterConfiguration.AddMemoryStorageProvider("Default");
            //options.ClusterConfiguration.AddSimpleMessageStreamProvider(STREAMPROVIDERNAME);

            options.ClusterConfiguration.Globals.ClientDropTimeout = TimeSpan.FromSeconds(5);
            options.ClusterConfiguration.Defaults.DefaultTraceLevel = Orleans.Runtime.Severity.Verbose;
            options.ClusterConfiguration.Defaults.TraceLevelOverrides.Add(new Tuple<string, Severity>("StreamConsumerExtension", Severity.Verbose3));

            options.ClusterConfiguration.Globals.RegisterStorageProvider<Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage>(
                providerName: "basic", 
                properties:
                        new Dictionary<string, string>                        {
                            { "ConnectionString" , string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes", Path.Combine(context.DeploymentDirectory, "basic.mdf"))},
                            { "TableName", "basic"},
                            { "UseJsonFormat", "both" }
                        });

            //options.ClientConfiguration.AddSimpleMessageStreamProvider(STREAMPROVIDERNAME);
            options.ClientConfiguration.DefaultTraceLevel = Orleans.Runtime.Severity.Verbose;

            return new TestCluster(options);
        }
        #endregion



        [TestMethod]
        public async Task TestMethodGetAGrainTest()
        {
            var g = testingCluster.GrainFactory.GetGrain<IMyGrain>(0);

            await g.SaveSomething(1, "ff", Guid.NewGuid(), DateTime.Now, new int[] { 1, 2, 3, 4, 5 });
        }



        [TestMethod]
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

            Assert.AreEqual(thing1, await grain.GetThing1());
            Assert.AreEqual(thing2, await grain.GetThing2());
            Assert.AreEqual(thing3, await grain.GetThing3());
            Assert.AreEqual(thing4, await grain.GetThing4());
            var res = await grain.GetThings1();
            CollectionAssert.AreEqual(things, res.ToList());
        }



        [TestMethod]
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

            Assert.AreEqual(thing1, await grain.GetThing1());
            Assert.AreEqual(thing2, await grain.GetThing2());
            Assert.AreEqual(thing3, await grain.GetThing3());
            Assert.AreEqual(thing4, await grain.GetThing4());
            var res = await grain.GetThings1();
            CollectionAssert.AreEqual(things, res.ToList());
        }


    }
}
