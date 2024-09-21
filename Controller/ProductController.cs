using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using  Shop.Models;
using Microsoft.EntityFrameworkCore;

namespace Shop.Controller
{
    [ApiController]
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
        {
            var products = await context.Products.AsNoTracking().ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById([FromServices] DataContext context, int id)
        {
            var product = await context.Products
                .Include(x => x.Category)
                .AsNoTracking()
                .Where(x => x.CategoryId == id)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("categories/{id:int}")]
        public async Task<ActionResult<List<Product>>> GetByCategory([FromServices] DataContext context, int id)
        {
            if (!await context.Categories.AnyAsync(x => x.Id == id))
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }

            var products = await context.Products
                .Include(x => x.Category)
                .AsNoTracking()
                
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "Nenhum produto encontrado para esta categoria." });
            }

            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Post(
         [FromServices] DataContext context,
         [FromBody] Product model)
        {
            if (ModelState.IsValid)
            {
                if (!await context.Categories.AnyAsync(x => x.Id == model.CategoryId))
                {
                    return BadRequest(new { message = "A categoria não existe." });
                }

                context.Products.Add(model);
                await context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
            }

            return BadRequest(ModelState);
        }
    }
}
