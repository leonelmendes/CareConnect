using CareConnect.API.Repositories.Users;
using CareConnect.API.Services;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepositories _repository;
    private readonly S3Service _s3Service;

    public UsersController(IUserRepositories repository, S3Service s3Service)
    {
        _repository = repository;
        _s3Service = s3Service;
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

    [HttpGet("cuidadores")]
    public async Task<IActionResult> GetCuidadoresAtivos()
    {
        try
        {
            var cuidadores = await _repository.GetCuidadoresAtivosAsync();

            // Se a lista vier vazia, podes optar por devolver NotFound() ou apenas uma lista vazia com Ok(). 
            // Geralmente devolver Ok() com lista vazia é o ideal para arrays.
            return Ok(cuidadores);
        }
        catch (Exception ex)
        {
            // Aqui podes registar o erro (logger)
            return StatusCode(500, "Ocorreu um erro ao obter a lista de cuidadores.");
        }
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

    [HttpPost("upload-avatar")]
    [Authorize]
    // Adicionamos [FromQuery] string pasta com um valor padrão "geral" caso venha vazio:
    public async Task<IActionResult> UploadAvatar(IFormFile foto, [FromQuery] string pasta = "geral")
    {
        try
        {
            if (foto == null || foto.Length == 0)
                return BadRequest(new { sucesso = false, mensagem = "Nenhum ficheiro enviado." });

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { sucesso = false, mensagem = "Sessão inválida." });

            // AQUI ESTÁ A MÁGICA: Passamos a pasta dinâmica que veio do mobile!
            // O S3 vai criar a pasta automaticamente no bucket: careconnect-fotos/gestores/foto.jpg
            var urlS3 = await _s3Service.UploadFotoAsync(foto, pasta.ToLower());

            var usuario = await _repository.GetByIdAsync(userId);
            if (usuario != null)
            {
                usuario.AvatarUrl = urlS3;
                await _repository.UpdateAsync(usuario); // Ou SaveChangesAsync()
            }

            return Ok(new { sucesso = true, avatarUrl = urlS3 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
        }
    }
}