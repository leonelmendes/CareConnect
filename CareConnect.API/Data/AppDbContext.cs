using CareConnect.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<CarePlan> CarePlans { get; set; }
    public DbSet<TaskLog> TaskLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Relação: Patient -> User (Gestor)
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Gestor)
            .WithMany(u => u.PacientesGeridos)
            .HasForeignKey(p => p.GestorId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict: Impede que apagues um User se ele tiver Pacientes.

        // 2. Relação: CarePlan -> Patient
        modelBuilder.Entity<CarePlan>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.CarePlans)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade: Se apagares o Paciente (hard delete), os Planos vão junto.

        // 3. Relação: TaskLog -> CarePlan
        modelBuilder.Entity<TaskLog>()
            .HasOne(t => t.CarePlan)
            .WithMany(c => c.TaskLogs)
            .HasForeignKey(t => t.CarePlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Relação: TaskLog -> User (Executor)
        modelBuilder.Entity<TaskLog>()
            .HasOne(t => t.Executor)
            .WithMany(u => u.TarefasExecutadas)
            .HasForeignKey(t => t.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict: Protege os registos históricos. Não podes apagar um User se ele já executou tarefas.
    }
}