using System;

namespace CareConnect.Shared.Models;

public class TaskLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarePlanId { get; set; }
    public Guid ExecutorId { get; set; }
    public DateTime TimestampExecucao { get; set; }
    public TaskStatus Status { get; set; }
    public string Notas { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
}