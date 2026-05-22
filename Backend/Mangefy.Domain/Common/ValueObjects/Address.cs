namespace Mangefy.Domain.Common.ValueObjects;

public sealed class Address : ValueObject
{
    public string Cep { get; }
    public string Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Uf { get; }

    private Address(string cep, string logradouro, string numero, string? complemento,
        string bairro, string cidade, string uf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }

    public static Address Create(string cep, string logradouro, string numero,
        string bairro, string cidade, string uf, string? complemento = null)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new DomainException("CEP não pode ser vazio.");

        var cepDigits = new string(cep.Where(char.IsDigit).ToArray());
        if (cepDigits.Length != 8)
            throw new DomainException("CEP inválido — deve conter 8 dígitos.");

        if (string.IsNullOrWhiteSpace(logradouro))
            throw new DomainException("Logradouro não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("Número não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(bairro))
            throw new DomainException("Bairro não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(cidade))
            throw new DomainException("Cidade não pode ser vazia.");

        if (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2)
            throw new DomainException("UF inválida — deve ter 2 caracteres.");

        return new Address(
            cepDigits,
            logradouro.Trim(),
            numero.Trim(),
            complemento?.Trim(),
            bairro.Trim(),
            cidade.Trim(),
            uf.Trim().ToUpperInvariant());
    }

    public string CepFormatado => $"{Cep[..5]}-{Cep[5..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Cep;
        yield return Logradouro;
        yield return Numero;
        yield return Complemento ?? string.Empty;
        yield return Bairro;
        yield return Cidade;
        yield return Uf;
    }
}
