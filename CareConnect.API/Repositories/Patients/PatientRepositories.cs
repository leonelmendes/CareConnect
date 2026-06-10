using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Patients;

public class PatientRepositories : IPatientRepositories
{
    private readonly AppDbContext _context;

    public PatientRepositories(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Patient>> GetAllByGestorIdAsync(Guid gestorId)
    {
        return await _context.Patients
            .Where(p => p.GestorId == gestorId && p.Ativo)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(Guid id, Guid gestorId)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.GestorId == gestorId && p.Ativo);
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        // O ID já é gerado automaticamente pelo modelo, apenas adicionamos e guardamos
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
        
        return patient;
    }

    public async Task<Patient?> UpdateAsync(Guid id, Patient patientAtualizado, Guid gestorId)
    {
        var patientExistente = await GetByIdAsync(id, gestorId);

        if (patientExistente == null)
        {
            return null; // O controlador vai saber que deve devolver um 404 Not Found
        }

        // Atualizamos apenas os campos permitidos
        patientExistente.Nome = patientAtualizado.Nome;
        patientExistente.DataNascimento = patientAtualizado.DataNascimento;
        patientExistente.Contacto = patientAtualizado.Contacto;
        patientExistente.ContactoEmergencia = patientAtualizado.ContactoEmergencia;
        patientExistente.CondicoesMedicas = patientAtualizado.CondicoesMedicas;
        patientExistente.Alergias = patientAtualizado.Alergias;
        patientExistente.Notas = patientAtualizado.Notas;

        await _context.SaveChangesAsync();
        
        return patientExistente;
    }

    public async Task<bool> DeactivateAsync(Guid id, Guid gestorId)
    {
        var patient = await GetByIdAsync(id, gestorId);

        if (patient == null)
        {
            return false;
        }

        // Soft Delete: Apenas mudamos o estado para falso
        patient.Ativo = false;
        await _context.SaveChangesAsync();

        return true;
    }
}