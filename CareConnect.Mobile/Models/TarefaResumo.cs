using System;

namespace CareConnect.Mobile.Models;

public class TarefaResumo
{
    public Guid Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string NomeUtente { get; set; } = string.Empty;
    public bool EstaConcluida { get; set; }

    // Novos campos necessários para a Timeline
    public string Categoria { get; set; } = string.Empty; // Ex: "Medicação", "Higiene"
    public string AvatarUtente { get; set; } = "avatar_elderly.png"; // Imagem padrão se falhar

    // 2. PROPRIEDADES UI (Lógica de Apresentação)

    // Formato 24h para encaixar no design da Timeline (Ex: 08:00, 16:30)
    public string HoraFormatada => DataHora.ToLocalTime().ToString("HH:mm");

    // Cor do círculo/borda: Verde se concluído, Cinzento suave se pendente (não distrai o olhar)
    public string CorStatus => EstaConcluida ? "#10B981" : "#D1D5DB";

    // Mantive os teus campos originais pois vão dar muito jeito no ecrã de Histórico!
    public string TextoStatus => EstaConcluida ? "Concluído" : "Pendente";
    public string FundoStatus => EstaConcluida ? "#D1FAE5" : "#F3F4F6";

    // Ícone automático baseado na Categoria (calcula-se sozinho, menos código na ViewModel!)
    public string IconeTipo
    {
        get
        {
            var cat = Categoria?.ToLower() ?? "";

            if (cat.Contains("medic")) return "icon_pill.png";
            if (cat.Contains("higien") || cat.Contains("banho")) return "icon_shower.png";
            if (cat.Contains("aliment") || cat.Contains("refei")) return "icon_food.png";
            if (cat.Contains("mobil") || cat.Contains("fisiot")) return "icon_walk.png";

            return "icon_heartbeat.png"; // Ícone padrão
        }
    }
}