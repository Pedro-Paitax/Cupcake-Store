using System.ComponentModel.DataAnnotations;

namespace CupcakeStore.Api.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class CupcakeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 500 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "O preço deve estar entre R$ 0,01 e R$ 9.999,99.")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "Informe a imagem (nome do recurso ou URL).")]
    [StringLength(300, ErrorMessage = "A imagem deve ter no máximo 300 caracteres.")]
    public string Imagem { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public bool Edicao => Id.HasValue;
}

public class DashboardViewModel
{
    public int TotalPedidos { get; set; }
    public int PedidosHoje { get; set; }
    public int PedidosEmAberto { get; set; }
    public decimal Faturamento { get; set; }
    public int CupcakesAtivos { get; set; }
    public int Clientes { get; set; }
    public Dictionary<StatusPedido, int> PorStatus { get; set; } = new();
    public List<Pedido> UltimosPedidos { get; set; } = new();
    public List<(string Nome, int Quantidade)> MaisVendidos { get; set; } = new();
}

public class PedidosListaViewModel
{
    public StatusPedido? Filtro { get; set; }
    public List<Pedido> Pedidos { get; set; } = new();
}
