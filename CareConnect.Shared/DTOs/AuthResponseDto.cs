// Ficheiro: CareConnect.Shared/DTOs/AuthResponseDto.cs
using System;

namespace CareConnect.Shared.DTOs;

public class AuthResponseDto
{
    public bool Sucesso { get; set; }
    public string MensagemErro { get; set; } = string.Empty;

    // --- DADOS DO TOKEN ---
    public string Token { get; set; } = string.Empty;
    public DateTime DataExpiracao { get; set; }
    
    // --- DADOS DO UTILIZADOR (Cruciais para a UI/Home) ---
    public Guid UserId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty; // "Gestor" ou "Cuidador"
    public string AvatarUrl { get; set; } = string.Empty;
}