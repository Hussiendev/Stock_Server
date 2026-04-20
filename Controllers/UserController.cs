using Microsoft.AspNetCore.Mvc;
using Stock_Server.Mapper;
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service)
    {
        _service = service;
    }
    [HttpPost]
public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto request)
{
    try
    {
        var user = await _service.CreateUser(request);
        return Ok(user);
    }
    catch (BadRequestException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (DBException ex)
    {
        // Log if needed (service already logs)
        return StatusCode(500,ex.Message);
    }
    catch (Exception ex)
    {
        // Unexpected errors – log and return generic 500
        return StatusCode(500, ex.Message);
    }
}
    
}