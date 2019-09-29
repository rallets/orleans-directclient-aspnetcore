using System;

namespace WebApi.Services
{
    public interface IProductIdentityService
    {
        Guid GetNewIdentity();
    }

    public class ProductIdentityService : IProductIdentityService
    {
        public Guid GetNewIdentity()
        {
            return Guid.NewGuid();
        }
    }
}
