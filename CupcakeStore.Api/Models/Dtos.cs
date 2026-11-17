namespace CupcakeStore.Api.Models;

// Os campos de entrada são anuláveis de propósito: quem valida é a camada de serviço,
// que devolve mensagens em português (ver Services/Validacoes.cs).

public record CadastroUsuarioDto(string? Nome, string? Email, string? Senha, string? Telefone, string? Endereco);

public record AtualizarUsuarioDto(string? Nome, string? Email, string? Telefone, string? Endereco);

public record LoginDto(string? Email, string? Senha);

public record UsuarioDto(int Id, string Nome, string Email, string Telefone, string? Endereco)
{
    public static UsuarioDto De(Usuario u) => new(u.Id, u.Nome, u.Email, u.Telefone, u.Endereco);
}

public record LoginRespostaDto(string Token, UsuarioDto Usuario);

public record CupcakeDto(int Id, string Nome, string Descricao, decimal Preco, string Imagem)
{
    public static CupcakeDto De(Cupcake c) => new(c.Id, c.Nome, c.Descricao, c.Preco, c.Imagem);
}

public record ItemPedidoEntradaDto(int CupcakeId, int Quantidade);

public record CriarPedidoDto(List<ItemPedidoEntradaDto>? Itens, string? EnderecoEntrega);

public record ItemPedidoDto(int CupcakeId, string Nome, string Imagem, int Quantidade, decimal PrecoUnitario, decimal Subtotal);

public record PedidoDto(
    int Id,
    string Status,
    string StatusDescricao,
    DateTime CriadoEm,
    string EnderecoEntrega,
    decimal Subtotal,
    decimal TaxaEntrega,
    decimal Total,
    List<ItemPedidoDto> Itens)
{
    public static PedidoDto De(Pedido p) => new(
        p.Id,
        p.Status.ToString(),
        p.Status.Rotulo(),
        p.CriadoEm,
        p.EnderecoEntrega,
        p.Subtotal,
        p.TaxaEntrega,
        p.Total,
        p.Itens.Select(i => new ItemPedidoDto(
            i.CupcakeId,
            i.Cupcake?.Nome ?? $"Cupcake #{i.CupcakeId}",
            i.Cupcake?.Imagem ?? string.Empty,
            i.Quantidade,
            i.PrecoUnitario,
            i.Subtotal)).ToList());
}
