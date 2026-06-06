using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.CarePlans;

public class CarePlanRepositories : ICarePlanRepositories
{
    private readonly AppDbContext _context;
    public CarePlanRepositories(AppDbContext context) { _context = context; }

    public async Task<IEnumerable<CarePlan>> GetAllAsync() => await _context.CarePlans.ToListAsync();
    public async Task<CarePlan?> GetByIdAsync(Guid id) => await _context.CarePlans.FindAsync(id);
    
    public async Task<CarePlan> AddAsync(CarePlan carePlan)
    {
        _context.CarePlans.Add(carePlan);
        await _context.SaveChangesAsync();
        return carePlan;
    }
}