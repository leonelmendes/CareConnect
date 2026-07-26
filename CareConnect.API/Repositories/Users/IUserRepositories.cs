using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Users;

public interface IUserRepositories
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User> AddAsync(User user);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid);
    Task<User?> GetByEmailAsync(string email);
    Task UpdateAsync(User usuaruserio);
    Task<IEnumerable<CuidadorResumo>> GetCuidadoresAtivosAsync();
}