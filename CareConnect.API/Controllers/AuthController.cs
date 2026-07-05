using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CareConnect.Shared.DTOs;
using System.Security.Claims;
using CareConnect.API.Repositories.Auth;
using CareConnect.API.Repositories.Users;

namespace CareConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepositories _authService;
    private readonly IUserRepositories _repository;

    public AuthController(IAuthRepositories authService, IUserRepositories repository)
    {
        _authService = authService;
        _repository = repository;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (!resultado.Sucesso)
        {
            return Unauthorized(new { sucesso = false, mensagemErro = resultado.MensagemErro });
        }

        return Ok(new
        {
            sucesso = true,
            token = resultado.Token,
            perfil = resultado.Perfil,
            dataExpiracao = DateTime.UtcNow.AddDays(7),
            mensagemErro = ""
        });
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            return Unauthorized(new { sucesso = false, mensagemErro = "Token inválido." });

        var user = await _repository.GetByIdAsync(userId); 
        
        if (user == null)
            return Unauthorized(new { sucesso = false, mensagemErro = "Utilizador não encontrado." });

        // Gera um novo token com mais 7 dias!
        var novoToken = _authService.GerarTokenJwt(user);

        return Ok(new
        {
            sucesso = true,
            token = novoToken,
            perfil = user.Role.ToString(),
            dataExpiracao = DateTime.UtcNow.AddDays(7),
            mensagemErro = ""
        });
    }

    /*[AllowAnonymous]
    [HttpPost("sync-login")]
    public async Task<IActionResult> SyncFirebaseUser([FromBody] LoginDto? dto)
    {
        // Tenta ler do Header Authorization (Claims do Token Firebase) OU do Body HTTP que o mobile enviar
        var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("user_id")?.Value
                       ?? dto?.FirebaseUid;
                    
        var email = User.FindFirst(ClaimTypes.Email)?.Value 
                 ?? User.FindFirst("email")?.Value
                 ?? dto?.Email;

        if (string.IsNullOrEmpty(firebaseUid))
            return BadRequest(new { sucesso = false, mensagemErro = "Token ou Firebase UID inválido." });

        var resultado = await _authService.SyncFirebaseAsync(firebaseUid, email ?? string.Empty, string.Empty);

        if (!resultado.Sucesso)
        {
            return BadRequest(new { sucesso = false, mensagemErro = resultado.MensagemErro });
        }

        return Ok(new
        {
            sucesso = true,
            token = resultado.Token,
            perfil = resultado.Perfil,
            dataExpiracao = DateTime.UtcNow.AddDays(7),
            mensagemErro = ""
        });
    }
    */
}