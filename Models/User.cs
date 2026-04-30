namespace Stock_Server.Models;

using System;

using System.Runtime.Serialization;

public enum Roles
{
    [EnumMember(Value = "Employee")]
    Employee,
    [EnumMember(Value = "Admin")]
    Admin
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Roles Role { get; set; } = Roles.Employee;
    public bool IsVerified { get; set; } = false;           
    public DateTime LastLogin { get; set; } = DateTime.UtcNow; 
    public string? RefreshToken { get; set; } = string.Empty;   
    public DateTime? RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;
}