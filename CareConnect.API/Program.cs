using CareConnect.API.Data;
using CareConnect.API.Repositories.Auth; // <-- Adicionado para corrigir as referências do Swagger
using CareConnect.API.Repositories.CarePlans;
using CareConnect.API.Repositories.Patients;
using CareConnect.API.Repositories.TaskLogs;
using CareConnect.API.Repositories.Users;
using CareConnect.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// BASE DE DADOS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CONTROLADORES E ROTAS
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();

// REPOSITÓRIOS (Injeção de Dependências)
builder.Services.AddScoped<IPatientRepositories, PatientRepositories>();
builder.Services.AddScoped<IUserRepositories, UserRepositories>();
builder.Services.AddScoped<ICarePlanRepositories, CarePlanRepositories>();
builder.Services.AddScoped<ITaskLogRepositories, TaskLogRepositories>();
builder.Services.AddScoped<IAuthRepositories, AuthRepositories>();
builder.Services.AddScoped<S3Service>();

// ==========================================
// 4. AUTENTICAÇÃO (Firebase)
// ==========================================
var firebaseProjectId = "careconnect-30522";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

//SWAGGER (Com Segurança JWT)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    {
        Title = "CareConnect API",
        Version = "v1",
    });

    // jwt bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, 
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira APENAS o token gigante do Firebase (não precisa de escrever 'Bearer ' antes)."
    });

    // 2. Aplica o Cadeado a todos os Endpoints (Requisito de Segurança)
    c.AddSecurityRequirement( doc =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
        });
});

var app = builder.Build();

// ==========================================
// 6. PIPELINE HTTP
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// A ordem destes dois é vital!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();