using Stock_Server.Repository;
using Stock_Server.Models;
public interface IUserRepo :IRepository<User>
{
    Task UpdateRefreshTokenAsync(string userId, string? hashedToken, DateTime? expiry);
    Task UpdateLoginMetadataAsync(string userId, DateTime lastLogin, bool isVerified);
    Task UpdateLogoutMetaDataAsync(string Id,string? token,DateTime? expired);
    
}