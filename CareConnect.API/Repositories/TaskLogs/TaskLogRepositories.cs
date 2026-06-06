using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

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

    public async Task<IEnumerable<TaskLog>> GetAllAsync()
    {
        return await _context.TaskLogs.ToListAsync();
    }

    public Task<TaskLog?> GetByIdAsync(Guid id)
    {
        return _context.TaskLogs.FindAsync(id).AsTask();
    }
}