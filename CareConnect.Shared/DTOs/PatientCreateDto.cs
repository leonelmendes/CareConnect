using System;

namespace CareConnect.Shared.DTOs;

public class PatientCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string CondicoesMedicas { get; set; } = string.Empty;
    public Guid GestorId { get; set; }
}