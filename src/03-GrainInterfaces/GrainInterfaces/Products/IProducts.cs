using System;
using System.Threading.Tasks;

namespace GrainInterfaces.Products
{
    public interface IProducts : Orleans.IGrainWithGuidKey
    {
        Task Add(Product product);
        Task<Product[]> GetAll();
        Task<bool> Exists(Guid id);
    }
}
