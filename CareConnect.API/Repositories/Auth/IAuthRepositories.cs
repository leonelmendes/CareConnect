using CareConnect.Shared.DTOs;

namespace CareConnect.API.Repositories.Auth
{
    public interface IAuthRepositories
    {
        Task<(bool Sucesso, string Token, string Perfil, string MensagemErro)> LoginAsync(LoginDto dto);
        Task<(bool Sucesso, string Token, string Perfil, string MensagemErro)> SyncFirebaseAsync(string firebaseUid, string email, string nome);
    }
}