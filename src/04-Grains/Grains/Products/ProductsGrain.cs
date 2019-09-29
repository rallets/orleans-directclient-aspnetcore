using GrainInterfaces.Products;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansSilo.Products
{
    [StorageProvider(ProviderName = "BlobStore")]
    [ImplicitStreamSubscription("ProductCreatedStream")]
    public class ProductsGrain : Grain<ProductsState>, IProducts
    {
        private readonly ILogger _logger;
        protected StreamSubscriptionHandle<Product> _sub;

        public ProductsGrain(ILogger<ProductsGrain> logger)
        {
            _logger = logger;
        }

        public async Task Add(Product product)
        {
            if (State.Products.Any(p => product.Id == p))
            {
                return;
            }
            State.Products.Add(product.Id);
            await base.WriteStateAsync();
        }

        public async Task<Product[]> GetAll()
        {
            var products = new List<Task<Product>>();
            foreach (var id in this.State.Products)
            {
                var product = GrainFactory.GetGrain<IProduct>(id);
                products.Add(product.GetState());
            }
            return await Task.WhenAll(products);
        }

        public Task<bool> Exists(Guid id)
        {
            var result = State.Products.Contains(id);
            _logger.Info($"Product exists {id} => {result}");
            return Task.FromResult(result);
        }
    }
}
