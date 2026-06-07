using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.Users;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepositories _repository;

    public UsersController(IUserRepositories repository)
    {
        _repository = repository;
    }

    [Authorize]
    [HttpPost("sync-login")]
    public async Task<IActionResult> SyncFirebaseUser()
    {
        // 1. A API extrai os dados diretamente do Token do Firebase (passaporte)
        // O Firebase envia o ID único do utilizador na claim "NameIdentifier" ou "user_id"
        var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("user_id")?.Value;
                    
        var email = User.FindFirst(ClaimTypes.Email)?.Value 
                ?? User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(firebaseUid))
            return BadRequest("Token inválido: Firebase UID não encontrado.");

        // 2. Procuramos na nossa base de dados PostgreSQL se este utente/gestor já existe
        // (Terás de garantir que o teu IUserRepository tem um método como GetByFirebaseUidAsync)
        var existingUser = await _repository.GetByFirebaseUidAsync(firebaseUid);

        if (existingUser != null)
        {
            // O utilizador já está na nossa base de dados, devolvemos os dados dele
            return Ok(existingUser);
        }

        // 3. É a primeira vez que este utilizador faz login! Vamos criá-lo no PostgreSQL.
        var newUser = new User
        {
            FirebaseUid = firebaseUid, // Guarda a ligação ao Firebase
            Email = email,
            Nome = "Novo Utilizador", // Pode ser atualizado depois no ecrã de Perfil
            Role = UserRole.Gestor, 
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _repository.AddAsync(newUser);

        return Ok(createdUser);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _repository.GetAllAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(UserCreateDto dto)
    {
        // A API assume o controlo da geração do ID
        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            PasswordHash = dto.PasswordHash, 
            Role = dto.Role
        };

        var createdUser = await _repository.AddAsync(user);
        
        return CreatedAtAction(nameof(GetUsers), new { id = createdUser.Id }, createdUser);
    }
}