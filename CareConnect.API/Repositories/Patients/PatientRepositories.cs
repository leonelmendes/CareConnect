using Microsoft.EntityFrameworkCore;
using CareConnect.API.Data;
using CareConnect.Shared.Models;

namespace CareConnect.API.Repositories.Patients
{
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
                .Include(p => p.Cuidadores) // <-- ADICIONADO AQUI
                .Where(p => p.GestorId == gestorId && p.Ativo)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(Guid id, Guid gestorId)
        {
            return await _context.Patients
                .Include(p => p.Cuidadores) // <-- ADICIONADO AQUI
                .FirstOrDefaultAsync(p => p.Id == id && p.GestorId == gestorId && p.Ativo);
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            // 1. Inicializa a lista de cuidadores do Entity Framework
            patient.Cuidadores = new List<User>();

            // 2. Transforma os IDs que vieram do telemóvel nos objetos reais dos Cuidadores
            if (patient.CuidadoresIds != null && patient.CuidadoresIds.Any())
            {
                var cuidadoresReais = await _context.Users
                    .Where(u => patient.CuidadoresIds.Contains(u.Id))
                    .ToListAsync();

                foreach (var cuidador in cuidadoresReais)
                {
                    patient.Cuidadores.Add(cuidador);
                }
            }

            // 3. O Entity Framework agora vê os cuidadores anexados e guarda a relação automaticamente
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

        public async Task<IEnumerable<Patient>> GetPacientesDoCuidadorAsync(Guid cuidadorId)
        {
            return await _context.Patients
                .Include(p => p.Cuidadores) // Traz os dados do cuidador para a App
                .Where(p => p.Cuidadores.Any(c => c.Id == cuidadorId) && p.Ativo) // Filtra apenas os utentes DESTE cuidador
                .ToListAsync();
        }
    }
}