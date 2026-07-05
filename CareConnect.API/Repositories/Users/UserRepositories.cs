
using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Users;

public class UserRepositories : IUserRepositories
{
    private readonly AppDbContext _context;
    
    public UserRepositories(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
    }
}