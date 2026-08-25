using System.Text;
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

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// BASE DE DADOS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CONTROLADORES E ROTAS
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Esta linha mágica previne o loop infinito sem precisares de usar [JsonIgnore]
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();

// REPOSITÓRIOS (Injeção de Dependências)
builder.Services.AddScoped<IPatientRepositories, PatientRepositories>();
builder.Services.AddScoped<IUserRepositories, UserRepositories>();
builder.Services.AddScoped<ICarePlanRepositories, CarePlanRepositories>();
builder.Services.AddScoped<ITaskLogRepositories, TaskLogRepositories>();
builder.Services.AddScoped<IAuthRepositories, AuthRepositories>();
builder.Services.AddScoped<S3Service>();

/// ==========================================
// 4. AUTENTICAÇÃO E JWT
// ==========================================
var jwtSecret = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:Secret"];

// ⚠️ TRAVA DE SEGURANÇA: Se a chave não existir, avisa imediatamente com uma mensagem clara!
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("❌ ERRO FATAL: A chave 'Jwt:Key' não foi encontrada no appsettings.json ou nos User Secrets! Verifique as configurações da API.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CareConnectAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CareConnectMobile"
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