using System;
using System.Text.Json.Serialization;

namespace CareConnect.Shared.Models;

public class CarePlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlanType Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TimeSpan HoraProgramada { get; set; }
    public string Frequencia { get; set; } = string.Empty;

    // Propriedades de Navegação
    [JsonIgnore]
    public Patient? Patient { get; set; }
    
    [JsonIgnore]
    public ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
}