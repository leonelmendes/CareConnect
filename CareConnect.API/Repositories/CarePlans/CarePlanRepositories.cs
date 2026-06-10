using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.CarePlans;

public class CarePlanRepositories : ICarePlanRepositories
{
    private readonly AppDbContext _context;
    public CarePlanRepositories(AppDbContext context) { _context = context; }

    public async Task<IEnumerable<CarePlan>> GetAllByPatientIdAsync(Guid patientId, Guid gestorId)
    {
        return await _context.CarePlans
            .Include(c => c.Patient) // Traz os dados do Paciente associado
            .Where(c => c.PatientId == patientId && c.Patient!.GestorId == gestorId && c.Patient.Ativo)
            .ToListAsync();
    }

    public async Task<CarePlan?> GetByIdAsync(Guid id, Guid gestorId)
    {
        return await _context.CarePlans
            .Include(c => c.Patient)
            .FirstOrDefaultAsync(c => c.Id == id && c.Patient!.GestorId == gestorId && c.Patient.Ativo);
    }

    public async Task<CarePlan?> CreateAsync(CarePlan carePlan, Guid gestorId)
    {
        // 🛡️ Regra de Ouro: Validar se o paciente pertence a este gestor antes de aceitar o plano
        var pacienteValido = await _context.Patients
            .AnyAsync(p => p.Id == carePlan.PatientId && p.GestorId == gestorId && p.Ativo);

        if (!pacienteValido)
        {
            return null; // O paciente não existe ou é de outro gestor
        }

        await _context.CarePlans.AddAsync(carePlan);
        await _context.SaveChangesAsync();

        return carePlan;
    }

    public async Task<CarePlan?> UpdateAsync(Guid id, CarePlan carePlanAtualizado, Guid gestorId)
    {
        var planoExistente = await GetByIdAsync(id, gestorId);

        if (planoExistente == null) return null;

        // Atualiza apenas os dados do plano (não permitimos mudar o plano de paciente)
        planoExistente.Tipo = carePlanAtualizado.Tipo;
        planoExistente.Descricao = carePlanAtualizado.Descricao;
        planoExistente.HoraProgramada = carePlanAtualizado.HoraProgramada;
        planoExistente.Frequencia = carePlanAtualizado.Frequencia;

        await _context.SaveChangesAsync();

        return planoExistente;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid gestorId)
    {
        var planoExistente = await GetByIdAsync(id, gestorId);

        if (planoExistente == null) return false;

        // Ao contrário do Patient, aqui apagamos fisicamente da base de dados
        _context.CarePlans.Remove(planoExistente);
        await _context.SaveChangesAsync();

        return true;
    }
}