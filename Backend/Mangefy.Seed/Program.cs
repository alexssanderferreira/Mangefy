using System.Net.Http.Json;
using System.Text.Json;

const string BASE        = "http://localhost:5184/api";
const string ADMIN_EMAIL = "admin@mangefy.com.br";
var ADMIN_PASS  = args.Length > 0 ? args[0] : (Environment.GetEnvironmentVariable("MANGEFY_SEED_PASS") ?? "Admin@123");

var http = new HttpClient();
var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

Console.WriteLine("=== Mangefy Seed ===\n");

// ── 1. Login admin ───────────────────────────────────────────────────────────
Console.Write("Autenticando admin... ");
var loginRes = await http.PostAsJsonAsync($"{BASE}/auth/admin/login", new { email = ADMIN_EMAIL, password = ADMIN_PASS });
loginRes.EnsureSuccessStatusCode();
var loginData = await loginRes.Content.ReadFromJsonAsync<JsonElement>();
var adminToken = loginData.GetProperty("accessToken").GetString()!;
http.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
Console.WriteLine("OK");

// ── 2. Tipo de Negócio ────────────────────────────────────────────────────────
Console.Write("Criando tipo de negócio 'Restaurante'... ");
var btRes = await http.PostAsJsonAsync($"{BASE}/admin/business-types", new { name = "Restaurante", description = "Restaurante tradicional com atendimento em mesa" });
Guid btId;
if (btRes.IsSuccessStatusCode)
{
    var bt = await btRes.Content.ReadFromJsonAsync<JsonElement>();
    btId = bt.GetProperty("id").GetGuid();
    Console.WriteLine($"OK ({btId})");
}
else
{
    var list = await http.GetFromJsonAsync<JsonElement[]>($"{BASE}/admin/business-types");
    btId = list![0].GetProperty("id").GetGuid();
    Console.WriteLine($"já existe ({btId})");
}

// ── 3. Planos ─────────────────────────────────────────────────────────────────
Console.WriteLine("Criando planos...");
var plans = new[]
{
    new { name = "Starter",      monthlyPrice = 99m,  maxTables = 5,  maxMenuItems = 50,  maxUsers = 3,  maxCustomRoles = 0,  description = "Ideal para pequenos estabelecimentos" },
    new { name = "Profissional", monthlyPrice = 199m, maxTables = 20, maxMenuItems = 200, maxUsers = 10, maxCustomRoles = 3,  description = "Para restaurantes em crescimento" },
    new { name = "Enterprise",   monthlyPrice = 399m, maxTables = 99, maxMenuItems = 999, maxUsers = 50, maxCustomRoles = 10, description = "Para redes e grandes operações" },
};

var planIds = new List<Guid>();
foreach (var p in plans)
{
    Console.Write($"  → {p.name}... ");
    var res = await http.PostAsJsonAsync($"{BASE}/admin/plans", p);
    if (res.IsSuccessStatusCode)
    {
        var data = await res.Content.ReadFromJsonAsync<JsonElement>();
        var id = data.GetProperty("id").GetGuid();
        planIds.Add(id);
        await http.PatchAsync($"{BASE}/admin/plans/{id}/activate", null);
        Console.WriteLine($"OK ({id})");
    }
    else
    {
        Console.WriteLine($"falhou ({res.StatusCode})");
    }
}

if (planIds.Count == 0)
{
    Console.WriteLine("Nenhum plano criado. Abortando.");
    return;
}

// ── 4. PlanFeatureSets ────────────────────────────────────────────────────────
Console.WriteLine("Configurando feature sets por plano...");

// Starter: apenas funcionalidades básicas
var starterFeatures = new[]
{
    "features.tabs",
    "features.stock_basic",
    "features.reports_basic",
    "features.daily_cash",
};

// Profissional: tudo do Starter + extras
var profissionalFeatures = new[]
{
    "features.tabs",
    "features.kds",
    "features.multi_menu",
    "features.stock_basic",
    "features.stock_advanced",
    "features.reports_basic",
    "features.reports_advanced",
    "features.daily_cash",
    "features.reservations",
    "features.custom_roles",
};

// Enterprise: todas as features
var enterpriseFeatures = new[]
{
    "features.tabs",
    "features.kds",
    "features.multi_menu",
    "features.stock_basic",
    "features.stock_advanced",
    "features.reports_basic",
    "features.reports_advanced",
    "features.daily_cash",
    "features.reservations",
    "features.delivery",
    "features.custom_roles",
};

var featureMatrix = new[]
{
    (planIdx: 0, features: starterFeatures),
    (planIdx: 1, features: profissionalFeatures),
    (planIdx: 2, features: enterpriseFeatures),
};

foreach (var (planIdx, features) in featureMatrix)
{
    var planId = planIds[planIdx];
    Console.Write($"  → plano {plans[planIdx].name} × Restaurante... ");
    var res = await http.PutAsJsonAsync(
        $"{BASE}/admin/plans/{planId}/feature-sets/{btId}",
        new { enabledFeatures = features });
    Console.WriteLine(res.IsSuccessStatusCode ? "OK" : $"falhou ({res.StatusCode})");
}

// ── 5. Owners ─────────────────────────────────────────────────────────────────
Console.WriteLine("Criando owners...");
var owners = new[]
{
    new { name = "José da Silva",    email = "ze@cantinaze.com.br"       },
    new { name = "Maria Souza",      email = "contato@bomsabor.com.br"   },
    new { name = "Giovanni Rossi",   email = "napoli@bellanapoli.com.br" },
    new { name = "Carlos Gaúcho",    email = "gaucha@gaucha.com.br"      },
    new { name = "Paulo Bistrot",    email = "bistro@paulistano.com.br"  },
    new { name = "Ana da Vila",      email = "vila@lanchvila.com.br"     },
    new { name = "Roberto Café",     email = "cafe@cafecentral.com.br"   },
    new { name = "Kenji Nagoya",     email = "nagoya@sushinagoya.com.br" },
    new { name = "Bruno Bros",       email = "bros@hambros.com.br"       },
    new { name = "Cláudia União",    email = "uniao@padariao.com.br"     },
};

var DEFAULT_OWNER_PASSWORD = ADMIN_PASS;

var ownerIds = new List<Guid>();
foreach (var o in owners)
{
    Console.Write($"  → {o.name}... ");
    var res = await http.PostAsJsonAsync($"{BASE}/admin/owners", o);
    if (!res.IsSuccessStatusCode)
    {
        var err = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"falhou ({res.StatusCode}): {err[..Math.Min(120, err.Length)]}");
        ownerIds.Add(Guid.Empty);
        continue;
    }

    var data = await res.Content.ReadFromJsonAsync<JsonElement>();
    var id = data.GetProperty("id").GetGuid();
    var activationToken = data.GetProperty("activationToken").GetString()!;

    var activateRes = await http.PostAsJsonAsync($"{BASE}/auth/owner/activate", new { token = activationToken, newPassword = DEFAULT_OWNER_PASSWORD });
    if (activateRes.IsSuccessStatusCode)
    {
        ownerIds.Add(id);
        Console.WriteLine($"OK ({id}) — ativado");
    }
    else
    {
        var err = await activateRes.Content.ReadAsStringAsync();
        Console.WriteLine($"criado mas falhou ativar ({activateRes.StatusCode}): {err[..Math.Min(80, err.Length)]}");
        ownerIds.Add(id);
    }
}

// ── 6. Tenants ────────────────────────────────────────────────────────────────
Console.WriteLine("Criando tenants...");
var tenants = new[]
{
    // Ativos
    new { name = "Cantina do Zé",          slug = "cantina-ze",          planIdx = 0, ownerIdx = 0, trialDays = 0,  suspend = false },
    new { name = "Restaurante Bom Sabor",  slug = "bom-sabor",           planIdx = 1, ownerIdx = 1, trialDays = 0,  suspend = false },
    new { name = "Pizzaria Bella Napoli",  slug = "bella-napoli",        planIdx = 1, ownerIdx = 2, trialDays = 0,  suspend = false },
    new { name = "Churrascaria Gaúcha",    slug = "churrascaria-gaucha", planIdx = 2, ownerIdx = 3, trialDays = 0,  suspend = false },
    new { name = "Bistrô Paulistano",      slug = "bistro-paulistano",   planIdx = 2, ownerIdx = 4, trialDays = 0,  suspend = false },
    // Trial
    new { name = "Lanchonete da Vila",     slug = "lanchonete-vila",     planIdx = 0, ownerIdx = 5, trialDays = 14, suspend = false },
    new { name = "Café Central",           slug = "cafe-central",        planIdx = 1, ownerIdx = 6, trialDays = 7,  suspend = false },
    new { name = "Sushi Nagoya",           slug = "sushi-nagoya",        planIdx = 1, ownerIdx = 7, trialDays = 3,  suspend = false },
    // Suspensos
    new { name = "Hamburgueria Bros",      slug = "hamburgueria-bros",   planIdx = 0, ownerIdx = 8, trialDays = 0,  suspend = true  },
    new { name = "Padaria União",          slug = "padaria-uniao",       planIdx = 0, ownerIdx = 9, trialDays = 0,  suspend = true  },
};

var createdTenants = new List<(Guid id, string slug, bool suspend)>();
foreach (var t in tenants)
{
    Console.Write($"  → {t.name}... ");
    var planId  = planIds[Math.Min(t.planIdx, planIds.Count - 1)];
    var ownerId = ownerIds[t.ownerIdx];

    if (ownerId == Guid.Empty)
    {
        Console.WriteLine("pulado (owner não criado)");
        continue;
    }

    var res = await http.PostAsJsonAsync($"{BASE}/tenants", new
    {
        name           = t.name,
        slug           = t.slug,
        ownerId        = ownerId,
        planId         = planId,
        businessTypeId = btId,
        timezone       = "America/Sao_Paulo",
        trialDays      = t.trialDays
    });

    if (res.IsSuccessStatusCode)
    {
        var data = await res.Content.ReadFromJsonAsync<JsonElement>();
        var id = data.GetProperty("id").GetGuid();
        createdTenants.Add((id, t.slug, t.suspend));
        Console.WriteLine($"OK ({id})");
    }
    else
    {
        var err = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"falhou ({res.StatusCode}): {err[..Math.Min(120, err.Length)]}");
    }
}

// ── 7. Suspender ─────────────────────────────────────────────────────────────
Console.WriteLine("Suspendendo tenants...");
foreach (var (id, _, _) in createdTenants.Where(x => x.suspend))
{
    Console.Write($"  → {id}... ");
    var res = await http.PatchAsync($"{BASE}/tenants/{id}/suspend", null);
    Console.WriteLine(res.IsSuccessStatusCode ? "OK" : $"falhou ({res.StatusCode})");
}

// ── 8. Dados operacionais por tenant ─────────────────────────────────────────
Console.WriteLine("Criando dados operacionais por tenant...");

foreach (var (tenantId, slug, suspend) in createdTenants.Where(x => !x.suspend))
{
    Console.WriteLine($"  [{slug}]");

    // 8a. Login como owner para obter token do tenant
    var ownerIdx = Array.FindIndex(tenants, t => t.slug == slug);
    if (ownerIdx < 0) continue;
    var ownerEmail = owners[tenants[ownerIdx].ownerIdx].email;

    Console.Write($"    Login owner... ");
    var ownerLoginRes = await http.PostAsJsonAsync($"{BASE}/auth/login", new
    {
        tenantSlug = slug,
        email      = ownerEmail,
        password   = DEFAULT_OWNER_PASSWORD
    });

    if (!ownerLoginRes.IsSuccessStatusCode)
    {
        Console.WriteLine($"falhou ({ownerLoginRes.StatusCode}) — pulando tenant");
        continue;
    }

    var ownerLoginData = await ownerLoginRes.Content.ReadFromJsonAsync<JsonElement>();
    var ownerToken = ownerLoginData.GetProperty("accessToken").GetString()!;
    var tenantHttp = new HttpClient();
    tenantHttp.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);
    Console.WriteLine("OK");

    var tBase = $"{BASE}/tenants/{tenantId}";

    // 8b. Cargo padrão (apenas em planos com features.custom_roles)
    var planIdx2 = Array.FindIndex(tenants, t => t.slug == slug);
    var hasCustoRoles = planIdx2 >= 0 && tenants[planIdx2].planIdx >= 1; // Profissional e Enterprise
    Guid roleId = Guid.Empty;
    if (hasCustoRoles)
    {
        Console.Write($"    Cargo 'Gerente'... ");
        var roleRes = await tenantHttp.PostAsJsonAsync($"{tBase}/roles", new
        {
            name        = "Gerente",
            description = "Acesso completo às operações do estabelecimento",
            permissions = new[]
            {
                "tabs.read", "tabs.create", "tabs.close", "tabs.cancel",
                "orders.read", "orders.create", "orders.update_status", "orders.cancel",
                "stock.read", "stock.manage",
                "reports.read",
                "employees.read", "employees.manage",
                "menu.read",
                "tables.read"
            }
        });
        if (roleRes.IsSuccessStatusCode)
        {
            var rd = await roleRes.Content.ReadFromJsonAsync<JsonElement>();
            roleId = rd.GetProperty("id").GetGuid();
            Console.WriteLine($"OK ({roleId})");
        }
        else
        {
            var err = await roleRes.Content.ReadAsStringAsync();
            Console.WriteLine($"falhou ({roleRes.StatusCode}): {err[..Math.Min(100, err.Length)]}");
        }
    }
    else
    {
        // Busca o cargo gerado pelo onboarding para Starter
        var rolesListRes = await tenantHttp.GetAsync($"{tBase}/roles");
        if (rolesListRes.IsSuccessStatusCode)
        {
            var rolesList = await rolesListRes.Content.ReadFromJsonAsync<JsonElement[]>();
            var templateRole = rolesList?.FirstOrDefault(r =>
                r.TryGetProperty("isOwnerRole", out var v) && !v.GetBoolean());
            if (templateRole?.ValueKind == JsonValueKind.Object)
                roleId = templateRole.Value.GetProperty("id").GetGuid();
        }
        Console.WriteLine($"    Cargo onboarding usado ({roleId})");
    }

    // 8c. Funcionário administrador
    if (roleId != Guid.Empty)
    {
        Console.Write($"    Funcionário 'Garçom'... ");
        var empRes = await tenantHttp.PostAsJsonAsync($"{tBase}/employees", new
        {
            name        = "Garçom Teste",
            email       = $"garcom@{slug}.mangefy.local",
            tenantRoleId = roleId
        });
        if (!empRes.IsSuccessStatusCode)
        {
            var err = await empRes.Content.ReadAsStringAsync();
            Console.WriteLine($"falhou ({empRes.StatusCode}): {err[..Math.Min(100, err.Length)]}");
        }
        else Console.WriteLine("OK");
    }

    // 8d. 5 Mesas
    Console.Write($"    Mesas (1–5)... ");
    var tableOk = 0;
    for (var i = 1; i <= 5; i++)
    {
        var tableRes = await tenantHttp.PostAsJsonAsync($"{tBase}/tables", new
        {
            number   = i.ToString(),
            capacity = 4,
            section  = "Salão"
        });
        if (!tableRes.IsSuccessStatusCode && tableOk == 0 && i == 1)
        {
            var err = await tableRes.Content.ReadAsStringAsync();
            Console.Write($"erro: {err[..Math.Min(80, err.Length)]} — ");
        }
        if (tableRes.IsSuccessStatusCode) tableOk++;
    }
    Console.WriteLine($"{tableOk}/5 criadas");

    // 8e. Usar menu padrão existente (criado no onboarding) ou criar novo se plano permitir
    Console.Write($"    Menu principal... ");
    var menusRes = await tenantHttp.GetAsync($"{tBase}/menus");
    Guid menuId = Guid.Empty;
    if (menusRes.IsSuccessStatusCode)
    {
        var menusList = await menusRes.Content.ReadFromJsonAsync<JsonElement[]>();
        if (menusList?.Length > 0)
            menuId = menusList[0].GetProperty("id").GetGuid();
    }
    if (menuId == Guid.Empty)
    {
        var menuRes = await tenantHttp.PostAsJsonAsync($"{tBase}/menus", new { name = "Cardápio Principal" });
        if (!menuRes.IsSuccessStatusCode)
        {
            var err = await menuRes.Content.ReadAsStringAsync();
            Console.WriteLine($"falhou criar menu ({menuRes.StatusCode}): {err[..Math.Min(100, err.Length)]}");
            continue;
        }
        var menuData = await menuRes.Content.ReadFromJsonAsync<JsonElement>();
        menuId = menuData.GetProperty("id").GetGuid();
    }

    var catRes = await tenantHttp.PostAsJsonAsync($"{tBase}/menus/{menuId}/categories", new
    {
        name        = "Pratos Principais",
        description = "Pratos quentes servidos no almoço e jantar"
    });
    if (!catRes.IsSuccessStatusCode)
    {
        var err = await catRes.Content.ReadAsStringAsync();
        Console.WriteLine($"falhou criar categoria ({catRes.StatusCode}): {err[..Math.Min(100, err.Length)]}");
        continue;
    }
    var catData = await catRes.Content.ReadFromJsonAsync<JsonElement>();
    var catId = catData.GetProperty("id").GetGuid();

    var menuItems = new[]
    {
        new { name = "Prato do Dia",    price = 32.90m, description = "Prato executivo com arroz, feijão, salada e proteína do dia" },
        new { name = "Frango Grelhado", price = 38.50m, description = "Filé de frango grelhado com legumes salteados" },
        new { name = "Suco Natural",    price = 12.00m, description = "Suco de frutas da estação, 500ml" },
    };

    var itemsOk = 0;
    foreach (var item in menuItems)
    {
        var itemRes = await tenantHttp.PostAsJsonAsync($"{tBase}/menus/{menuId}/categories/{catId}/items", new
        {
            name        = item.name,
            description = item.description,
            price       = item.price,
            station     = "Kitchen"
        });
        if (itemRes.IsSuccessStatusCode) itemsOk++;
    }
    Console.WriteLine($"menu OK ({menuId}), {itemsOk}/3 itens criados");
}

Console.WriteLine("\n✓ Seed concluído!");
Console.WriteLine($"  Planos:        {planIds.Count}");
Console.WriteLine($"  Feature sets:  {featureMatrix.Length}");
Console.WriteLine($"  Owners:        {ownerIds.Count(id => id != Guid.Empty)}");
Console.WriteLine($"  Tenants:       {createdTenants.Count}");
Console.WriteLine($"    Ativos:      {tenants.Count(t => !t.suspend && t.trialDays == 0)}");
Console.WriteLine($"    Trial:       {tenants.Count(t => t.trialDays > 0)}");
Console.WriteLine($"    Suspensos:   {tenants.Count(t => t.suspend)}");
Console.WriteLine($"\n  Senha utilizada: {(args.Length > 0 ? "arg[0]" : Environment.GetEnvironmentVariable("MANGEFY_SEED_PASS") is not null ? "env MANGEFY_SEED_PASS" : "padrão")}");
Console.WriteLine($"  Login dos donos: e-mail do owner + senha do seed + slug do tenant.");
