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
            .FirstOrDefaultAsync(t => t.Id == id && (t.ExecutorId == executorId || t.CarePlan!.Patient!.GestorId == executorId));

        if (taskLog == null) return null;

        // Atualiza o estado
        taskLog.Status = novoStatus;

        // REGISTA O TIMESTAMP EXATO DA EXECUÇÃO SE ESTIVER A CONCLUIR
        if (novoStatus == CareTaskStatus.Realizado || novoStatus == (CareTaskStatus)1) // Ajusta conforme o teu enum
        {
            taskLog.TimestampExecucao = DateTime.UtcNow;
        }

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
            // Fazemos Include tanto do CarePlan como do Utente direto (para os Ad-Hoc)
            .Include(t => t.CarePlan)
                .ThenInclude(c => c.Patient)
            .Where(t => t.ExecutorId == executorId &&
                        t.DataHoraAgendada >= inicioDoDia &&
                        t.DataHoraAgendada <= fimDoDia)
            .OrderBy(t => t.DataHoraAgendada)
            .Select(t => new TarefaResumoDto
            {
                Id = t.Id,
                DataHora = t.DataHoraAgendada,

                // Usamos os novos campos universais que criámos
                Titulo = t.Titulo,
                Categoria = t.Categoria,

                // Se tiver um CarePlan, vai buscar o nome lá. Se não (Ad-Hoc), usa o UtenteId direto (se tiveres a relação configurada)
                // Assumindo que Patient tem a propriedade "Nome"
                NomeUtente = t.CarePlan != null && t.CarePlan.Patient != null
                                ? t.CarePlan.Patient.Nome
                                : "Utente Desconhecido",

                Concluida = t.Status == CareTaskStatus.Realizado
            })
            .ToListAsync();
    }

    public async Task<bool> RegistarAdHocAsync(Guid cuidadorId, RegistoAdHocDto dto)
    {
        try
        {
            var novoRegisto = new TaskLog
            {
                ExecutorId = cuidadorId,
                UtenteId = dto.UtenteId,
                Titulo = dto.Titulo,
                Categoria = dto.Categoria, // Adicionado para suportar os ícones na UI
                Notas = dto.Notas,

                DataHoraAgendada = dto.DataHora, // Para aparecer corretamente na Timeline
                TimestampExecucao = dto.DataHora, // Como é Ad-Hoc, foi feita na hora

                Status = CareTaskStatus.Realizado,
                IsAdHoc = true,
                CarePlanId = null
            };

            await _context.TaskLogs.AddAsync(novoRegisto);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}