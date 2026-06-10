using System.Security.Claims;
using CareConnect.API.Repositories.TaskLogs;
using CareConnect.API.Repositories.Users;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskLogsController : ControllerBase
{
    private readonly ITaskLogRepositories _repository;
    private readonly IUserRepositories _userRepository;

    public TaskLogsController(ITaskLogRepositories repository, IUserRepositories userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        if (string.IsNullOrEmpty(firebaseUid)) return null;
        return await _userRepository.GetByFirebaseUidAsync(firebaseUid);
    }

    // GET: api/tasklogs/careplan/{carePlanId}
    [HttpGet("careplan/{carePlanId:guid}")]
    public async Task<IActionResult> GetAllByCarePlanId(Guid carePlanId)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var logs = await _repository.GetAllByCarePlanIdAsync(carePlanId, currentUser.Id);
        return Ok(logs);
    }

    // POST: api/tasklogs
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskLog novoLog)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        novoLog.Id = Guid.NewGuid();

        var logCriado = await _repository.CreateAsync(novoLog, currentUser.Id);

        if (logCriado == null) return BadRequest("Plano de cuidados inválido ou sem permissão.");

        return Ok(logCriado); 
    }

    // DTO auxiliar para receber apenas o Status e as Notas no método de atualização
    public class UpdateStatusDto
    {
        public CareTaskStatus Status { get; set; }
        public string Notas { get; set; } = string.Empty;
    }

    // PATCH: api/tasklogs/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var logAtualizado = await _repository.UpdateStatusAsync(id, dto.Status, dto.Notas, currentUser.Id);

        if (logAtualizado == null) return NotFound("Registo de tarefa não encontrado ou sem permissão.");

        return Ok(logAtualizado);
    }

}