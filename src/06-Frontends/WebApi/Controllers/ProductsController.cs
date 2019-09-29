﻿using GrainInterfaces.Products;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models.Products;
using WebApi.Services;
using static WebApi.Controllers.ProductsMapper;

namespace WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IClusterClient _orleansClient;
        private readonly IProductIdentityService _productIdentityService;

        public ProductsController(
            IClusterClient orleansClient,
            IProductIdentityService productIdentityService
            )
        {
            _orleansClient = orleansClient;
            _productIdentityService = productIdentityService;
        }

        [HttpGet]
        public async Task<ActionResult<ProductsViewModel>> GetAllAsync()
        {
            var products = _orleansClient.GetGrain<IProducts>(Guid.Empty);
            var result = await products.GetAll();
            var response = MapToViewModel(result);
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<ProductViewModel>> PostAsync(ProductCreateRequest request)
        {
            var item = MapFromRequest(request);
            var identity = _productIdentityService.GetNewIdentity();
            var product = _orleansClient.GetGrain<IProduct>(identity);
            var result = await product.Create(item);
            var response = MapToViewModel(result);
            return response;
        }

    }

    public static class ProductsMapper
    {
        public static Product MapFromRequest(ProductCreateRequest request)
        {
            return new Product
            {
                CreationDate = DateTimeOffset.Now,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
            };
        }

        public static ProductViewModel MapToViewModel(Product item)
        {
            return new ProductViewModel(item);
        }

        public static ProductsViewModel MapToViewModel(IEnumerable<Product> items)
        {
            var result = new ProductsViewModel
            {
                Products = items.Select(x => new ProductViewModel(x)).ToList()
            };
            return result;
        }
    }
}
