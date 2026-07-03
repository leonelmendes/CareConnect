using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CareConnect.Shared.DTOs;
using System.Security.Claims;
using CareConnect.API.Repositories.Auth;

namespace CareConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepositories _authService;

    public AuthController(IAuthRepositories authService)
    {
        _authService = authService;
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