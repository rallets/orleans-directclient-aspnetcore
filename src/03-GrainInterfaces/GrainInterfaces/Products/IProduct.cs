using System.Threading.Tasks;

namespace GrainInterfaces.Products
{
    public interface IProduct : Orleans.IGrainWithGuidKey
    {
        Task<Product> Create(Product product);
        Task<Product> GetState();
    }
}
