using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Server.Mapper;
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;

namespace Stock_Server.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles ="Admin")] 
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


[HttpDelete("{id}")]

public async Task<IActionResult> DeleteUser(string id)
{
    try
    {
        await _service.DeleteUser(id);
        return NoContent();
    }
    catch (ItemNotFoundException ex)
    {
        return StatusCode(404,ex.Message);
    }
    catch (DBException ex)
    {
        // Log if needed (service already logs)
        return StatusCode(500, ex.Message);
    }
    catch (Exception ex)
    {
        // Unexpected errors – log and return generic 500
        return StatusCode(500, ex.Message);
    }    
}

[HttpGet("{name}")]
public async Task <IActionResult> GetUser(string Name)
    {
        try
        {
            var user= await _service.GetUserBYName(Name);
            return Ok(user);
            
        }
        catch(ItemNotFoundException ex){
            return StatusCode(404,ex.Message);
        }
        catch(DBException ex){
            return StatusCode(500,ex.Message);
        }
        catch(Exception e)
        {
            return StatusCode(500,e.Message);
        }
    }
[HttpPut("{name}")]
public async Task<IActionResult> Update(string Name,UserUpdateRequest req)
    {
        try
        {
            await _service.Update_User(Name,req);
            return NoContent();
        }
        catch(ItemNotFoundException ex){
            return StatusCode(404,ex.Message);
        }
        catch(DBException ex){
            return StatusCode(500,ex.Message);
        }
    }
  [HttpGet]   
public async Task<IActionResult> GetAll()
    {
        try
        {
            var users= await _service.GetAll();
            return Ok(users);
        }
        catch(DBException ex){
            return StatusCode(500,ex.Message);
        }
    }
}