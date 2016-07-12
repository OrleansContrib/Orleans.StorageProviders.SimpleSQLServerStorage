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
using Orleans.TestingHost.Utils;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace SimpleSQLServerStorage.Tests
{
    //[DeploymentItem("ClientConfigurationForTesting.xml")]
    //[DeploymentItem("OrleansConfigurationForTesting.xml")]
    //[DeploymentItem("OrleansProviders.dll")]
    //[DeploymentItem("Orleans.StorageProviders.SimpleSQLServerStorage.dll")]
    //[DeploymentItem("SimpleGrains.dll")]
    //// EF is creating this file for us [DeploymentItem("PubSubStore.mdf")]
    //[TestClass]
    public class PubSubStoreTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        #region Orleans Stuff
        private static TestCluster testingCluster;

        public Logger Logger
        {
            get { return GrainClient.Logger; }
        }


        private readonly TimeSpan _timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

        private readonly ITestOutputHelper output;

        public PubSubStoreTests(ITestOutputHelper output)
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



        public static TestCluster CreateTestCluster()
        {
            var options = new TestClusterOptions(3);

            //options.ClusterConfiguration.AddMemoryStorageProvider("Default");
            options.ClusterConfiguration.AddSimpleMessageStreamProvider("SMSProvider");

            options.ClusterConfiguration.Globals.ClientDropTimeout = TimeSpan.FromSeconds(5);
            options.ClusterConfiguration.Defaults.DefaultTraceLevel = Orleans.Runtime.Severity.Warning;
            options.ClusterConfiguration.Defaults.TraceLevelOverrides.Add(new Tuple<string, Severity>("StreamConsumerExtension", Severity.Verbose3));


            options.ClusterConfiguration.AddMemoryStorageProvider("memtester");
            options.ClusterConfiguration.AddSimpleSQLStorageProvider("PubSubStore",
                string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PubSubStore.mdf")), "true");

            options.ClientConfiguration.AddSimpleMessageStreamProvider("SMSProvider");
            options.ClientConfiguration.DefaultTraceLevel = Orleans.Runtime.Severity.Warning;

            return new TestCluster(options);
        }
        #endregion

        [Fact]
        public async Task PubSubStoreTest()
        {
            var streamGuid = Guid.NewGuid();
            string streamNamespace = "xxxx";
            string streamProviderName = "SMSProvider";

            // get producer and consumer
            var producer = GrainClient.GrainFactory.GetGrain<IStreamerOutGrain>(Guid.NewGuid());
            var consumer = GrainClient.GrainFactory.GetGrain<IStreamerInGrain>(Guid.NewGuid());

            // setup two subscriptions
            StreamSubscriptionHandle<int> firstSubscriptionHandle = await consumer.BecomeConsumer(streamGuid, streamNamespace, streamProviderName);
            StreamSubscriptionHandle<int> secondSubscriptionHandle = await consumer.BecomeConsumer(streamGuid, streamNamespace, streamProviderName);

            // produce some messages
            await producer.BecomeProducer(streamGuid, streamNamespace, streamProviderName);

            await producer.StartPeriodicProducing();
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            await producer.StopPeriodicProducing();

            // check
            await TestingUtils.WaitUntilAsync(lastTry => CheckCounters(producer, consumer, 2, lastTry), Timeout);

            // unsubscribe
            await consumer.StopConsuming(firstSubscriptionHandle);
            await consumer.StopConsuming(secondSubscriptionHandle);
        }


        private async Task<bool> CheckCounters(IStreamerOutGrain producer, IStreamerInGrain consumer, int consumerCount, bool assertIsTrue)
        {
            var numProduced = await producer.GetNumberProduced();
            var numConsumed = await consumer.GetNumberConsumed();
            if (assertIsTrue)
            {
                Assert.True(numConsumed.Values.All(v => v.Item2 == 0), "Errors");
                Assert.True(numProduced > 0, "Events were not produced");
                Assert.Equal(consumerCount, numConsumed.Count);//, "Incorrect number of consumers");
                foreach (int consumed in numConsumed.Values.Select(v => v.Item1))
                {
                    Assert.Equal(numProduced, consumed);//, "Produced and consumed counts do not match");
                }
            }
            else if (numProduced <= 0 || // no events produced?
                     consumerCount != numConsumed.Count || // subscription counts are wrong?
                     numConsumed.Values.Any(consumedCount => consumedCount.Item1 != numProduced) ||// consumed events don't match produced events for any subscription?
                     numConsumed.Values.Any(v => v.Item2 != 0)) // stream errors
            {
                if (numProduced <= 0)
                {
                    Logger.Info("numProduced <= 0: Events were not produced");
                }
                if (consumerCount != numConsumed.Count)
                {
                    Logger.Info("consumerCount != numConsumed.Count: Incorrect number of consumers. consumerCount = {0}, numConsumed.Count = {1}",
                        consumerCount, numConsumed.Count);
                }
                foreach (var consumed in numConsumed)
                {
                    if (numProduced != consumed.Value.Item1)
                    {
                        Logger.Info("numProduced != consumed: Produced and consumed counts do not match. numProduced = {0}, consumed = {1}",
                            numProduced, consumed.Key.HandleId + " -> " + consumed.Value);
                        //numProduced, Utils.DictionaryToString(numConsumed));
                    }
                }
                return false;
            }
            Logger.Info("All counts are equal. numProduced = {0}, numConsumed = {1}", numProduced,
                Utils.EnumerableToString(numConsumed, kvp => kvp.Key.HandleId.ToString() + "->" + kvp.Value.ToString()));
            return true;
        }


        [Fact]
        public async Task StreamingPubSubStoreTest()
        {
            var strmId = Guid.NewGuid();

            var streamProv = GrainClient.GetStreamProvider("SMSProvider");
            IAsyncStream<int> stream = streamProv.GetStream<int>(strmId, "test1");

            StreamSubscriptionHandle<int> handle = await stream.SubscribeAsync(
                (e, t) => { return TaskDone.Done; },
                e => { return TaskDone.Done; });
        }
    }
}
