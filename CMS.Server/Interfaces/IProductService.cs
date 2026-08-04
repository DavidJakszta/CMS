using CMS.Server.Models;
using CMS.Server.Models.DTOs;

namespace CMS.Server.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task<ProductResponse> CreateProductAsync(ProductRequest request, int ownerId);
        Task<ProductResponse?> UpdateProductAsync(int id, ProductRequest request, RequestContext requester);
        Task<bool> DeleteProductAsync(int id, RequestContext requester);
        Task<int> GetProductCountAsync(int userId);
        Task<Dictionary<int, int>> GetProductCountsAsync();
    }
}
