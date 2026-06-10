using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.Patients;
using Microsoft.AspNetCore.Authorization;
using CareConnect.API.Repositories.Users;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepositories _patientRepository;
    private readonly IUserRepositories _userRepository;

    public PatientsController(IPatientRepositories patientRepository, IUserRepositories userRepository)
    {
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    // 🛠️ Método Auxiliar: Descobre quem é o utilizador a partir do Token do Firebase
    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        // O Firebase guarda o ID único na claim NameIdentifier ou user_id
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        
        if (string.IsNullOrEmpty(firebaseUid)) return null;

        // Vai à base de dados buscar o nosso utilizador interno (com o Guid)
        return await _userRepository.GetByFirebaseUidAsync(firebaseUid);
    }

    // GET: api/patients
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized("Utilizador não encontrado na base de dados.");

        var pacientes = await _patientRepository.GetAllByGestorIdAsync(currentUser.Id);
        return Ok(pacientes);
    }

    // GET: api/patients/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var paciente = await _patientRepository.GetByIdAsync(id, currentUser.Id);
        
        if (paciente == null) return NotFound("Paciente não encontrado ou não tem permissões para o ver.");
        
        return Ok(paciente);
    }

    // POST: api/patients
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient novoPaciente)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        // 🛡️ Segurança: Ignoramos qualquer ID de gestor que venha no pedido e forçamos o do utilizador logado
        novoPaciente.GestorId = currentUser.Id;
        
        // Garantimos que o paciente nasce ativo e com ID novo
        novoPaciente.Id = Guid.NewGuid();
        novoPaciente.Ativo = true;
        novoPaciente.DataCriacao = DateTime.UtcNow;

        var pacienteCriado = await _patientRepository.CreateAsync(novoPaciente);

        return CreatedAtAction(nameof(GetById), new { id = pacienteCriado.Id }, pacienteCriado);
    }

    // PUT: api/patients/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Patient pacienteAtualizado)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var paciente = await _patientRepository.UpdateAsync(id, pacienteAtualizado, currentUser.Id);

        if (paciente == null) return NotFound("Paciente não encontrado.");

        return Ok(paciente);
    }

    // DELETE: api/patients/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var sucesso = await _patientRepository.DeactivateAsync(id, currentUser.Id);

        if (!sucesso) return NotFound("Paciente não encontrado.");

        return NoContent(); // 204: Apagado com sucesso e não há nada para devolver
    }
}