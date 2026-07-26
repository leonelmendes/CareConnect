using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Patients;

public interface IPatientRepositories
{
    // Obtém todos os pacientes ativos que pertencem a um gestor específico
    Task<IEnumerable<Patient>> GetAllByGestorIdAsync(Guid gestorId);

    // Obtém um paciente específico, garantindo que pertence a esse gestor
    Task<Patient?> GetByIdAsync(Guid id, Guid gestorId);

    // Cria um novo paciente
    Task<Patient> CreateAsync(Patient patient);

    // Atualiza os dados de um paciente existente
    Task<Patient?> UpdateAsync(Guid id, Patient patientAtualizado, Guid gestorId);

    // Inativa um paciente (Soft Delete) em vez de o apagar da base de dados
    Task<bool> DeactivateAsync(Guid id, Guid gestorId);
    // Obtém apenas os pacientes ativos atribuídos a um cuidador específico
    Task<IEnumerable<Patient>> GetPacientesDoCuidadorAsync(Guid cuidadorId);
}