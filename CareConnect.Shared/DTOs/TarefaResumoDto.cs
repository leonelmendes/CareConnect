namespace CareConnect.Shared.DTOs
{
    public class TarefaResumoDto
    {
        public Guid Id { get; set; }
        public DateTime DataHora { get; set; } // A API manda a data/hora real, o telemóvel formata
        public string Titulo { get; set; }
        public string NomeUtente { get; set; }
        public bool Concluida { get; set; }
    }
}