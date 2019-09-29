using GrainInterfaces.Serialization.ProtobufNet;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using ProtoBuf.Meta;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using TestClient;

namespace OrleansSilo
{
    public class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await ConnectClient())
                {
                    await RunClientTestsSetup(client);
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<SerializationProviderOptions>(_ =>
                {
                    _.SerializationProviders.Add(typeof(Orleans.Serialization.ProtobufNet.ProtobufNetSerializer).GetTypeInfo());
                    RuntimeTypeModel.Default.Add(typeof(DateTimeOffset), false).SetSurrogate(typeof(DateTimeOffsetSurrogate));
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "TestInventoryService";
                })
                .Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false)
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host");
            return client;
        }

        private static async Task RunClientTestsSetup(IClusterClient client)
        {
            int numProducts = 200;

            var products = await Test_Products.GetAll(client);
            if (products.Length < numProducts)
            {
                await Test_Products.AddBatch(client, numProducts - products.Length, 100);
            }

            products = await Test_Products.GetAll(client);

            Debug.Assert(products.Length >= numProducts);
        }
    }
}
