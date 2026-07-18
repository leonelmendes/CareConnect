using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Auth
{
    public interface IAuthRepositories
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<(bool Sucesso, string Token, string Perfil, string MensagemErro)> SyncFirebaseAsync(string firebaseUid, string email, string nome);
        //Task<User?> GetByIdAsync(Guid id);
        string GerarTokenJwt(User user);
    }
}