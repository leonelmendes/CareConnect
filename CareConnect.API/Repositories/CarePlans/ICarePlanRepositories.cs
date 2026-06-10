using CareConnect.Shared.Models;
namespace CareConnect.API.Repositories.CarePlans;

public interface ICarePlanRepositories
{
    // Obtém todos os planos de um paciente específico (garantindo que o paciente pertence a este gestor)
    Task<IEnumerable<CarePlan>> GetAllByPatientIdAsync(Guid patientId, Guid gestorId);

    // Obtém um plano específico
    Task<CarePlan?> GetByIdAsync(Guid id, Guid gestorId);

    // Cria um novo plano de cuidados para um paciente
    Task<CarePlan?> CreateAsync(CarePlan carePlan, Guid gestorId);

    // Atualiza um plano existente
    Task<CarePlan?> UpdateAsync(Guid id, CarePlan carePlanAtualizado, Guid gestorId);

    // Apaga um plano de cuidados
    Task<bool> DeleteAsync(Guid id, Guid gestorId);
}