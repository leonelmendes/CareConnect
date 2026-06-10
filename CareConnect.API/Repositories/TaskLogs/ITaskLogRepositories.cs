using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.TaskLogs;

public interface ITaskLogRepositories
{
    // Obtém todo o histórico de execuções associado a um plano de cuidados específico
    Task<IEnumerable<TaskLog>> GetAllByCarePlanIdAsync(Guid carePlanId, Guid gestorId);

    // Regista a execução de uma nova tarefa
    Task<TaskLog?> CreateAsync(TaskLog taskLog, Guid executorId);

    // Atualiza o estado de uma tarefa (ex: de Pendente para Realizado ou Falhado)
    Task<TaskLog?> UpdateStatusAsync(Guid id, CareTaskStatus novoStatus, string notas, Guid executorId);
}
