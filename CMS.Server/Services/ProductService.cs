using CMS.Server.DB;
using CMS.Server.Interfaces;
using CMS.Server.Models;
using CMS.Server.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CMS.Server.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;

        public ProductService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _db.Products
                .Include(p => p.Owner)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return products.Select(MapToResponse).ToList();
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            var product = await _db.Products
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
            return product is null ? null : MapToResponse(product);
        }

        public async Task<ProductResponse> CreateProductAsync(ProductRequest request, int ownerId)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                PictureUrl = request.PictureUrl,
                OwnerId = ownerId
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                PictureUrl = product.PictureUrl,
                OwnerId = product.OwnerId
            };
        }

        public async Task<ProductResponse?> UpdateProductAsync(int id, ProductRequest request, RequestContext requester)
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return null;

            if (!CanModify(product, requester))
                throw new UnauthorizedAccessException();

            product.Name = request.Name;
            product.Price = request.Price;
            product.Description = request.Description;
            product.PictureUrl = request.PictureUrl;

            await _db.SaveChangesAsync();
            return MapToResponse(product);
        }

        public async Task<bool> DeleteProductAsync(int id, RequestContext requester)
        {
            var product = await _db.Products.FindAsync(id);
            if (product is null) return false;

            if (!CanModify(product, requester))
                throw new UnauthorizedAccessException();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return true;
        }

        private static bool CanModify(Product product, RequestContext requester)
        {
            return requester.IsAdmin || requester.UserId == product.OwnerId;
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                PictureUrl = product.PictureUrl,
                OwnerId = product.OwnerId,
                OwnerName = product.Owner?.DisplayName ?? string.Empty
            };
        }
    }
}
