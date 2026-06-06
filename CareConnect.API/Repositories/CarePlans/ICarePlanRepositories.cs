using CareConnect.Shared.Models;
namespace CareConnect.API.Repositories.CarePlans;

public interface ICarePlanRepositories
{
    Task<IEnumerable<CarePlan>> GetAllAsync();
    Task<CarePlan?> GetByIdAsync(Guid id);
    Task<CarePlan> AddAsync(CarePlan carePlan);
}