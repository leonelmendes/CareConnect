using System;
using System.Text.Json.Serialization;

namespace CareConnect.Shared.Models;

public class TaskLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Referência à tarefa original agendada (Será NULL se a tarefa for Ad-Hoc)
    public Guid? CarePlanId { get; set; }

    // Quem vai executar / executou
    public Guid ExecutorId { get; set; }
    
    // A quem se destina (Essencial para Ad-Hoc, mas útil para todas para evitar joins pesados na API)
    public Guid? UtenteId { get; set; }

    // --- DADOS DA TAREFA (Visíveis na Timeline) ---
    // Substituímos "TituloAdHoc" por "Titulo". Serve tanto para as planeadas como para as Ad-Hoc.
    public string Titulo { get; set; } = string.Empty; 
    
    // Categoria para mapear os ícones na UI (Medicação, Higiene, Alimentação)
    public string Categoria { get; set; } = string.Empty; 

    // --- GESTÃO DE TEMPO ---
    // Quando é que a tarefa DEVE aparecer na Timeline
    public DateTime DataHoraAgendada { get; set; } 
    
    // Quando é que o cuidador clicou em "Salvar" no Modal
    public DateTime? TimestampExecucao { get; set; } 

    // --- ESTADO E TIPO ---
    public CareTaskStatus Status { get; set; } // Ex: Pendente, Concluida, NaoRealizada
    public bool IsAdHoc { get; set; }

    // --- DADOS DO MODAL DE EXECUÇÃO ---
    public string Notas { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;

    // --- PROPRIEDADES DE NAVEGAÇÃO (ENTITY FRAMEWORK) ---
    [JsonIgnore]
    public CarePlan? CarePlan { get; set; }

    [JsonIgnore]
    public User? Executor { get; set; }
    
     [JsonIgnore]
     public Patient? Utente { get; set; }
}