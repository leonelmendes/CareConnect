using System;
using System.Text.Json.Serialization;

namespace CareConnect.Shared.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirebaseUid { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }

    // Propriedades de Navegação (Relações)
    [JsonIgnore]
    public ICollection<TaskLog> TarefasExecutadas { get; set; } = new List<TaskLog>();
    
    [JsonIgnore]
    public ICollection<Patient> PacientesGeridos { get; set; } = new List<Patient>();
}