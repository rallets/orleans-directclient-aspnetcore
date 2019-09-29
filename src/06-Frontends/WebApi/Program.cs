using GrainInterfaces.Serialization.ProtobufNet;
using Microsoft.AspNetCore;
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
using System.Threading.Tasks;

namespace WebApi
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var silo = CreateSilo();
            await silo.StartAsync();

            var webHost = CreateWebHost(silo);
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static ISiloHost CreateSilo()
        {
            // define the cluster configuration
            return new SiloHostBuilder()
                .UseLocalhostClustering()
                // .EnableDirectClient() // obsolete, enabled by default
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
                .Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false)
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

        private static IWebHost CreateWebHost(ISiloHost silo)
        {
            var client = silo.Services.GetRequiredService<IClusterClient>();

            var webHost = new WebHostBuilder()
                            .ConfigureServices(services =>
                                services
                                    .AddSingleton<IGrainFactory>(client)
                                    .AddSingleton<IClusterClient>(client))
                            .UseStartup<WebApi.Startup>()
                            // Other ASP.NET configuration...
                            .UseKestrel()
                            .ConfigureKestrel((context, options) => { })
                            .Build();
            return webHost;
        }
    }
}
