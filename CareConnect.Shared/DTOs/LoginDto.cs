namespace CareConnect.Shared.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirebaseUid { get; set; } // Opcional, usado quando o login é pelo Firebase
}