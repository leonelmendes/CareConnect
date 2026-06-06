using System;
using CareConnect.Shared.Models;

namespace CareConnect.Shared.DTOs;

public class TaskLogCreateDto
{
    public Guid CarePlanId { get; set; }
    public Guid ExecutorId { get; set; }
    public DateTime TimestampExecucao { get; set; }
    public CareTaskStatus Status { get; set; }
    public string Notas { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
}