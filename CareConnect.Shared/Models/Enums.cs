namespace CareConnect.Shared.Models;

public enum UserRole
{
    Gestor,
    Cuidador
}

public enum PlanType
{
    Medicacao,
    Higiene,
    Terapia,
    Alimentacao
}

public enum CareTaskStatus
{
    Pendente,
    Realizado,
    Falhado
}