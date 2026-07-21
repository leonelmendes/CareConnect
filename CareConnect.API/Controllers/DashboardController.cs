using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.TaskLogs;

namespace CareConnect.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ITaskLogRepositories _taskLogRepo;

    public DashboardController(ITaskLogRepositories taskLogRepo)
    {
        _taskLogRepo = taskLogRepo;
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
        var tarefas = await _taskLogRepo.ObterResumoTarefasDoDiaAsync(executorId, DateTime.UtcNow);

        return Ok(tarefas);
    }
}