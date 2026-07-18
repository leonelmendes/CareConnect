public class PlanoCuidadoUI
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Horarios { get; set; } = string.Empty;
    public string UtenteNome { get; set; } = string.Empty;
    public string UtenteFoto { get; set; } = "avatar_elderly.png";
    public string Icone { get; set; } = "💊";
    public Color CorFundoIcone { get; set; } = Color.FromArgb("#EFF6FF");
}