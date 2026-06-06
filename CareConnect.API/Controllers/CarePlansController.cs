using Microsoft.AspNetCore.Mvc;
using CareConnect.Shared.Models;
using CareConnect.Shared.DTOs;
using CareConnect.API.Repositories.CarePlans;

namespace CareConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarePlansController : ControllerBase
{
    private readonly ICarePlanRepositories _repository;

    public CarePlansController(ICarePlanRepositories repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarePlan>>> GetCarePlans()
    {
        var plans = await _repository.GetAllAsync();
        return Ok(plans);
    }

    [HttpPost]
    public async Task<ActionResult<CarePlan>> CreateCarePlan(CarePlanCreateDto dto)
    {
        var carePlan = new CarePlan
        {
            Id = Guid.NewGuid(),
            PatientId = dto.PatientId,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            HoraProgramada = dto.HoraProgramada,
            Frequencia = dto.Frequencia
        };

        var createdPlan = await _repository.AddAsync(carePlan);
        return CreatedAtAction(nameof(GetCarePlans), new { id = createdPlan.Id }, createdPlan);
    }
}