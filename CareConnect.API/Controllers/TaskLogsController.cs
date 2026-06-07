using CareConnect.API.Repositories.TaskLogs;
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

    public TaskLogsController(ITaskLogRepositories repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskLog>>> GetTaskLogs()
    {
        var taskLogs = await _repository.GetAllAsync();
        return Ok(taskLogs);
    }

    [HttpPost]
    public async Task<ActionResult<TaskLog>> CreateTaskLog(TaskLogCreateDto dto)
    {
        var taskLog = new TaskLog
        {
            Id = Guid.NewGuid(),
            CarePlanId = dto.CarePlanId,
            ExecutorId = dto.ExecutorId,
            TimestampExecucao = dto.TimestampExecucao,
            Status = dto.Status,
            Notas = dto.Notas,
            FotoUrl = dto.FotoUrl
        };
        
        var createdTaskLog = await _repository.AddAsync(taskLog);
        return CreatedAtAction(nameof(GetTaskLogs), new { id = createdTaskLog.Id }, createdTaskLog);
    }

}