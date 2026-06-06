namespace CareConnect.Shared.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string CondicoesMedicas { get; set; } = string.Empty;
    
    public Guid GestorId { get; set; }
}