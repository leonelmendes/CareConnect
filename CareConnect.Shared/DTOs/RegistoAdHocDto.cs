using System;
using System.Collections.Generic;
using System.Text;

namespace CareConnect.Shared.DTOs
{
    public class RegistoAdHocDto
    {
        public Guid UtenteId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
    }
}
