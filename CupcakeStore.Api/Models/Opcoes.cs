namespace CupcakeStore.Api.Models;

public class LojaOptions
{
    public const string Secao = "Loja";

    public decimal TaxaEntrega { get; set; } = 5.00m;
    public int QuantidadeMaximaPorItem { get; set; } = 50;
}

public class JwtOptions
{
    public const string Secao = "Jwt";

    /// <summary>Em produção, defina a variável de ambiente Jwt__Key (mínimo 32 caracteres).</summary>
    public string Key { get; set; } = "chave-de-desenvolvimento-cupcake-store-troque-em-producao-2026";
    public string Issuer { get; set; } = "CupcakeStore";
    public string Audience { get; set; } = "CupcakeStoreApp";
    public int ExpiracaoDias { get; set; } = 7;
}
