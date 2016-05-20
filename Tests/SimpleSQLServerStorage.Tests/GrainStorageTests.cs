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
using SimpleGrains;
using Orleans.Runtime;
using System.Globalization;

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
        public static TestingSiloHost testingHost;

        private readonly TimeSpan timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        protected static readonly Random random = new Random();

        public Logger logger
        {
            get { return GrainClient.Logger; }
        }

        private readonly double timingFactor;


        public GrainStorageTests()
        {
            timingFactor = CalibrateTimings();
        }



        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            testingHost = new TestingSiloHost(new TestingSiloOptions
            {
                SiloConfigFile = new FileInfo("OrleansConfigurationForTesting.xml"),
                StartFreshOrleans = true,

                AdjustConfig = config =>
                {
                    config.Globals.RegisterStorageProvider<Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage>(providerName: "basic", 
                        properties:
                        new Dictionary<string, string>                        {
                            { "ConnectionString" , string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes", Path.Combine(context.DeploymentDirectory, "basic.mdf"))},
                            { "TableName", "basic"},
                            { "UseJsonFormat", "both" }
                        });
                    config.Globals.RegisterStorageProvider<Orleans.StorageProviders.SimpleSQLServerStorage.SimpleSQLServerStorage>(providerName: "SimpleSQLStore",
                        properties:
                        new Dictionary<string, string>                        {
                            { "ConnectionString" , string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes", Path.Combine(context.DeploymentDirectory, "SimpleSQLStore.mdf"))},
                            { "TableName", "basic"},
                            { "UseJsonFormat", "both" }
                        });
                }
            },
            new TestingClientOptions()
            {
                ClientConfigFile = new FileInfo("ClientConfigurationForTesting.xml")
            });
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Optional. 
            // By default, the next test class which uses TestignSiloHost will
            // cause a fresh Orleans silo environment to be created.
            testingHost.StopAllSilos();
        }



        [TestMethod]
        public async Task TestMethodGetAGrainTest()
        {
            var g = testingHost.GrainFactory.GetGrain<IMyGrain>(0);

            await g.SaveSomething(1, "ff", Guid.NewGuid(), DateTime.Now, new int[] { 1, 2, 3, 4, 5 });
        }



        [TestMethod]
        public async Task TestGrains()
        {
            var rnd = new Random();
            var rndId1 = rnd.Next();
            var rndId2 = rnd.Next();



            // insert your grain test code here
            var grain = testingHost.GrainFactory.GetGrain<IMyGrain>(rndId1);

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
            var grain = testingHost.GrainFactory.GetGrain<IStateTestGrain>(rndId1);

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
        public async Task SimpleSQLStore_Delete()
        {
            Guid id = Guid.NewGuid();
            ISimpleSQLStorageTestGrain grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain>(id);

            await grain.DoWrite(1);

            await grain.DoDelete();

            int val = await grain.GetValue(); // Should this throw instead?
            Assert.AreEqual(0, val, "Value after Delete");

            await grain.DoWrite(2);

            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Delete + New Write");
        }

        [TestMethod]
        public async Task Grain_SimpleSQLStore_Read()
        {
            Guid id = Guid.NewGuid();
            ISimpleSQLStorageTestGrain grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain>(id);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");
        }

        [TestMethod]
        public async Task Grain_GuidKey_SimpleSQLStore_Read_Write()
        {
            Guid id = Guid.NewGuid();
            ISimpleSQLStorageTestGrain grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain>(id);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);
            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();

            Assert.AreEqual(2, val, "Value after Re-Read");
        }

        [TestMethod]
        public async Task Grain_LongKey_SimpleSQLStore_Read_Write()
        {
            long id = random.Next();
            ISimpleSQLStorageTestGrain_LongKey grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain_LongKey>(id);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);
            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();

            Assert.AreEqual(2, val, "Value after Re-Read");
        }

        [TestMethod]
        public async Task Grain_LongKeyExtended_SimpleSQLStore_Read_Write()
        {
            long id = random.Next();
            string extKey = random.Next().ToString(CultureInfo.InvariantCulture);

            ISimpleSQLStorageTestGrain_LongExtendedKey
                grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain_LongExtendedKey>(id, extKey, null);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);
            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();
            Assert.AreEqual(2, val, "Value after DoRead");

            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Re-Read");

            string extKeyValue = await grain.GetExtendedKeyValue();
            Assert.AreEqual(extKey, extKeyValue, "Extended Key");
        }

        [TestMethod]
        public async Task Grain_GuidKeyExtended_SimpleSQLStore_Read_Write()
        {
            var id = Guid.NewGuid();
            string extKey = random.Next().ToString(CultureInfo.InvariantCulture);

            ISimpleSQLStorageTestGrain_GuidExtendedKey
                grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain_GuidExtendedKey>(id, extKey, null);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);
            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();
            Assert.AreEqual(2, val, "Value after DoRead");

            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Re-Read");

            string extKeyValue = await grain.GetExtendedKeyValue();
            Assert.AreEqual(extKey, extKeyValue, "Extended Key");
        }

        [TestMethod]
        public async Task Grain_Generic_SimpleSQLStore_Read_Write()
        {
            long id = random.Next();

            ISimpleSQLStorageGenericGrain<int> grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageGenericGrain<int>>(id);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);
            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();

            Assert.AreEqual(2, val, "Value after Re-Read");
        }

        [TestMethod]
        public async Task Grain_Generic_SimpleSQLStore_DiffTypes()
        {
            long id1 = random.Next();
            long id2 = id1;
            long id3 = id1;

            ISimpleSQLStorageGenericGrain<int> grain1 = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageGenericGrain<int>>(id1);

            ISimpleSQLStorageGenericGrain<string> grain2 = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageGenericGrain<string>>(id2);

            ISimpleSQLStorageGenericGrain<double> grain3 = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageGenericGrain<double>>(id3);

            int val1 = await grain1.GetValue();
            Assert.AreEqual(0, val1, "Initial value - 1");

            string val2 = await grain2.GetValue();
            Assert.AreEqual(null, val2, "Initial value - 2");

            double val3 = await grain3.GetValue();
            Assert.AreEqual(0.0, val3, "Initial value - 3");

            int expected1 = 1;
            await grain1.DoWrite(expected1);
            val1 = await grain1.GetValue();
            Assert.AreEqual(expected1, val1, "Value after Write#1 - 1");

            string expected2 = "Three";
            await grain2.DoWrite(expected2);
            val2 = await grain2.GetValue();
            Assert.AreEqual(expected2, val2, "Value after Write#1 - 2");

            double expected3 = 5.1;
            await grain3.DoWrite(expected3);
            val3 = await grain3.GetValue();
            Assert.AreEqual(expected3, val3, "Value after Write#1 - 3");

            val1 = await grain1.GetValue();
            Assert.AreEqual(expected1, val1, "Value before Write#2 - 1");
            expected1 = 2;
            await grain1.DoWrite(expected1);
            val1 = await grain1.GetValue();
            Assert.AreEqual(expected1, val1, "Value after Write#2 - 1");
            val1 = await grain1.DoRead();
            Assert.AreEqual(expected1, val1, "Value after Re-Read - 1");

            val2 = await grain2.GetValue();
            Assert.AreEqual(expected2, val2, "Value before Write#2 - 2");
            expected2 = "Four";
            await grain2.DoWrite(expected2);
            val2 = await grain2.GetValue();
            Assert.AreEqual(expected2, val2, "Value after Write#2 - 2");
            val2 = await grain2.DoRead();
            Assert.AreEqual(expected2, val2, "Value after Re-Read - 2");

            val3 = await grain3.GetValue();
            Assert.AreEqual(expected3, val3, "Value before Write#2 - 3");
            expected3 = 6.2;
            await grain3.DoWrite(expected3);
            val3 = await grain3.GetValue();
            Assert.AreEqual(expected3, val3, "Value after Write#2 - 3");
            val3 = await grain3.DoRead();
            Assert.AreEqual(expected3, val3, "Value after Re-Read - 3");
        }


        [TestMethod]
        public async Task Grain_SimpleSQLStore_SiloRestart()
        {
            var initialServiceId = testingHost.Globals.ServiceId;
            var initialDeploymentId = testingHost.DeploymentId;

            Console.WriteLine("DeploymentId={0} ServiceId={1}", testingHost.DeploymentId, testingHost.Globals.ServiceId);

            Guid id = Guid.NewGuid();
            ISimpleSQLStorageTestGrain grain = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain>(id);

            int val = await grain.GetValue();

            Assert.AreEqual(0, val, "Initial value");

            await grain.DoWrite(1);

            Console.WriteLine("About to reset Silos");
            testingHost.RestartDefaultSilos(true);
            Console.WriteLine("Silos restarted");

            Console.WriteLine("DeploymentId={0} ServiceId={1}", testingHost.DeploymentId, testingHost.Globals.ServiceId);
            Assert.AreEqual(initialServiceId, testingHost.Globals.ServiceId, "ServiceId same after restart.");
            Assert.AreNotEqual(initialDeploymentId, testingHost.DeploymentId, "DeploymentId different after restart.");

            val = await grain.GetValue();
            Assert.AreEqual(1, val, "Value after Write-1");

            await grain.DoWrite(2);
            val = await grain.GetValue();
            Assert.AreEqual(2, val, "Value after Write-2");

            val = await grain.DoRead();

            Assert.AreEqual(2, val, "Value after Re-Read");
        }


        [TestMethod]
        public void Persistence_Perf_Activate()
        {
            const string testName = "Persistence_Perf_Activate";
            int n = 1000;
            TimeSpan target = TimeSpan.FromMilliseconds(200 * n);

            // Timings for Activate
            RunPerfTest(n, testName, target,
                grainSimpleSQLStore => grainSimpleSQLStore.GetValue());
        }

        [TestMethod]
        public void Persistence_Perf_Write()
        {
            const string testName = "Persistence_Perf_Write";
            int n = 1000;
            TimeSpan target = TimeSpan.FromMilliseconds(2000 * n);

            // Timings for Write
            RunPerfTest(n, testName, target,
                grainSimpleSQLStore => grainSimpleSQLStore.DoWrite(n));
        }

        [TestMethod]
        public void Persistence_Perf_Write_Reread()
        {
            const string testName = "Persistence_Perf_Write_Read";
            int n = 1000;
            TimeSpan target = TimeSpan.FromMilliseconds(2000 * n);

            // Timings for Write
            RunPerfTest(n, testName + "--Write", target,
                grainSimpleSQLStore => grainSimpleSQLStore.DoWrite(n));

            // Timings for Activate
            RunPerfTest(n, testName + "--ReRead", target,
                grainSimpleSQLStore => grainSimpleSQLStore.DoRead());
        }


        //[TestMethod]
        //public void Persistence_Silo_StorageProvider_SimpleSQL(Type providerType)
        //{
        //    List<SiloHandle> silos = testingHost.GetActiveSilos().ToList();
        //    foreach (var silo in silos)
        //    {
        //        string provider = providerType.FullName;
        //        List<string> providers = silo.Silo.TestHook.GetStorageProviderNames().ToList();
        //        Assert.IsTrue(providers.Contains(provider), "No storage provider found: {0}", provider);
        //    }
        //}

        #region Utility functions
        // ---------- Utility functions ----------

        void RunPerfTest(int n, string testName, TimeSpan target,
            Func<ISimpleSQLStorageTestGrain, Task> actionSimpleSQL)
        {
            ISimpleSQLStorageTestGrain[] simpleSQLStoreGrains = new ISimpleSQLStorageTestGrain[n];

            for (int i = 0; i < n; i++)
            {
                Guid id = Guid.NewGuid();
                simpleSQLStoreGrains[i] = GrainClient.GrainFactory.GetGrain<ISimpleSQLStorageTestGrain>(id);
            }

            TimeSpan baseline = new TimeSpan(0, 0, 5), elapsed;

            elapsed = TimeRun(n, baseline, testName + " (SimpleSQL Store)",
                () => RunIterations(testName, n, i => actionSimpleSQL(simpleSQLStoreGrains[i])));

            if (elapsed > target.Multiply(timingFactor))
            {
                string msg = string.Format("{0}: Elapsed time {1} exceeds target time {2}", testName, elapsed, target);

                if (elapsed > target.Multiply(2.0 * timingFactor))
                {
                    Assert.Fail(msg);
                }
                else
                {
                    //this is from xunit
                    //throw new SkipException(msg);
                }
            }
        }

        private void RunIterations(string testName, int n, Func<int, Task> action)
        {
            List<Task> promises = new List<Task>();
            Stopwatch sw = Stopwatch.StartNew();
            // Fire off requests in batches
            for (int i = 0; i < n; i++)
            {
                var promise = action(i);
                promises.Add(promise);
                if ((i % 100) == 0 && i > 0)
                {
                    Task.WaitAll(promises.ToArray(), 30000);
                    promises.Clear();
                    //output.WriteLine("{0} has done {1} iterations  in {2} at {3} RPS",
                    //                  testName, i, sw.Elapsed, i / sw.Elapsed.TotalSeconds);
                }
            }
            Task.WaitAll(promises.ToArray(), 30000);
            sw.Stop();
            Console.WriteLine("{0} completed. Did {1} iterations in {2} at {3} RPS",
                              testName, n, sw.Elapsed, n / sw.Elapsed.TotalSeconds);
        }
        #endregion

        public static TimeSpan TimeRun(int numIterations, TimeSpan baseline, string what, Action action)
        {
            var stopwatch = new Stopwatch();

            long startMem = GC.GetTotalMemory(true);
            stopwatch.Start();

            action();

            stopwatch.Stop();
            long stopMem = GC.GetTotalMemory(false);
            long memUsed = stopMem - startMem;
            TimeSpan duration = stopwatch.Elapsed;

            string timeDeltaStr = "";
            if (baseline > TimeSpan.Zero)
            {
                double delta = (duration - baseline).TotalMilliseconds / baseline.TotalMilliseconds;
                timeDeltaStr = String.Format("-- Change = {0}%", 100.0 * delta);
            }
            Console.WriteLine("Time for {0} loops doing {1} = {2} {3} Memory used={4}", numIterations, what, duration, timeDeltaStr, memUsed);
            return duration;
        }

        public static double CalibrateTimings()
        {
            const int NumLoops = 10000;
            TimeSpan baseline = TimeSpan.FromTicks(80); // Baseline from jthelin03D
            int n;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < NumLoops; i++)
            {
                n = i;
            }
            sw.Stop();
            double multiple = 1.0 * sw.ElapsedTicks / baseline.Ticks;
            Console.WriteLine("CalibrateTimings: {0} [{1} Ticks] vs {2} [{3} Ticks] = x{4}",
                sw.Elapsed, sw.ElapsedTicks,
                baseline, baseline.Ticks,
                multiple);
            return multiple > 1.0 ? multiple : 1.0;
        }

    }



    public static class ExtensionStuff
    { 


        public static TimeSpan Multiply(this TimeSpan timeSpan, double value)
        {
            double ticksD = checked((double) timeSpan.Ticks * value);
            long ticks = checked((long) ticksD);
            return TimeSpan.FromTicks(ticks);
        }



    }
}
