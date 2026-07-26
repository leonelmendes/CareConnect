using CareConnect.API.Repositories.TaskLogs;
using CareConnect.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ITaskLogRepositories _taskLogRepositories;

    public DashboardController(ITaskLogRepositories taskLogRepo)
    {
        _taskLogRepositories = taskLogRepo;
    }

    [HttpGet("tarefas-hoje")]
    public async Task<ActionResult<IEnumerable<TarefaResumoDto>>> GetTarefasDeHoje()
    {
        // 1. Obter o ID do utilizador logado no token JWT
        // Usa o teu método "ObterUtilizadorAutenticadoAsync" que tens noutros controllers
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userIdString, out Guid executorId))
            return Unauthorized();

        // 2. Chama o nosso novo método no repositório já existente
        var tarefas = await _taskLogRepositories.ObterResumoTarefasDoDiaAsync(executorId, DateTime.UtcNow);

        return Ok(tarefas);
    }

    [HttpPost("ad-hoc")]
    public async Task<IActionResult> RegistarAdHoc([FromBody] RegistoAdHocDto dto)
    {
        if (dto == null) return BadRequest("Dados inválidos.");

        var cuidadorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");

        // Validar e converter para Guid em vez de int
        if (cuidadorIdClaim == null || !Guid.TryParse(cuidadorIdClaim.Value, out Guid cuidadorId))
        {
            return Unauthorized("Utilizador não autorizado ou ID não encontrado/inválido no token.");
        }

        var sucesso = await _taskLogRepositories.RegistarAdHocAsync(cuidadorId, dto);

        if (sucesso) return Ok();

        return StatusCode(500, "Ocorreu um erro ao gravar o registo.");
    }
}