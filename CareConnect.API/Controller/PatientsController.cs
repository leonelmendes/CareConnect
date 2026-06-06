using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.Patients;

namespace CareConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
    {
        var patients = await _patientRepository.GetAllAsync();
        return Ok(patients);
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> CreatePatient(PatientCreateDto dto)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            DataNascimento = dto.DataNascimento,
            CondicoesMedicas = dto.CondicoesMedicas,
            GestorId = dto.GestorId
        };
        
        var createdPatient = await _patientRepository.AddAsync(patient);
        return CreatedAtAction(nameof(GetPatients), new { id = createdPatient.Id }, createdPatient);
    }
}