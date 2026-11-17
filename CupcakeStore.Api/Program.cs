using System.Globalization;
using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Números sempre com ponto decimal (formulários e JSON), independente do Windows estar em pt-BR.
// A exibição em reais é feita por Formatacao.Brl().
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Render e outras hospedagens informam a porta pela variável PORT.
var porta = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(porta))
    builder.WebHost.UseUrls($"http://0.0.0.0:{porta}");

// Atrás do túnel da Cloudflare a API recebe HTTP puro; o proxy informa o esquema original em X-Forwarded-Proto.
// Sem isso, redirecionamentos (ex.: login do painel) e o cookie seguro saem como http://.
// Confiamos em qualquer proxy porque a API só é alcançável pela rede interna do Docker (sem porta publicada).
builder.Services.Configure<ForwardedHeadersOptions>(opcoes =>
{
    opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    opcoes.KnownNetworks.Clear();
    opcoes.KnownProxies.Clear();
});

// ---------- Configurações ----------
builder.Services.Configure<LojaOptions>(builder.Configuration.GetSection(LojaOptions.Secao));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Secao));
var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();

// ---------- Banco de dados ----------
// Sem connection string (ou nos testes) a aplicação roda com banco em memória,
// para dar para testar sem instalar o PostgreSQL.
var connectionString = ConexaoBanco.Resolver(builder.Configuration);
var emTeste = builder.Environment.IsEnvironment("Testing");
var usarMemoria = emTeste || string.IsNullOrWhiteSpace(connectionString);
var nomeBancoMemoria = emTeste ? $"testes-{Guid.NewGuid()}" : "cupcakestore";

builder.Services.AddDbContext<AppDbContext>(opcoes =>
{
    if (usarMemoria) opcoes.UseInMemoryDatabase(nomeBancoMemoria);
    else opcoes.UseNpgsql(connectionString);
});

// ---------- Serviços (regras de negócio) ----------
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CupcakeService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<TokenService>();

// ---------- Autenticação ----------
// Cookie para o painel administrativo (MVC) e JWT para a API usada pelo app e pela loja web.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Conta/Login";
        opcoes.AccessDeniedPath = "/Conta/Login";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
        opcoes.Cookie.Name = "cupcake.admin";
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;
        opcoes.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // vira Secure quando o X-Forwarded-Proto é https
    })
    .AddJwtBearer(opcoes =>
    {
        opcoes.MapInboundClaims = false;
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.Chave(jwt.Key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(opcoes =>
    opcoes.AddPolicy("Admin", p => p.RequireRole(Perfis.Admin)));

// Os POSTs do painel usam [ValidateAntiForgeryToken]; a API (JSON + JWT) não usa cookies, então não precisa.
builder.Services.AddControllersWithViews(opcoes =>
{
    // Mensagens padrão do ASP.NET vêm em inglês ("The value '' is invalid."): traduzidas para o usuário.
    var msg = opcoes.ModelBindingMessageProvider;
    msg.SetValueMustNotBeNullAccessor(_ => "Preencha este campo.");
    msg.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    msg.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
    msg.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
    msg.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado para {campo} é inválido.");
    msg.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
});

var app = builder.Build();

if (usarMemoria && !emTeste)
    app.Logger.LogWarning("Nenhuma connection string configurada: usando banco EM MEMÓRIA (os dados somem ao reiniciar).");

// Cria as tabelas (se preciso) e carrega o cardápio inicial e o usuário administrador.
using (var escopo = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(escopo.ServiceProvider);
}

// ---------- Pipeline ----------
app.UseForwardedHeaders();

// Cabeçalhos de segurança em todas as respostas.
app.Use(async (contexto, proximo) =>
{
    var cabecalhos = contexto.Response.Headers;
    cabecalhos["X-Content-Type-Options"] = "nosniff";
    cabecalhos["X-Frame-Options"] = "DENY";
    cabecalhos["Referrer-Policy"] = "strict-origin-when-cross-origin";
    cabecalhos["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data: https:; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src https://fonts.gstatic.com; " +
        "script-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await proximo();
});

// HSTS só em produção (o middleware ignora requisições http).
if (!app.Environment.IsDevelopment() && !emTeste)
    app.UseHsts();

// Tratamento global de erros: a API devolve JSON padronizado; o painel mostra uma página amigável.
app.UseExceptionHandler(erroApp => erroApp.Run(async contexto =>
{
    var excecao = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
    app.Logger.LogError(excecao, "Erro não tratado em {Caminho}", contexto.Request.Path);

    if (contexto.Request.Path.StartsWithSegments("/api"))
    {
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Erro interno",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Ocorreu um erro inesperado. Tente novamente em instantes."
        });
    }
    else
    {
        contexto.Response.Redirect("/Home/Erro");
    }
}));

// Páginas amigáveis para 404/403 no site; a API continua respondendo só com o status.
app.UseWhen(
    contexto => !contexto.Request.Path.StartsWithSegments("/api"),
    site => site.UseStatusCodePagesWithReExecute("/Home/Erro", "?codigo={0}"));
app.UseDefaultFiles();   // /loja/ → /loja/index.html
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", banco = usarMemoria ? "memoria" : "postgresql" }));
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Permite que o projeto de testes suba a aplicação (WebApplicationFactory<Program>).
public partial class Program { }
