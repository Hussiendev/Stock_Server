
using Stock_Server.Mapper;
using Stock_Server.Repository;
using Stock_Server.Service;
using Stock_Server.Models;
using Stock_Server.Util.Exceptions;
using System.Collections.Generic;
public class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly JSONUserMapper _mapper;
    private readonly ILogger<UserService> logger;

public UserService(IRepository<User> repository, JSONUserMapper mapper, ILogger<UserService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        this.logger = logger;
    }

    public async Task<UserResponse> CreateUser(RegisterUserDto request)
    {
         await ValidateUser(request);
            var user=_mapper.Map(request);
             
            await _repository.CreateAsync(user);
            logger.LogInformation("User with username {Username} created successfully", request.Username);
 return new UserResponse { Id = user.Id, Username = user.Username, Role = user.Role.ToString(), IsVerified = user.IsVerified };

            
       
    }
    public async Task  DeleteUser(string id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<UserSummaryResponse> GetUserBYName(string name)
    { var target_user=await _repository.GetByNameAsync(name);
        if (target_user == null)
        {
            throw new  ItemNotFoundException("user doenot exists");
        }
        return new UserSummaryResponse
        {
             Id =target_user.Id,
         Username = target_user.Username,
             Role=target_user.Role.ToString(),
                IsVerified=target_user.IsVerified
        };
        
    }
    
    public async    Task Update_User(string name,UserUpdateRequest req)
    {
        var user= await _repository.GetByNameAsync(name);
        if (user == null)
        {

            logger.LogError("Usre doest not exist");
            throw new ItemNotFoundException("User not found");
        }
        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            user.Username=req.Username;
        }
        if (req.Role != null)
        {
            user.Role= (Roles)req.Role;
        }
        if (!string.IsNullOrWhiteSpace(req.new_Pass))
        {
            user.PasswordHash=req.new_Pass;
            BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }
        await _repository.UpdateByNameAsync(name,user);


    }
    
    public  async Task<IEnumerable<UserSummaryResponse>> GetAll()
    {
        var users=await _repository.GetAllAsync();
        return users.Select(u=> new UserSummaryResponse
        {
            Id=u.Id,
            Username=u.Username,
            Role=u.Role.ToString(),
            IsVerified=u.IsVerified
        });
        
    }
    

    private async   Task ValidateUser(RegisterUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            logger.LogWarning("Validation failed: Username is required");
            throw new BadRequestException("Username is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogWarning("Validation failed: Password is required");
            throw new BadRequestException("Password is required");
        }

        var existingUser = await _repository.GetByNameAsync(request.Username);
        if (existingUser != null )
        {
            logger.LogWarning("Validation failed: Username {Username} already exists", request.Username);
            throw new BadRequestException($"Username {request.Username} already exists");
        }
             }
}