namespace CareConnect.Shared.Models;

public enum UserRole
{
    Gestor,
    Default,
    Cuidador
}

public enum PlanType
{
    Medicacao,
    Higiene,
    Refeicao,
    Fisioterapia
}

public enum CareTaskStatus
{
    Pendente,
    Realizado,
    Falhado
}