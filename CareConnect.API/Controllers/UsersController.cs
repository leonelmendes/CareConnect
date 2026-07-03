using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.Users;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _repository.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        // 1. Correção: Só bloqueia se o e-mail JÁ EXISTIR (diferente de null)
        var emailExiste = await _repository.GetByEmailAsync(dto.Email);
        if (emailExiste != null)
        {
            return BadRequest(new { sucesso = false, mensagemErro = "Este e-mail já está registado na nossa base de dados." });
        }

        var passwordHasheada = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            PasswordHash = passwordHasheada, 
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow,
            FirebaseUid = string.Empty
        };

        var createdUser = await _repository.AddAsync(user);
        
        return Ok(new
        {
            sucesso = true,
            perfil = createdUser.Role.ToString(),
            token = createdUser.Id.ToString(), //substituir o token pelo ID do utilizador
            mensagemErro = ""
        });
    }
}