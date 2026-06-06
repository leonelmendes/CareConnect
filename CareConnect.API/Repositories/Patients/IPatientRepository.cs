using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Patients;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient> AddAsync(Patient patient);
}