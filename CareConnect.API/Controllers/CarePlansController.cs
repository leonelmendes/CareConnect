using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.CarePlans;
using Microsoft.AspNetCore.Authorization;
using CareConnect.API.Repositories.Users;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CarePlansController : ControllerBase
{
    private readonly ICarePlanRepositories _repository;
    private readonly IUserRepositories _userRepository;

    public CarePlansController(ICarePlanRepositories repository, IUserRepositories userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    // Método Auxiliar de Segurança
    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        if (string.IsNullOrEmpty(firebaseUid)) return null;
        return await _userRepository.GetByFirebaseUidAsync(firebaseUid);
    }

    // GET: api/careplans/patient/{patientId}
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetAllByPatientId(Guid patientId)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var planos = await _repository.GetAllByPatientIdAsync(patientId, currentUser.Id);
        return Ok(planos);
    }

    // GET: api/careplans/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var plano = await _repository.GetByIdAsync(id, currentUser.Id);
        if (plano == null) return NotFound("Plano não encontrado ou sem permissão.");

        return Ok(plano);
    }

    // POST: api/careplans
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CarePlan novoPlano)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        // Garante a geração de um novo ID no backend
        novoPlano.Id = Guid.NewGuid();

        var planoCriado = await _repository.CreateAsync(novoPlano, currentUser.Id);

        if (planoCriado == null) 
            return BadRequest("Paciente inválido, inativo ou não lhe pertence.");

        return CreatedAtAction(nameof(GetById), new { id = planoCriado.Id }, planoCriado);
    }

    // PUT: api/careplans/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CarePlan planoAtualizado)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var plano = await _repository.UpdateAsync(id, planoAtualizado, currentUser.Id);
        if (plano == null) return NotFound("Plano não encontrado.");

        return Ok(plano);
    }

    // DELETE: api/careplans/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var sucesso = await _repository.DeleteAsync(id, currentUser.Id);
        if (!sucesso) return NotFound("Plano não encontrado.");

        return NoContent();
    }
}