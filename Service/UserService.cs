
using Stock_Server.Mapper;
using Stock_Server.Repository;
using Stock_Server.Service;
using Stock_Server.Models;
using Stock_Server.Util.Exceptions;
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
 return new UserResponse { Id = user.Id, Username = user.Username, Role = user.Role, IsVerified = user.IsVerified };

            
       
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