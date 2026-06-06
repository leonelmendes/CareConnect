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
}