using CareConnect.Shared.Models;

namespace CareConnect.Shared.DTOs;

public class UserCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}