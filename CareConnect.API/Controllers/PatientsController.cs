using CareConnect.API.Repositories.Patients;
using CareConnect.API.Repositories.Users;
using CareConnect.API.Services; // ⚠️ Adicionado para o S3Service
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepositories _patientRepository;
    private readonly IUserRepositories _userRepository;
    private readonly S3Service _s3Service; // ⚠️ Injeção do serviço S3

    public PatientsController(IPatientRepositories patientRepository, IUserRepositories userRepository, S3Service s3Service)
    {
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _s3Service = s3Service;
    }

    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");
        
        if (string.IsNullOrEmpty(userIdString)) 
            return null;

        // 2. Converte a string para um Guid do PostgreSQL
        if (!Guid.TryParse(userIdString, out Guid userId))
            return null;

        // 3. Procura o utilizador real na base de dados!
        return await _userRepository.GetByIdAsync(userId);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized("Utilizador não encontrado.");

        var pacientes = await _patientRepository.GetAllByGestorIdAsync(currentUser.Id);
        return Ok(pacientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var paciente = await _patientRepository.GetByIdAsync(id, currentUser.Id);
        if (paciente == null) return NotFound("Paciente não encontrado.");
        
        return Ok(paciente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Patient novoPaciente)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        novoPaciente.GestorId = currentUser.Id;
        novoPaciente.Id = Guid.NewGuid();
        novoPaciente.Ativo = true;
        novoPaciente.DataCriacao = DateTime.UtcNow;

        // O objeto "novoPaciente" já leva a lista "CuidadoresIds" preenchida que veio do telemóvel.
        // Mandamos diretamente para o repositório resolver.
        var pacienteCriado = await _patientRepository.CreateAsync(novoPaciente);

        return CreatedAtAction(nameof(GetById), new { id = pacienteCriado.Id }, pacienteCriado);
    }

    [HttpPost("{id:guid}/upload-avatar")]
    public async Task<IActionResult> UploadAvatar(Guid id, IFormFile foto)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var paciente = await _patientRepository.GetByIdAsync(id, currentUser.Id);
        if (paciente == null) return NotFound("Paciente não encontrado.");

        if (foto == null || foto.Length == 0)
            return BadRequest("Nenhuma imagem enviada.");

        // Manda para a pasta "utentes/" no bucket S3!
        var urlS3 = await _s3Service.UploadFotoAsync(foto, "utentes");

        paciente.AvatarUrl = urlS3;
        await _patientRepository.UpdateAsync(id, paciente, currentUser.Id);

        return Ok(new { sucesso = true, avatarUrl = urlS3 });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Patient pacienteAtualizado)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var paciente = await _patientRepository.UpdateAsync(id, pacienteAtualizado, currentUser.Id);
        if (paciente == null) return NotFound("Paciente não encontrado.");

        return Ok(paciente);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var sucesso = await _patientRepository.DeactivateAsync(id, currentUser.Id);
        if (!sucesso) return NotFound("Paciente não encontrado.");

        return NoContent();
    }

    [HttpGet("meus-pacientes")]
    [Authorize] // Garante que só quem tem login consegue aceder
    public async Task<IActionResult> GetMeusPacientes()
    {
        try
        {
            // Extrai o ID do Cuidador logado a partir do token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid cuidadorId))
            {
                return Unauthorized("ID do utilizador não encontrado no token.");
            }

            var pacientes = await _patientRepository.GetPacientesDoCuidadorAsync(cuidadorId);

            // ou devolver a entidade diretamente se não for muito pesada
            return Ok(pacientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao carregar os pacientes.");
        }
    }
}