using CareConnect.API.Data;
using CareConnect.API.Repositories.CarePlans;
using CareConnect.API.Repositories.Users;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CarePlansController : ControllerBase
{
    private readonly ICarePlanRepositories _repository;
    private readonly IUserRepositories _userRepository;
    private readonly AppDbContext _context;

    public CarePlansController(ICarePlanRepositories repository, IUserRepositories userRepository, AppDbContext context)
    {
        _repository = repository;
        _userRepository = userRepository;
        _context = context;
    }

    // Método Auxiliar de Segurança
    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
        
        if (string.IsNullOrEmpty(userIdString)) 
            return null;

        // Converte a string para o formato Guid do PostgreSQL
        if (!Guid.TryParse(userIdString, out Guid userId))
            return null;

        // Vai buscar o utilizador real à base de dados!
        return await _userRepository.GetByIdAsync(userId);
    }

    // GET: api/careplans/patient/{patientId}
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetAllByPatientId(Guid patientId)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var planos = await _repository.GetAllByPatientIdAsync(patientId, currentUser.Id);
        return Ok(planos);
    }

    // GET: api/careplans/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var plano = await _repository.GetByIdAsync(id, currentUser.Id);
        if (plano == null) return NotFound("Plano não encontrado ou sem permissão.");

        return Ok(plano);
    }

    // POST: api/careplans
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CarePlan novoPlano)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        novoPlano.Id = Guid.NewGuid();

        // Se o frontend não mandou um ExecutorId explícito, assumimos que é o utilizador logado (caso seja o próprio a criar)
        if (novoPlano.ExecutorId == Guid.Empty)
        {
            novoPlano.ExecutorId = currentUser.Id;
        }

        // 1. Salva o plano principal (passando o novoPlano que já tem o ExecutorId correto)
        var planoCriado = await _repository.CreateAsync(novoPlano, currentUser.Id);

        if (planoCriado == null)
            return BadRequest("Paciente inválido, inativo ou não lhe pertence.");

        // GERAÇÃO DE TAREFAS USANDO O EXECUTOR CORRETO DO PLANO
        var tarefasGeradas = new List<TaskLog>();
        var dataInicioUtc = DateTime.UtcNow;

        for (int i = 0; i <= 6; i++)
        {
            var dataDia = dataInicioUtc.AddDays(i);

            var dataFinalDaTarefaUtc = new DateTime(
                dataDia.Year,
                dataDia.Month,
                dataDia.Day,
                planoCriado.HoraProgramada.Hours,
                planoCriado.HoraProgramada.Minutes,
                planoCriado.HoraProgramada.Seconds,
                DateTimeKind.Utc
            );

            var novaTarefa = new TaskLog
            {
                CarePlanId = planoCriado.Id,
                ExecutorId = planoCriado.ExecutorId, // Atribui diretamente ao cuidador do plano!
                UtenteId = planoCriado.PatientId,
                Titulo = string.IsNullOrWhiteSpace(planoCriado.Descricao) ? planoCriado.Tipo.ToString() : planoCriado.Descricao,
                Categoria = planoCriado.Tipo.ToString(),
                DataHoraAgendada = dataFinalDaTarefaUtc,
                Status = 0,
                IsAdHoc = false,
                Notas = string.Empty,
                FotoUrl = string.Empty,
                TimestampExecucao = null
            };

            tarefasGeradas.Add(novaTarefa);
        }

        await _context.TaskLogs.AddRangeAsync(tarefasGeradas);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = planoCriado.Id }, planoCriado);
    }

    // PUT: api/careplans/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CarePlan planoAtualizado)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var plano = await _repository.UpdateAsync(id, planoAtualizado, currentUser.Id);
        if (plano == null) return NotFound("Plano não encontrado.");

        return Ok(plano);
    }

    // DELETE: api/careplans/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var sucesso = await _repository.DeleteAsync(id, currentUser.Id);
        if (!sucesso) return NotFound("Plano não encontrado.");

        return NoContent();
    }
}