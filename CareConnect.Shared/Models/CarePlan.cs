using System;

namespace CareConnect.Shared.Models;

public class CarePlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public PlanType Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TimeSpan HoraProgramada { get; set; }
    public string Frequencia { get; set; } = string.Empty;
}