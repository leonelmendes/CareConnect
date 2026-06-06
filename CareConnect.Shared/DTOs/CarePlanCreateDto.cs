using System;
using CareConnect.Shared.Models;

namespace CareConnect.Shared.DTOs;

public class CarePlanCreateDto
{
    public Guid PatientId { get; set; }
    public PlanType Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TimeSpan HoraProgramada { get; set; }
    public string Frequencia { get; set; } = string.Empty;
}