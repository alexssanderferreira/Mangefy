using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

/// <summary>
/// Configurações de comanda: numeração, política de desconto e cortesia.
/// </summary>
public sealed class TabSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public int MinTabNumber { get; private set; }
    public int MaxTabNumber { get; private set; }

    /// <summary>
    /// Percentual máximo de desconto que o cargo sem permissão de override pode aplicar.
    /// 0 = sem desconto permitido; 100 = sem limite (qualquer valor).
    /// Padrão: 10%.
    /// </summary>
    public decimal MaxDiscountPercent { get; private set; }

    /// <summary>
    /// Acima de qual valor absoluto (em reais) o motivo do desconto é obrigatório.
    /// Nulo = nunca obrigatório.
    /// </summary>
    public decimal? DiscountReasonRequiredAbove { get; private set; }

    private TabSettings() { }

    public static TabSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new TabSettings
        {
            TenantId = tenantId,
            MinTabNumber = 1,
            MaxTabNumber = 50,
            MaxDiscountPercent = 10m,
            DiscountReasonRequiredAbove = null
        };
    }

    public void UpdateRange(int min, int max)
    {
        if (min < 1)
            throw new DomainException("Número mínimo de comanda deve ser maior que zero.");

        if (max <= min)
            throw new DomainException("Número máximo deve ser maior que o mínimo.");

        MinTabNumber = min;
        MaxTabNumber = max;
        SetUpdatedAt();
    }

    public void UpdateDiscountPolicy(decimal maxDiscountPercent, decimal? discountReasonRequiredAbove)
    {
        if (maxDiscountPercent < 0 || maxDiscountPercent > 100)
            throw new DomainException("Percentual máximo de desconto deve estar entre 0 e 100.");

        if (discountReasonRequiredAbove.HasValue && discountReasonRequiredAbove.Value < 0)
            throw new DomainException("Limite de motivo de desconto não pode ser negativo.");

        MaxDiscountPercent = maxDiscountPercent;
        DiscountReasonRequiredAbove = discountReasonRequiredAbove;
        SetUpdatedAt();
    }

    public int TotalNumbers => MaxTabNumber - MinTabNumber + 1;

    public bool IsValidNumber(int number) => number >= MinTabNumber && number <= MaxTabNumber;
}
