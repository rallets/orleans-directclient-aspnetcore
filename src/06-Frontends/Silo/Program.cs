using GrainInterfaces.Serialization.ProtobufNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using OrleansSilo.Products;
using ProtoBuf.Meta;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansSilo
{
    public class Program
    {
        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);

        static ISiloHost silo;
        static bool siloStopping = false;
        static readonly object syncLock = new object();

        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                SetupApplicationShutdown();

                silo = CreateSilo();
                await silo.StartAsync();

                CreateClient(silo);

                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                // Wait for the silo to completely shutdown before exiting. 
                _siloStopped.WaitOne();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        static void SetupApplicationShutdown()
        {
            /// Capture the user pressing Ctrl+C
            Console.CancelKeyPress += (s, a) =>
            {
                /// Prevent the application from crashing ungracefully.
                a.Cancel = true;
                /// Don't allow the following code to repeat if the user presses Ctrl+C repeatedly.
                lock (syncLock)
                {
                    if (!siloStopping)
                    {
                        siloStopping = true;
                        Task.Run(StopSilo).Ignore();
                    }
                }
                /// Event handler execution exits immediately, leaving the silo shutdown running on a background thread,
                /// but the app doesn't crash because a.Cancel has been set = true
            };
        }

        private static ISiloHost CreateSilo()
        {
            // define the cluster configuration
            return new SiloHostBuilder()
                .UseLocalhostClustering()
                .EnableDirectClient()
                .Configure<SerializationProviderOptions>(_ =>
                {
                    _.SerializationProviders.Add(typeof(Orleans.Serialization.ProtobufNet.ProtobufNetSerializer).GetTypeInfo());
                    RuntimeTypeModel.Default.Add(typeof(DateTimeOffset), false).SetSurrogate(typeof(DateTimeOffsetSurrogate));
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansDirectClientTest";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ProductsGrain).Assembly).WithReferences())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })

                .UseDashboard(options => { })
                 .UsePerfCounterEnvironmentStatistics()

                .AddAzureTableGrainStorage("TableStore", options => options.ConnectionString = "UseDevelopmentStorage=true")
                .AddAzureBlobGrainStorage("BlobStore", options => options.ConnectionString = "UseDevelopmentStorage=true")

            .Build();
        }

        static async Task StopSilo()
        {
            await silo.StopAsync();
            _siloStopped.Set();
        }

        private static void CreateClient(ISiloHost silo)
        {
            string _allowFrontendOrigin = "_allowFrontendOrigin";

            var client = silo.Services.GetRequiredService<IClusterClient>();

            var webHost = new WebHostBuilder()
                            .ConfigureServices(services =>
                                services
                                    .AddSingleton<IGrainFactory>(client)
                                    .AddSingleton<IClusterClient>(client))
                            .UseStartup<WebApi.Startup>()
                            // Other ASP.NET configuration...
                            .ConfigureServices(services =>
                            {
                                services.AddCors(options =>
                                {
                                    options.AddPolicy(_allowFrontendOrigin,
                                    builder =>
                                    {
                                        builder.AllowAnyOrigin() // .WithOrigins("http://localhost:4200")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                                    });
                                });
                            })
                            .Build();
        }
    }
}
