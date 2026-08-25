using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CareConnect.Shared.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Nome { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Contacto { get; set; }
    public string? ContactoEmergencia { get; set; }
    public string CondicoesMedicas { get; set; } = string.Empty;
    public string? Alergias { get; set; }
    public string? Notas { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Chave Estrangeira (Guarda apenas o ID do Gestor)
    public Guid GestorId { get; set; }

    // Chave estrangeira  Utente
    //public Guid? CuidadorId { get; set; }

    // Propriedades de Navegação (Obrigatórias para o AppDbContext funcionar)
    [JsonIgnore]
    public User? Gestor { get; set; }
    
    [JsonIgnore]
    public ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();

    //[JsonIgnore]
    public ICollection<User> Cuidadores { get; set; } = new List<User>();

    [NotMapped] // Usa o NotMapped do Entity Framework para não tentar criar esta coluna na base de dados
    public List<Guid>? CuidadoresIds { get; set; } = new List<Guid>();
}