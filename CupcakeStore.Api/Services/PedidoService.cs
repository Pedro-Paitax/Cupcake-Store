using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CupcakeStore.Api.Services;

public class PedidoService(AppDbContext db, IOptions<LojaOptions> opcoes)
{
    private readonly LojaOptions _loja = opcoes.Value;

    public async Task<Resultado<Pedido>> CriarAsync(int usuarioId, CriarPedidoDto dto)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado<Pedido>.Falha(TipoErro.NaoEncontrado, "Usuário não encontrado.");

        if (dto.Itens is null || dto.Itens.Count == 0)
            return Resultado<Pedido>.Falha(TipoErro.Validacao, "O carrinho está vazio.");

        if (dto.Itens.Any(i => i.Quantidade < 1))
            return Resultado<Pedido>.Falha(TipoErro.Validacao, "A quantidade de cada item deve ser pelo menos 1.");

        // Junta itens repetidos do mesmo cupcake numa linha só.
        var agrupados = dto.Itens
            .GroupBy(i => i.CupcakeId)
            .Select(g => new { CupcakeId = g.Key, Quantidade = g.Sum(i => i.Quantidade) })
            .ToList();

        if (agrupados.Any(i => i.Quantidade > _loja.QuantidadeMaximaPorItem))
            return Resultado<Pedido>.Falha(TipoErro.Validacao,
                $"A quantidade máxima por cupcake é {_loja.QuantidadeMaximaPorItem}.");

        var endereco = string.IsNullOrWhiteSpace(dto.EnderecoEntrega) ? usuario.Endereco : dto.EnderecoEntrega.Trim();
        if (string.IsNullOrWhiteSpace(endereco))
            return Resultado<Pedido>.Falha(TipoErro.Validacao, "Informe o endereço de entrega.");
        if (endereco.Length > 200)
            return Resultado<Pedido>.Falha(TipoErro.Validacao, "O endereço deve ter no máximo 200 caracteres.");

        var ids = agrupados.Select(i => i.CupcakeId).ToList();
        var cupcakes = await db.Cupcakes.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            Status = StatusPedido.Recebido,
            TaxaEntrega = _loja.TaxaEntrega,
            EnderecoEntrega = endereco
        };

        foreach (var item in agrupados)
        {
            if (!cupcakes.TryGetValue(item.CupcakeId, out var cupcake))
                return Resultado<Pedido>.Falha(TipoErro.Validacao, $"O cupcake #{item.CupcakeId} não existe.");
            if (!cupcake.Ativo)
                return Resultado<Pedido>.Falha(TipoErro.Validacao, $"{cupcake.Nome} não está mais disponível.");

            // O preço vem sempre do banco: o valor enviado pelo cliente é ignorado.
            pedido.Itens.Add(new ItemPedido
            {
                CupcakeId = cupcake.Id,
                Cupcake = cupcake,
                Quantidade = item.Quantidade,
                PrecoUnitario = cupcake.Preco
            });
        }

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        return Resultado<Pedido>.Ok(pedido);
    }

    public async Task<List<Pedido>> ListarDoUsuarioAsync(int usuarioId) =>
        await ConsultaCompleta()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public async Task<List<Pedido>> ListarAsync(StatusPedido? status = null)
    {
        var consulta = ConsultaCompleta();
        if (status.HasValue) consulta = consulta.Where(p => p.Status == status.Value);
        return await consulta.OrderByDescending(p => p.CriadoEm).ToListAsync();
    }

    public async Task<Pedido?> ObterAsync(int id) =>
        await ConsultaCompleta().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Resultado<Pedido>> AtualizarStatusAsync(int id, StatusPedido novo)
    {
        var pedido = await ConsultaCompleta().FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null)
            return Resultado<Pedido>.Falha(TipoErro.NaoEncontrado, "Pedido não encontrado.");

        if (!pedido.Status.PodeMudarPara(novo))
            return Resultado<Pedido>.Falha(TipoErro.Validacao,
                $"Não é possível mudar de \"{pedido.Status.Rotulo()}\" para \"{novo.Rotulo()}\".");

        pedido.Status = novo;
        pedido.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Resultado<Pedido>.Ok(pedido);
    }

    /// <summary>O cliente só pode cancelar o próprio pedido enquanto ele ainda não entrou em preparo.</summary>
    public async Task<Resultado<Pedido>> CancelarPeloClienteAsync(int id, int usuarioId)
    {
        var pedido = await ConsultaCompleta().FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);
        if (pedido is null)
            return Resultado<Pedido>.Falha(TipoErro.NaoEncontrado, "Pedido não encontrado.");
        if (pedido.Status != StatusPedido.Recebido)
            return Resultado<Pedido>.Falha(TipoErro.Validacao, "Só é possível cancelar pedidos que ainda não entraram em preparo.");

        pedido.Status = StatusPedido.Cancelado;
        pedido.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Resultado<Pedido>.Ok(pedido);
    }

    public async Task<DashboardViewModel> MontarDashboardAsync()
    {
        // "Hoje" no fuso de Brasília (UTC-3), convertido de volta para UTC.
        var inicioHojeUtc = DateTime.UtcNow.AddHours(-3).Date.AddHours(3);

        var naoCancelados = db.Pedidos.Where(p => p.Status != StatusPedido.Cancelado);
        var somaItens = await db.ItensPedido
            .Where(i => i.Pedido!.Status != StatusPedido.Cancelado)
            .SumAsync(i => (decimal?)(i.Quantidade * i.PrecoUnitario)) ?? 0m;
        var somaTaxas = await naoCancelados.SumAsync(p => (decimal?)p.TaxaEntrega) ?? 0m;

        var porStatus = await db.Pedidos
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToListAsync();

        var maisVendidos = await db.ItensPedido
            .Where(i => i.Pedido!.Status != StatusPedido.Cancelado)
            .GroupBy(i => i.Cupcake!.Nome)
            .Select(g => new { Nome = g.Key, Quantidade = g.Sum(i => i.Quantidade) })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalPedidos = await db.Pedidos.CountAsync(),
            PedidosHoje = await db.Pedidos.CountAsync(p => p.CriadoEm >= inicioHojeUtc),
            PedidosEmAberto = await db.Pedidos.CountAsync(p =>
                p.Status != StatusPedido.Entregue && p.Status != StatusPedido.Cancelado),
            Faturamento = somaItens + somaTaxas,
            CupcakesAtivos = await db.Cupcakes.CountAsync(c => c.Ativo),
            Clientes = await db.Usuarios.CountAsync(u => u.Perfil == Perfis.Cliente),
            PorStatus = porStatus.ToDictionary(x => x.Status, x => x.Total),
            UltimosPedidos = await ConsultaCompleta().OrderByDescending(p => p.CriadoEm).Take(8).ToListAsync(),
            MaisVendidos = maisVendidos.Select(x => (x.Nome, x.Quantidade)).ToList()
        };
    }

    private IQueryable<Pedido> ConsultaCompleta() =>
        db.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Itens).ThenInclude(i => i.Cupcake);
}
