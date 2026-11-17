namespace CupcakeStore.Api.Models;

public static class Perfis
{
    public const string Cliente = "cliente";
    public const string Admin = "admin";
}

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string Perfil { get; set; } = Perfis.Cliente;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public List<Pedido> Pedidos { get; set; } = new();
}

public class Cupcake
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    /// <summary>
    /// Nome do recurso de imagem (ex.: "chocolate") ou URL completa (https://...).
    /// O app usa o nome do recurso; o painel e a loja web resolvem para /img/{nome}.png.
    /// </summary>
    public string Imagem { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

public enum StatusPedido
{
    Recebido,
    EmPreparo,
    SaiuParaEntrega,
    Entregue,
    Cancelado
}

public class Pedido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public StatusPedido Status { get; set; } = StatusPedido.Recebido;
    public decimal TaxaEntrega { get; set; }
    public string EnderecoEntrega { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public List<ItemPedido> Itens { get; set; } = new();

    // Valores derivados: não são gravados no banco (3FN). Exigem Itens carregados.
    public decimal Subtotal => Itens.Sum(i => i.Subtotal);
    public decimal Total => Subtotal + TaxaEntrega;
    public int QuantidadeItens => Itens.Sum(i => i.Quantidade);
}

public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
    public int CupcakeId { get; set; }
    public Cupcake? Cupcake { get; set; }
    public int Quantidade { get; set; }

    /// <summary>Preço do cupcake no momento da compra (histórico).</summary>
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}

public static class StatusPedidoExtensions
{
    public static string Rotulo(this StatusPedido status) => status switch
    {
        StatusPedido.Recebido => "Recebido",
        StatusPedido.EmPreparo => "Em preparo",
        StatusPedido.SaiuParaEntrega => "Saiu para entrega",
        StatusPedido.Entregue => "Entregue",
        StatusPedido.Cancelado => "Cancelado",
        _ => status.ToString()
    };

    public static bool Finalizado(this StatusPedido status) =>
        status is StatusPedido.Entregue or StatusPedido.Cancelado;

    /// <summary>
    /// Fluxo permitido: Recebido → Em preparo → Saiu para entrega → Entregue.
    /// Qualquer pedido ainda não finalizado pode ser cancelado.
    /// </summary>
    public static bool PodeMudarPara(this StatusPedido atual, StatusPedido novo)
    {
        if (atual.Finalizado()) return false;
        if (novo == StatusPedido.Cancelado) return true;
        return (int)novo == (int)atual + 1;
    }
}
