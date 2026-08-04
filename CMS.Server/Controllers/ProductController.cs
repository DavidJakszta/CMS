using System.Security.Claims;
using CMS.Server.Interfaces;
using CMS.Server.Models;
using CMS.Server.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
                return NotFound();
            return Ok(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            var requester = GetCurrentRequester();
            if (requester.UserId is null)
                return Unauthorized();

            var product = await _productService.CreateProductAsync(request, requester.UserId.Value);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, request, GetCurrentRequester());
                if (product is null)
                    return NotFound();
                return Ok(product);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id, GetCurrentRequester());
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private RequestContext GetCurrentRequester()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.TryParse(userIdClaim, out var parsed) ? parsed : (int?)null;
            return new RequestContext(userId, User.IsInRole("Admin"));
        }
    }
}
