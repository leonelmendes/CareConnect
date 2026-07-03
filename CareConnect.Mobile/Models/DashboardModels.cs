namespace CareConnect.Mobile.Models;

public class PacienteResumo
{
    public string Nome { get; set; } = string.Empty;
    public string Imagem { get; set; } = string.Empty;
    public bool EstaOnline { get; set; }
}

public class AlertaRecente
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tempo { get; set; } = string.Empty;
    public string CorIcone { get; set; } = string.Empty; 
    public string ImagemIcone { get; set; } = string.Empty;
}