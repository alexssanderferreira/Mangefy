# Configuração local — Mangefy Backend

Os segredos sensíveis **não estão no repositório**. Configure-os via `dotnet user-secrets` antes de rodar o projeto localmente.

## 1. Inicializar user-secrets

```bash
cd Backend/Mangefy.API
dotnet user-secrets init
```

## 2. Definir os segredos

```bash
dotnet user-secrets set "Jwt:Key" "sua-chave-secreta-com-pelo-menos-32-caracteres"
dotnet user-secrets set "AdminSaas:Email" "admin@mangefy.com"
dotnet user-secrets set "AdminSaas:PasswordHash" "<hash-bcrypt-gerado-abaixo>"
```

## 3. Gerar o hash do AdminSaas

Use BCrypt work factor 12. Exemplo em C# (rode um script ou use uma ferramenta online de BCrypt):

```csharp
string hash = BCrypt.Net.BCrypt.HashPassword("suaSenhaAdmin", workFactor: 12);
Console.WriteLine(hash);
```

## 4. Variáveis de ambiente (alternativa ao user-secrets)

Em produção ou CI, defina as variáveis de ambiente:

```
ConnectionStrings__DefaultConnection=<connection-string>
Jwt__Key=<chave-32-chars>
AdminSaas__Email=<email>
AdminSaas__PasswordHash=<hash-bcrypt>
```

> **Nunca comite valores reais de Jwt:Key ou AdminSaas em appsettings.json.**
