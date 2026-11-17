using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;

namespace CupcakeStore.Tests;

public class PedidoServiceTests
{
    [Fact]
    public async Task Total_e_soma_dos_itens_mais_taxa_de_entrega()
    {
        using var c = new Cenario(taxaEntrega: 5m);
        var cliente = await c.CriarClienteAsync();
        var chocolate = await c.CriarCupcakeAsync("Chocolate", 8m);
        var morango = await c.CriarCupcakeAsync("Morango", 9.5m);

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto(
            [new(chocolate.Id, 2), new(morango.Id, 1)], "Rua A, 1"));

        Assert.True(r.Sucesso, r.Mensagem);
        Assert.Equal(25.5m, r.Valor!.Subtotal);   // 2×8 + 1×9,50
        Assert.Equal(5m, r.Valor.TaxaEntrega);
        Assert.Equal(30.5m, r.Valor.Total);
        Assert.Equal(StatusPedido.Recebido, r.Valor.Status);
    }

    [Fact]
    public async Task Preco_e_congelado_no_momento_da_compra()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync("Baunilha", 7m);
        var pedido = (await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"))).Valor!;

        cupcake.Preco = 12m;                       // reajuste depois da compra
        await c.Db.SaveChangesAsync();
        var salvo = await c.Pedidos.ObterAsync(pedido.Id);

        Assert.Equal(7m, salvo!.Itens.Single().PrecoUnitario);
    }

    [Fact]
    public async Task Itens_repetidos_do_mesmo_cupcake_viram_uma_linha()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto(
            [new(cupcake.Id, 2), new(cupcake.Id, 3)], "Rua A"));

        var item = Assert.Single(r.Valor!.Itens);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public async Task Carrinho_vazio_e_rejeitado()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([], "Rua A"));

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Equal("O carrinho está vazio.", r.Mensagem);
    }

    [Fact]
    public async Task Lista_de_itens_nula_e_rejeitada()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto(null, "Rua A"));

        Assert.Equal(TipoErro.Validacao, r.Erro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(51)]
    public async Task Quantidade_fora_do_limite_e_rejeitada(int quantidade)
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, quantidade)], "Rua A"));

        Assert.Equal(TipoErro.Validacao, r.Erro);
    }

    [Fact]
    public async Task Cupcake_inexistente_e_rejeitado()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(9999, 1)], "Rua A"));

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Contains("não existe", r.Mensagem);
    }

    [Fact]
    public async Task Cupcake_desativado_nao_pode_ser_comprado()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var fora = await c.CriarCupcakeAsync("Sazonal", 10m, ativo: false);

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(fora.Id, 1)], "Rua A"));

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Contains("não está mais disponível", r.Mensagem);
    }

    [Fact]
    public async Task Sem_endereco_informado_usa_o_endereco_do_cadastro()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync(endereco: "Rua do Cadastro, 10");
        var cupcake = await c.CriarCupcakeAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], null));

        Assert.Equal("Rua do Cadastro, 10", r.Valor!.EnderecoEntrega);
    }

    [Fact]
    public async Task Sem_endereco_em_lugar_nenhum_e_rejeitado()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync(endereco: null);
        var cupcake = await c.CriarCupcakeAsync();

        var r = await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "   "));

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Equal("Informe o endereço de entrega.", r.Mensagem);
    }

    [Fact]
    public async Task Cliente_so_ve_os_proprios_pedidos()
    {
        using var c = new Cenario();
        var ana = await c.CriarClienteAsync("ana@email.com");
        var bia = await c.CriarClienteAsync("bia@email.com");
        var cupcake = await c.CriarCupcakeAsync();
        await c.Pedidos.CriarAsync(ana.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"));
        await c.Pedidos.CriarAsync(ana.Id, new CriarPedidoDto([new(cupcake.Id, 2)], "Rua A"));
        await c.Pedidos.CriarAsync(bia.Id, new CriarPedidoDto([new(cupcake.Id, 3)], "Rua B"));

        var daAna = await c.Pedidos.ListarDoUsuarioAsync(ana.Id);

        Assert.Equal(2, daAna.Count);
        Assert.All(daAna, p => Assert.Equal(ana.Id, p.UsuarioId));
    }

    [Theory]
    [InlineData(StatusPedido.Recebido, StatusPedido.EmPreparo, true)]
    [InlineData(StatusPedido.EmPreparo, StatusPedido.SaiuParaEntrega, true)]
    [InlineData(StatusPedido.SaiuParaEntrega, StatusPedido.Entregue, true)]
    [InlineData(StatusPedido.Recebido, StatusPedido.Cancelado, true)]
    [InlineData(StatusPedido.Recebido, StatusPedido.Entregue, false)]
    [InlineData(StatusPedido.EmPreparo, StatusPedido.Recebido, false)]
    [InlineData(StatusPedido.Entregue, StatusPedido.Cancelado, false)]
    [InlineData(StatusPedido.Cancelado, StatusPedido.EmPreparo, false)]
    public void Fluxo_de_status_segue_a_ordem_da_cozinha(StatusPedido atual, StatusPedido novo, bool permitido)
    {
        Assert.Equal(permitido, atual.PodeMudarPara(novo));
    }

    [Fact]
    public async Task Atualizar_status_invalido_nao_altera_o_pedido()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync();
        var pedido = (await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"))).Valor!;

        var r = await c.Pedidos.AtualizarStatusAsync(pedido.Id, StatusPedido.Entregue);

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Equal(StatusPedido.Recebido, (await c.Pedidos.ObterAsync(pedido.Id))!.Status);
    }

    [Fact]
    public async Task Cliente_cancela_pedido_recebido_mas_nao_em_preparo()
    {
        using var c = new Cenario();
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync();
        var p1 = (await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"))).Valor!;
        var p2 = (await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"))).Valor!;
        await c.Pedidos.AtualizarStatusAsync(p2.Id, StatusPedido.EmPreparo);

        var cancelou = await c.Pedidos.CancelarPeloClienteAsync(p1.Id, cliente.Id);
        var naoCancelou = await c.Pedidos.CancelarPeloClienteAsync(p2.Id, cliente.Id);

        Assert.True(cancelou.Sucesso);
        Assert.Equal(StatusPedido.Cancelado, cancelou.Valor!.Status);
        Assert.Equal(TipoErro.Validacao, naoCancelou.Erro);
    }

    [Fact]
    public async Task Cliente_nao_cancela_pedido_de_outra_pessoa()
    {
        using var c = new Cenario();
        var dono = await c.CriarClienteAsync("dono@email.com");
        var outro = await c.CriarClienteAsync("outro@email.com");
        var cupcake = await c.CriarCupcakeAsync();
        var pedido = (await c.Pedidos.CriarAsync(dono.Id, new CriarPedidoDto([new(cupcake.Id, 1)], "Rua A"))).Valor!;

        var r = await c.Pedidos.CancelarPeloClienteAsync(pedido.Id, outro.Id);

        Assert.Equal(TipoErro.NaoEncontrado, r.Erro);
    }

    [Fact]
    public async Task Dashboard_ignora_pedidos_cancelados_no_faturamento()
    {
        using var c = new Cenario(taxaEntrega: 5m);
        var cliente = await c.CriarClienteAsync();
        var cupcake = await c.CriarCupcakeAsync("Chocolate", 10m);
        await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 2)], "Rua A")); // 25
        var cancelado = (await c.Pedidos.CriarAsync(cliente.Id, new CriarPedidoDto([new(cupcake.Id, 5)], "Rua A"))).Valor!;
        await c.Pedidos.AtualizarStatusAsync(cancelado.Id, StatusPedido.Cancelado);

        var d = await c.Pedidos.MontarDashboardAsync();

        Assert.Equal(25m, d.Faturamento);
        Assert.Equal(2, d.TotalPedidos);
        Assert.Equal(1, d.PedidosEmAberto);
        Assert.Equal(2, d.MaisVendidos.Single().Quantidade);
    }
}
