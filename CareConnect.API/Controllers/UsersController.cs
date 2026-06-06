using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.Users;

namespace CareConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepositories _repository;

    public UsersController(IUserRepositories repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _repository.GetAllAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(UserCreateDto dto)
    {
        // A API assume o controlo da geração do ID
        var user = new User
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            Email = dto.Email,
            PasswordHash = dto.PasswordHash, 
            Role = dto.Role
        };

        var createdUser = await _repository.AddAsync(user);
        
        return CreatedAtAction(nameof(GetUsers), new { id = createdUser.Id }, createdUser);
    }
}