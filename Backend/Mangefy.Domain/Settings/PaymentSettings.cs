using Mangefy.Domain.Common;
using Mangefy.Domain.Tabs;

namespace Mangefy.Domain.Settings;

public sealed class PaymentSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    private readonly List<PaymentMethod> _enabledMethods = [];
    public IReadOnlyList<PaymentMethod> EnabledMethods => _enabledMethods.AsReadOnly();

    private PaymentSettings() { }

    public static PaymentSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        var settings = new PaymentSettings { TenantId = tenantId };
        settings._enabledMethods.AddRange([
            PaymentMethod.Cash, PaymentMethod.CreditCard,
            PaymentMethod.DebitCard, PaymentMethod.Pix
        ]);
        return settings;
    }

    public void EnableMethod(PaymentMethod method)
    {
        if (!_enabledMethods.Contains(method))
        {
            _enabledMethods.Add(method);
            SetUpdatedAt();
        }
    }

    public void DisableMethod(PaymentMethod method)
    {
        if (_enabledMethods.Count == 1 && _enabledMethods.Contains(method))
            throw new DomainException("Pelo menos um método de pagamento deve estar habilitado.");

        _enabledMethods.Remove(method);
        SetUpdatedAt();
    }

    public bool IsMethodEnabled(PaymentMethod method) => _enabledMethods.Contains(method);
}
