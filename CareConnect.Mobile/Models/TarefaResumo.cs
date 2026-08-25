using System;

namespace CareConnect.Mobile.Models;

public class TarefaResumo
{
    // 1. DADOS DA BASE DE DADOS / API
    public Guid Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string NomeUtente { get; set; } = string.Empty;
    public bool EstaConcluida { get; set; }
    public string Notas { get; set; } = string.Empty;
    public DateTime? TimestampExecucao { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string AvatarUtente { get; set; } = "avatar_1.png";

    // NOVOS CAMPOS PARA O HISTÓRICO E AD-HOC
    public bool IsAdHoc { get; set; }
    public string FotoUrl { get; set; } = string.Empty;


    // 2. PROPRIEDADES UI (Lógica de Apresentação)

    // Formato 24h para encaixar no design da Timeline (Ex: 08:00, 16:30)
    public string HoraFormatada => DataHora.ToLocalTime().ToString("HH:mm");

    // Hora exata da conclusão (para aparecer ao lado do "✅ Feito às:")
    public string HoraConclusaoFormatada => TimestampExecucao.HasValue ? TimestampExecucao.Value.ToLocalTime().ToString("HH:mm") : "";

    // Esconde a label "🕒 Previsto:" se for uma tarefa Ad-Hoc
    public bool MostrarPrevisto => !IsAdHoc;

    // Lógica para decidir o Texto do Estado
    public string TextoStatus
    {
        get
        {
            if (IsAdHoc) return "Submetido";
            return EstaConcluida ? "Concluído com Sucesso" : "Não realizado / Pendente";
        }
    }

    // Cor do círculo/borda (Roxo para Ad-Hoc, Verde para Concluído, Vermelho/Cinzento para Pendente)
    public string CorStatus
    {
        get
        {
            if (IsAdHoc) return "#8B5CF6"; // Roxo elegante
            return EstaConcluida ? "#10B981" : "#EF4444"; // Verde ou Vermelho
        }
    }

    // Cor do fundo do Badge (se usares fundo no texto)
    public string FundoStatus
    {
        get
        {
            if (IsAdHoc) return "#EDE9FE"; // Fundo roxo claro
            return EstaConcluida ? "#D1FAE5" : "#FEE2E2"; // Fundo verde claro ou vermelho claro
        }
    }

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