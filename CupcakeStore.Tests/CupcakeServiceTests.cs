using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;

namespace CupcakeStore.Tests;

public class CupcakeServiceTests
{
    private static CupcakeFormViewModel Form(string nome = "Cupcake de Pistache", decimal preco = 12m) => new()
    {
        Nome = nome,
        Descricao = "Massa de pistache com ganache branca.",
        Preco = preco,
        Imagem = "baunilha",
        Ativo = true
    };

    [Fact]
    public async Task Cria_cupcake_valido()
    {
        using var c = new Cenario();

        var r = await c.Cupcakes.SalvarAsync(Form());

        Assert.True(r.Sucesso);
        Assert.True(r.Valor!.Id > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Preco_zero_ou_negativo_e_rejeitado(int preco)
    {
        using var c = new Cenario();

        var r = await c.Cupcakes.SalvarAsync(Form(preco: preco));

        Assert.Equal(TipoErro.Validacao, r.Erro);
    }

    [Fact]
    public async Task Nome_duplicado_e_rejeitado_ignorando_maiusculas()
    {
        using var c = new Cenario();
        await c.Cupcakes.SalvarAsync(Form("Cupcake de Pistache"));

        var r = await c.Cupcakes.SalvarAsync(Form("CUPCAKE DE PISTACHE"));

        Assert.Equal(TipoErro.Conflito, r.Erro);
    }

    [Fact]
    public async Task Editar_mantendo_o_mesmo_nome_e_permitido()
    {
        using var c = new Cenario();
        var criado = (await c.Cupcakes.SalvarAsync(Form())).Valor!;
        var edicao = Form();
        edicao.Id = criado.Id;
        edicao.Preco = 13.5m;

        var r = await c.Cupcakes.SalvarAsync(edicao);

        Assert.True(r.Sucesso);
        Assert.Equal(13.5m, r.Valor!.Preco);
    }

    [Fact]
    public async Task Desativar_tira_do_cardapio_publico()
    {
        using var c = new Cenario();
        var a = await c.CriarCupcakeAsync("A");
        await c.CriarCupcakeAsync("B");

        await c.Cupcakes.AlternarAtivoAsync(a.Id);

        Assert.Single(await c.Cupcakes.ListarAsync(somenteAtivos: true));
        Assert.Equal(2, (await c.Cupcakes.ListarAsync(somenteAtivos: false)).Count);
    }

    [Fact]
    public void Cardapio_inicial_tem_os_8_sabores_do_app()
    {
        var cardapio = DbSeeder.CardapioInicial().ToList();

        Assert.Equal(8, cardapio.Count);
        Assert.All(cardapio, x => Assert.True(x.Preco > 0));
        Assert.Equal(8, cardapio.Select(x => x.Nome).Distinct().Count());
    }
}
