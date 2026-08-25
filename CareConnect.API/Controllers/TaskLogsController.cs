using CareConnect.API.Data;
using CareConnect.API.Repositories.TaskLogs;
using CareConnect.API.Repositories.Users;
using CareConnect.API.Services;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CareConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskLogsController : ControllerBase
{
    private readonly ITaskLogRepositories _repository;
    private readonly IUserRepositories _userRepository;
    private readonly AppDbContext _context;
    private readonly S3Service _s3Service;

    public TaskLogsController(ITaskLogRepositories repository, IUserRepositories userRepository, S3Service s3Service, AppDbContext context)
    {
        _repository = repository;
        _userRepository = userRepository;
        _s3Service = s3Service;
        _context = context;
    }

    private async Task<User?> ObterUtilizadorAutenticadoAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");

        if (string.IsNullOrEmpty(userIdString))
            return null;

        if (!Guid.TryParse(userIdString, out Guid userId))
            return null;

        // Busca pelo ID real do Postgres!
        return await _context.Users.FindAsync(userId);
    }

    // GET: api/tasklogs/careplan/{carePlanId}
    [HttpGet("careplan/{carePlanId:guid}")]
    public async Task<IActionResult> GetAllByCarePlanId(Guid carePlanId)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var logs = await _repository.GetAllByCarePlanIdAsync(carePlanId, currentUser.Id);
        return Ok(logs);
    }

    // POST: api/tasklogs
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskLog novoLog)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        novoLog.Id = Guid.NewGuid();

        var logCriado = await _repository.CreateAsync(novoLog, currentUser.Id);

        if (logCriado == null) return BadRequest("Plano de cuidados inválido ou sem permissão.");

        return Ok(logCriado); 
    }

    // DTO auxiliar para receber apenas o Status e as Notas no método de atualização
    public class UpdateStatusDto
    {
        public CareTaskStatus Status { get; set; }
        public string Notas { get; set; } = string.Empty;
    }

    // PATCH: api/tasklogs/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var logAtualizado = await _repository.UpdateStatusAsync(id, dto.Status, dto.Notas, currentUser.Id);

        if (logAtualizado == null) return NotFound("Registo de tarefa não encontrado ou sem permissão.");

        return Ok(logAtualizado);
    }

    [HttpGet("dia/{data}")]
    public async Task<IActionResult> ObterTarefasDoDia(string data)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();

        // Se falhar agora, é porque o token expirou de facto
        if (currentUser == null) return Unauthorized();

        // Validar a data que vem do MAUI (ex: "2026-07-28")
        if (!DateTime.TryParse(data, out DateTime dataPesquisa))
            return BadRequest("Data com formato inválido.");

        // A MAGIA PARA O NEON/POSTGRESQL LER AS DATAS CORRETAMENTE
        var inicioDoDia = DateTime.SpecifyKind(dataPesquisa.Date, DateTimeKind.Utc);
        var fimDoDia = inicioDoDia.AddDays(1).AddTicks(-1);

        var tarefas = await _context.TaskLogs
            .Include(t => t.Utente)
            .Where(t => t.ExecutorId == currentUser.Id // Usa o Guid correto
                     && t.DataHoraAgendada >= inicioDoDia
                     && t.DataHoraAgendada <= fimDoDia)
            .OrderBy(t => t.DataHoraAgendada)
            .Select(t => new TarefaResumoDto
            {
                Id = t.Id,
                DataHora = t.DataHoraAgendada,
                Titulo = t.Titulo,
                Categoria = t.Categoria,
                NomeUtente = t.Utente != null ? t.Utente.Nome : "Utente Desconhecido",
                AvatarUtente = t.Utente != null ? t.Utente.AvatarUrl : string.Empty,
                Concluida = t.Status == CareTaskStatus.Realizado,
                TimestampExecucao = t.TimestampExecucao,
                Notas = t.Notas,
                IsAdHoc = t.IsAdHoc
            })
            .ToListAsync();

        return Ok(tarefas);
    }

    [HttpGet("gerar-pdf/{utenteId}/dia/{data}")]
    public async Task<IActionResult> GerarRelatorioPdf(Guid utenteId, DateTime data)
    {
        // 1. Vais à Base de Dados buscar as tarefas desse dia (SUBSTITUI PELA TUA LÓGICA)
        // var tarefas = await _context.TaskLogs.Where(...).ToListAsync();
        // var nomeUtente = "Carregar da BD";

        int totalConcluidas = 0; // tarefas.Count(t => t.Status == 1);
        int totalPendentes = 0;

        // 2. Desenhar o PDF na memória do Servidor
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("CareConnect - Relatório de Cuidados").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    // col.Item().Text($"Utente: {nomeUtente}").FontSize(14).SemiBold();
                    col.Item().Text($"Data: {data:dd/MM/yyyy}");
                    col.Item().Text($"Estatísticas: {totalConcluidas} Concluídas | {totalPendentes} Pendentes").FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(15).Text("Detalhe de Tarefas:").SemiBold().FontSize(14);

                    // AQUI FAZES O FOREACH DAS TAREFAS QUE VIERAM DA BD
                    /*
                    foreach(var t in tarefas) {
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(taskCol =>
                        {
                            taskCol.Item().Text($"{t.Titulo}").SemiBold();
                        });
                    }
                    */
                });
            });
        }).GeneratePdf(); // Gera diretamente para um array de bytes!

        // 3. Devolve o ficheiro em formato PDF para o telemóvel baixar
        return File(pdfBytes, "application/pdf", $"Relatorio_{data:yyyyMMdd}.pdf");
    }

    /*[HttpGet("dia/{data:datetime}")]
    public async Task<IActionResult> ObterTarefasDoDia(DateTime data)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        try
        {
            var tarefas = await _repository.ObterResumoTarefasDoDiaAsync(currentUser.Id, data);
            return Ok(tarefas);
        }
        catch (Exception)
        {
            return StatusCode(500, "Ocorreu um erro ao carregar as tarefas do dia.");
        }
    }*/

    [HttpPost("adhoc")]
    public async Task<IActionResult> CriarAdHoc([FromBody] RegistoAdHocDto dto)
    {
        var currentUser = await ObterUtilizadorAutenticadoAsync();
        if (currentUser == null) return Unauthorized();

        var sucesso = await _repository.RegistarAdHocAsync(currentUser.Id, dto);

        if (!sucesso) return BadRequest("Não foi possível registar a tarefa Ad-Hoc.");

        return Ok();
    }

    [HttpPost("upload-foto")]
    public async Task<IActionResult> UploadFotoAdHoc(IFormFile foto)
    {
        // 1. Validar autenticação (opcional dependendo de como tens o controller configurado)
        // var currentUser = await ObterUtilizadorAutenticadoAsync();
        // if (currentUser == null) return Unauthorized();

        // 2. Validar se a foto chegou bem
        if (foto == null || foto.Length == 0)
            return BadRequest("Nenhuma imagem enviada.");

        try
        {
            // 3. Manda para a pasta "tarefas" (ou "adhoc") no bucket S3!
            var urlS3 = await _s3Service.UploadFotoAsync(foto, "tarefas");

            // 4. Devolve o link no formato JSON exato que configurámos no Mobile
            return Ok(new { fotoUrl = urlS3 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno ao fazer upload da foto: {ex.Message}");
        }
    }

}