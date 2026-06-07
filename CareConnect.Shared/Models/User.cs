using System;

namespace CareConnect.Shared.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirebaseUid { get; set; } = string.Empty; 
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
}