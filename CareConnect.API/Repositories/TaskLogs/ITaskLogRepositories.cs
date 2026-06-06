using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.TaskLogs;

public interface ITaskLogRepositories
{
    Task<IEnumerable<TaskLog>> GetAllAsync();
    Task<TaskLog?> GetByIdAsync(Guid id);
    Task<TaskLog> AddAsync(TaskLog taskLog);
}
