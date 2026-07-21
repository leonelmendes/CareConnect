using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;

namespace CareConnect.API.Repositories.TaskLogs;

public class TaskLogRepositories : ITaskLogRepositories
{
    private readonly AppDbContext _context;

    public TaskLogRepositories(AppDbContext appDbContext)
    {
        this._context = appDbContext;
    }

    public async Task<TaskLog> AddAsync(TaskLog taskLog)
    {
        _context.TaskLogs.Add(taskLog);
        await _context.SaveChangesAsync();
        return taskLog;
    }

    public async Task<IEnumerable<TaskLog>> GetAllByCarePlanIdAsync(Guid carePlanId, Guid gestorId)
    {
        return await _context.TaskLogs
            // Viagem dupla nas tabelas para garantir a segurança: TaskLog -> CarePlan -> Patient
            .Include(t => t.CarePlan)
            .ThenInclude(c => c.Patient)
            .Where(t => t.CarePlanId == carePlanId && t.CarePlan!.Patient!.GestorId == gestorId)
            .OrderByDescending(t => t.TimestampExecucao)
            .ToListAsync();
    }

    public async Task<TaskLog?> CreateAsync(TaskLog taskLog, Guid executorId)
    {
        // Valida se o plano de cuidados pertence a um paciente gerido por este utilizador
        var planoValido = await _context.CarePlans
            .Include(c => c.Patient)
            .AnyAsync(c => c.Id == taskLog.CarePlanId && c.Patient!.GestorId == executorId && c.Patient.Ativo);

        if (!planoValido) return null;

        taskLog.ExecutorId = executorId;
        taskLog.TimestampExecucao = DateTime.UtcNow;

        await _context.TaskLogs.AddAsync(taskLog);
        await _context.SaveChangesAsync();

        return taskLog;
    }

    public async Task<TaskLog?> UpdateStatusAsync(Guid id, CareTaskStatus novoStatus, string notas, Guid executorId)
    {
        var taskLog = await _context.TaskLogs
            .Include(t => t.CarePlan)
            .ThenInclude(c => c.Patient)
            .FirstOrDefaultAsync(t => t.Id == id && t.CarePlan!.Patient!.GestorId == executorId);

        if (taskLog == null) return null;

        // Atualiza o estado
        taskLog.Status = novoStatus;
        
        // Se o utilizador enviou uma nota justificativa, adicionamos ao histórico
        if (!string.IsNullOrWhiteSpace(notas))
        {
            var registoTempo = $"[{DateTime.UtcNow:dd/MM/yyyy HH:mm}]";
            taskLog.Notas = string.IsNullOrWhiteSpace(taskLog.Notas)
                ? $"{registoTempo} {notas}"
                : $"{taskLog.Notas}\n{registoTempo} {notas}";
        }

        await _context.SaveChangesAsync();
        return taskLog;
    }

    public async Task<IEnumerable<TarefaResumoDto>> ObterResumoTarefasDoDiaAsync(Guid executorId, DateTime data)
    {
        var inicioDoDia = data.Date;
        var fimDoDia = inicioDoDia.AddDays(1).AddTicks(-1);

        return await _context.TaskLogs
            .Include(t => t.CarePlan)
            .ThenInclude(c => c.Patient)
            .Where(t => t.ExecutorId == executorId && 
                        t.TimestampExecucao >= inicioDoDia && 
                        t.TimestampExecucao <= fimDoDia)
            .OrderBy(t => t.TimestampExecucao)
            .Select(t => new TarefaResumoDto
            {
                Id = t.Id,
                DataHora = t.TimestampExecucao,
                
                // CORREÇÃO: Usamos t.CarePlan.Descricao em vez de Titulo
                Titulo = t.CarePlan!.Descricao ?? "Tarefa sem descrição", 
                
                // Nota: Confirma se o teu modelo Patient usa a propriedade 'Nome'
                NomeUtente = t.CarePlan.Patient!.Nome ?? "Utente Desconhecido", 
                
                Concluida = t.Status == CareTaskStatus.Realizado 
            })
            .ToListAsync();
    }
}