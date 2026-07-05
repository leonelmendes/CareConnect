using System;
using System.Collections.Generic;
using System.Text;

namespace CareConnect.Mobile.Models
{
    public class Utente
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string NomeCuidador { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string StatusCuidado { get; set; } = "Estável"; // "Estável" ou "Alerta"

        // Propriedades calculadas para a UI (Cores das badges)
        public Color CorFundoStatus => StatusCuidado == "Estável" ? Color.FromArgb("#D1FAE5") : Color.FromArgb("#FEF3C7");
        public Color CorTextoStatus => StatusCuidado == "Estável" ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
    }
}
