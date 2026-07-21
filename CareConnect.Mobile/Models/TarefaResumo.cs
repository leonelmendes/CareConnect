namespace CareConnect.Mobile.Models;

public class TarefaResumo
{
    public Guid Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Titulo { get; set; }
    public string NomeUtente { get; set; }
    public bool EstaConcluida { get; set; }
    
    // Propriedades Exclusivas para a UI do MAUI (Lógica de Apresentação)
    public string Hora => DataHora.ToLocalTime().ToString("hh:mm");
    public string Periodo => DataHora.ToLocalTime().ToString("tt").ToUpper(); 
    public string CorStatus => EstaConcluida ? "#10B981" : "#F59E0B"; 
    public string TextoStatus => EstaConcluida ? "Concluído" : "Pendente";
    public string FundoStatus => EstaConcluida ? "#D1FAE5" : "#FEF3C7"; 
}