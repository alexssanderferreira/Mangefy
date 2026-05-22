using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Plans;

public sealed class Plan : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Money MonthlyPrice { get; private set; }
    public int MaxTables { get; private set; }
    public int MaxMenuItems { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxCustomRoles { get; private set; }  // 0 = não permite cargos customizados
    public PlanStatus Status { get; private set; }

    private Plan() { }

    public static Plan Create(
        string name,
        decimal monthlyPrice,
        int maxTables,
        int maxMenuItems,
        int maxUsers,
        int maxCustomRoles = 0,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do plano não pode ser vazio.");

        if (monthlyPrice < 0)
            throw new DomainException("Preço mensal não pode ser negativo.");

        if (maxTables <= 0)
            throw new DomainException("Número máximo de mesas deve ser maior que zero.");

        if (maxMenuItems <= 0)
            throw new DomainException("Número máximo de itens do cardápio deve ser maior que zero.");

        if (maxUsers <= 0)
            throw new DomainException("Número máximo de usuários deve ser maior que zero.");

        if (maxCustomRoles < 0)
            throw new DomainException("Limite de cargos customizados não pode ser negativo.");

        return new Plan
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            MonthlyPrice = Money.Create(monthlyPrice),
            MaxTables = maxTables,
            MaxMenuItems = maxMenuItems,
            MaxUsers = maxUsers,
            MaxCustomRoles = maxCustomRoles,
            Status = PlanStatus.Active
        };
    }

    public void UpdatePricing(decimal newMonthlyPrice)
    {
        MonthlyPrice = Money.Create(newMonthlyPrice);
        SetUpdatedAt();
    }

    public void UpdateLimits(int maxTables, int maxMenuItems, int maxUsers, int maxCustomRoles)
    {
        if (maxTables <= 0)
            throw new DomainException("Número máximo de mesas deve ser maior que zero.");
        if (maxMenuItems <= 0)
            throw new DomainException("Número máximo de itens do cardápio deve ser maior que zero.");
        if (maxUsers <= 0)
            throw new DomainException("Número máximo de usuários deve ser maior que zero.");
        if (maxCustomRoles < 0)
            throw new DomainException("Limite de cargos customizados não pode ser negativo.");

        MaxTables = maxTables;
        MaxMenuItems = maxMenuItems;
        MaxUsers = maxUsers;
        MaxCustomRoles = maxCustomRoles;
        SetUpdatedAt();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Status = PlanStatus.Inactive;
        SetUpdatedAt();
    }

    public void Activate()
    {
        Status = PlanStatus.Active;
        SetUpdatedAt();
    }

    public bool AllowsCustomRoles() => MaxCustomRoles > 0;
}
