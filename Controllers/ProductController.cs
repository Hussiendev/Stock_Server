using Microsoft.AspNetCore.Mvc;
using Stock_Server.Mapper;
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JSONProduct>>> GetAll()
    {
        var products = await _service.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<JSONProduct>> GetByName(string name)
    {
        try
        {
            var product = await _service.GetProductByNameAsync(name);
            return Ok(product);
        }
        catch (ItemNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<JSONProduct>> Create([FromBody] JSONProduct request)
    {
        /*
         Example POST body:
         {
           "name": "Laptop",
           "description": "Gaming laptop",
           "price": 1299.99,
           "quantity": 5,
           "supplierName": "Acme Supplies"
         }
        */

        try
        {
            var created = await _service.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetByName), new { name = created.Name }, created);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DBException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

   [HttpPut("{name}")]
public async Task<IActionResult> Update(string name, [FromBody]UpdateJSONProduct request)
{
    try
    {
        await _service.UpdateProductAsync(name, request);
        return NoContent();
    }
    catch (ItemNotFoundException ex)
    {
        return NotFound(ex.Message);
    }
    catch (BadRequestException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (DBException ex)
    {
        return StatusCode(500, ex.Message);
    }
}
[HttpGet("{name}/capital")]
public async Task<IActionResult> GetCapital(string name)
{
    try
    {
        var capital = await _service.GetCapital(name);
        return Ok(new { productName = name, capital });
    }
    catch (ItemNotFoundException ex)
    {
        return NotFound(ex.Message);
    }
    catch (DBException ex)
    {
        return StatusCode(500, ex.Message);
    }
}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _service.DeleteProductAsync(id);
            return NoContent();
        }
        catch (ItemNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (DBException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}