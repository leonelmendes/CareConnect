using CareConnect.API.Repositories.Users;
using CareConnect.API.Services;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize] // Protegido! Só utilizadores logados na app podem mandar fotos
    public async Task<IActionResult> UploadAvatar(IFormFile foto)
    {
        try
        {
            // 1. VALIDAÇÃO DE SEGURANÇA
            if (foto == null || foto.Length == 0)
                return BadRequest(new { sucesso = false, mensagem = "Nenhum ficheiro foi enviado." });

            if (!foto.ContentType.StartsWith("image/"))
                return BadRequest(new { sucesso = false, mensagem = "O ficheiro enviado não é uma imagem válida." });

            // 2. IDENTIFICAÇÃO DO UTILIZADOR (Através do Token JWT)
            // Procura pelo ID no claim padrão "NameIdentifier" ou no "sub"
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                return Unauthorized(new { sucesso = false, mensagem = "Sessão inválida. Faça login novamente." });

            // 3. UPLOAD PARA A AWS S3
            // Guarda dentro de uma pasta chamada "avatares" no teu bucket
            var urlS3 = await _s3Service.UploadFotoAsync(foto, "avatares");

            // 4. ATUALIZAÇÃO NA BASE DE DADOS
            var usuario = await _repository.GetByIdAsync(userId);
            if (usuario == null)
                return NotFound(new { sucesso = false, mensagem = "Utilizador não encontrado na base de dados." });

            usuario.AvatarUrl = urlS3;
            await _repository.UpdateAsync(usuario); // Guarda as alterações no SQL

            // 5. RESPOSTA DE SUCESSO
            return Ok(new
            {
                sucesso = true,
                avatarUrl = urlS3,
                mensagem = "Foto de perfil guardada com sucesso!"
            });
        }
        catch (Exception ex)
        {
            // Se a AWS ou o banco falharem, captura o erro sem deixar a API ir abaixo
            return StatusCode(500, new { sucesso = false, mensagem = "Erro no servidor ao carregar imagem: " + ex.Message });
        }
    }
}