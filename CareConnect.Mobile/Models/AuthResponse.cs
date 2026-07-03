namespace CareConnect.Mobile.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime DataExpiracao { get; set; }
    public string Perfil { get; set; } = string.Empty; 
    public bool Sucesso { get; set; }
    public string MensagemErro { get; set; } = string.Empty;
}