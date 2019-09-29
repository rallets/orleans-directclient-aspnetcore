using GrainInterfaces.Products;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace OrleansSilo.Products
{
    [StorageProvider(ProviderName = "TableStore")]
    public class ProductGrain : Grain<Product>, IProduct
    {
        private readonly ILogger _logger;

        public ProductGrain(ILogger<ProductGrain> logger)
        {
            _logger = logger;
        }

        public async Task<Product> Create(Product product)
        {
            product.Id = this.GetPrimaryKey();
            State = product;
            await base.WriteStateAsync();

            _logger.Info($"Product created => {product.Id}");

            await GrainFactory.GetGrain<IProducts>(Guid.Empty).Add(product);

            return State;
        }

        public Task<Product> GetState()
        {
            return Task.FromResult(State);
        }
    }
}
