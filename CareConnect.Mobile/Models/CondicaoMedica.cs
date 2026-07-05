namespace CareConnect.Mobile.Models;

public class CondicaoMedica
{
    public string Icone { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string DataDiagnostico { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Ex: Controlado, Estável

    public Color CorFundoStatus => Status == "Controlado" ? Color.FromArgb("#D1FAE5") : Color.FromArgb("#FEF3C7");
    public Color CorTextoStatus => Status == "Controlado" ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
}