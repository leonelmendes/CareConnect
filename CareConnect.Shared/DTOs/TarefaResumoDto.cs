namespace CareConnect.Shared.DTOs
{
    public class TarefaResumoDto
    {
        public Guid Id { get; set; }
        public DateTime DataHora { get; set; } // A API manda a data/hora real, o telemóvel formata
        public string Titulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string NomeUtente { get; set; } = string.Empty;
        public string AvatarUtente { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime? TimestampExecucao { get; set; }
        public bool IsAdHoc { get; set; }
        public bool Concluida { get; set; }

        // Propriedade formatada para exibir no histórico (ex: "Concluído às 14:30")
        public string TextoDetalheHistorico => Concluida
            ? $"Concluído às {(TimestampExecucao.HasValue ? TimestampExecucao.Value.ToString("HH:mm") : DataHora.ToString("HH:mm"))}"
            : "Não realizado / Pendente";
    }
}