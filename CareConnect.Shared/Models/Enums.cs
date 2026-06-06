namespace CareConnect.Shared.Models;

public enum UserRole
{
    Gestor,
    Executor
}

public enum PlanType
{
    Medicacao,
    Higiene,
    Refeicao,
    Fisioterapia
}

public enum TaskStatus
{
    Pendente,
    Realizado,
    Falhado
}