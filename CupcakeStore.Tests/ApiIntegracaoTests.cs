using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CupcakeStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace CupcakeStore.Tests;

public class AppDeTeste : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
}

/// <summary>Testes de integração: sobem a aplicação inteira (rotas, autenticação, JSON, banco) em memória.</summary>
public class ApiIntegracaoTests(AppDeTeste app) : IClassFixture<AppDeTeste>
{
    private readonly HttpClient _cliente = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static string EmailUnico() => $"cliente.{Guid.NewGuid():N}@teste.com";

    private async Task<(string Token, string Email)> CadastrarEEntrarAsync()
    {
        var email = EmailUnico();
        var cadastro = await _cliente.PostAsJsonAsync("/api/usuarios",
            new { nome = "Cliente Teste", email, senha = "senha12345", telefone = "41999990000", endereco = "Rua Teste, 1" });
        Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);

        var login = await _cliente.PostAsJsonAsync("/api/login", new { email, senha = "senha12345" });
        login.EnsureSuccessStatusCode();
        var corpo = await login.Content.ReadFromJsonAsync<LoginRespostaDto>();
        return (corpo!.Token, email);
    }

    private HttpRequestMessage ComToken(HttpMethod metodo, string url, string token, object? corpo = null)
    {
        var req = new HttpRequestMessage(metodo, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (corpo is not null) req.Content = JsonContent.Create(corpo);
        return req;
    }

    [Fact]
    public async Task Health_responde_ok()
    {
        var resp = await _cliente.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Cardapio_vem_com_os_8_cupcakes_iniciais()
    {
        var lista = await _cliente.GetFromJsonAsync<List<CupcakeDto>>("/api/cupcakes");

        Assert.NotNull(lista);
        Assert.True(lista!.Count >= 8);
        Assert.Contains(lista, c => c.Nome == "Cupcake Red Velvet" && c.Preco == 10m);
    }

    [Fact]
    public async Task Cupcake_inexistente_retorna_404()
    {
        var resp = await _cliente.GetAsync("/api/cupcakes/99999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Fluxo_completo_cadastro_login_pedido_e_historico()
    {
        var (token, _) = await CadastrarEEntrarAsync();
        var cardapio = await _cliente.GetFromJsonAsync<List<CupcakeDto>>("/api/cupcakes");
        var escolhido = cardapio!.First();

        var criar = await _cliente.SendAsync(ComToken(HttpMethod.Post, "/api/pedidos", token,
            new { itens = new[] { new { cupcakeId = escolhido.Id, quantidade = 3 } }, enderecoEntrega = "Rua Nova, 22" }));
        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);
        var pedido = await criar.Content.ReadFromJsonAsync<PedidoDto>();
        Assert.Equal(escolhido.Preco * 3 + 5m, pedido!.Total);
        Assert.Equal("Recebido", pedido.Status);

        var meus = await _cliente.SendAsync(ComToken(HttpMethod.Get, "/api/pedidos/meus", token));
        var lista = await meus.Content.ReadFromJsonAsync<List<PedidoDto>>();
        Assert.Contains(lista!, p => p.Id == pedido.Id);
    }

    [Fact]
    public async Task Cadastro_com_email_repetido_retorna_409_com_mensagem()
    {
        var (_, email) = await CadastrarEEntrarAsync();

        var resp = await _cliente.PostAsJsonAsync("/api/usuarios",
            new { nome = "Outra Pessoa", email, senha = "senha12345", telefone = "41988887777" });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        var problema = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Este e-mail já está em uso.", problema!.Detail);
    }

    [Fact]
    public async Task Cadastro_invalido_retorna_400_em_portugues()
    {
        var resp = await _cliente.PostAsJsonAsync("/api/usuarios",
            new { nome = "Zé", email = "invalido", senha = "123", telefone = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var problema = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.False(string.IsNullOrWhiteSpace(problema!.Detail));
    }

    [Fact]
    public async Task Login_com_senha_errada_retorna_401()
    {
        var (_, email) = await CadastrarEEntrarAsync();

        var resp = await _cliente.PostAsJsonAsync("/api/login", new { email, senha = "errada123" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Criar_pedido_sem_token_retorna_401()
    {
        var resp = await _cliente.PostAsJsonAsync("/api/pedidos",
            new { itens = new[] { new { cupcakeId = 1, quantidade = 1 } } });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Token_adulterado_e_recusado()
    {
        var (token, _) = await CadastrarEEntrarAsync();

        var resp = await _cliente.SendAsync(ComToken(HttpMethod.Get, "/api/pedidos/meus", token + "x"));

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Pedido_com_carrinho_vazio_retorna_400()
    {
        var (token, _) = await CadastrarEEntrarAsync();

        var resp = await _cliente.SendAsync(ComToken(HttpMethod.Post, "/api/pedidos", token,
            new { itens = Array.Empty<object>(), enderecoEntrega = "Rua X" }));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Cliente_nao_ve_pedido_de_outro_cliente()
    {
        var (tokenA, _) = await CadastrarEEntrarAsync();
        var (tokenB, _) = await CadastrarEEntrarAsync();
        var criar = await _cliente.SendAsync(ComToken(HttpMethod.Post, "/api/pedidos", tokenA,
            new { itens = new[] { new { cupcakeId = 1, quantidade = 1 } }, enderecoEntrega = "Rua A" }));
        var pedido = await criar.Content.ReadFromJsonAsync<PedidoDto>();

        var resp = await _cliente.SendAsync(ComToken(HttpMethod.Get, $"/api/pedidos/{pedido!.Id}", tokenB));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Atualizar_perfil_pela_api()
    {
        var (token, email) = await CadastrarEEntrarAsync();

        var resp = await _cliente.SendAsync(ComToken(HttpMethod.Put, "/api/usuarios/me", token,
            new { nome = "Nome Atualizado", email, telefone = "4130303030", endereco = "Rua Atualizada, 9" }));

        resp.EnsureSuccessStatusCode();
        var u = await resp.Content.ReadFromJsonAsync<UsuarioDto>();
        Assert.Equal("Nome Atualizado", u!.Nome);
    }

    [Fact]
    public async Task Painel_exige_login_de_administrador()
    {
        var resp = await _cliente.GetAsync("/Painel");

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.Contains("/Conta/Login", resp.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Paginas_publicas_carregam()
    {
        Assert.Equal(HttpStatusCode.OK, (await _cliente.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _cliente.GetAsync("/Conta/Login")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _cliente.GetAsync("/loja/")).StatusCode);
    }
}
