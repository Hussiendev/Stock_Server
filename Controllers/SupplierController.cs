using Microsoft.AspNetCore.Mvc;
using Stock_Server.Mapper;
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly SupplierService _service;

    public SupplierController(SupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JSONSupplier>>> GetAll()
    {
        var suppliers = await _service.GetAllSuppliersAsync();
        return Ok(suppliers);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<JSONSupplier>> GetByName(string name)
    {
        try
        {
            var supplier = await _service.GetSupplierByNameAsync(name);
            return Ok(supplier);
        }
        catch (ItemNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<JSONSupplier>> Create([FromBody] JSONSupplier request)
    {
        /*
         Example POST body:
         {
           "name": "Acme Supplies"
         }
        */

        try
        {
            var created = await _service.CreateSupplierAsync(request);
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
    public async Task<IActionResult> Update(string name, [FromBody] JSONSupplier request)
    {
        try
        {
            await _service.UpdateSupplierAsync(name, request);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _service.DeleteSupplierAsync(id);
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