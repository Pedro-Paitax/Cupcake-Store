using CupcakeStore.Api.Data;
using CupcakeStore.Api.Services;

namespace CupcakeStore.Tests;

public class UtilitariosTests
{
    [Theory]
    [InlineData(8, "R$ 8,00")]
    [InlineData(1234.5, "R$ 1.234,50")]
    [InlineData(0.99, "R$ 0,99")]
    public void Formata_valores_em_reais(double valor, string esperado)
    {
        // xUnit não converte InlineData para decimal, por isso o parâmetro é double.
        Assert.Equal(esperado, ((decimal)valor).Brl());
    }

    [Fact]
    public void Converte_url_do_postgres_para_connection_string()
    {
        var cs = ConexaoBanco.ConverterUrl("postgres://pedro:s3nh%40@ep-cool.neon.tech:5432/cupcake?sslmode=require");

        Assert.Equal("Host=ep-cool.neon.tech;Port=5432;Database=cupcake;Username=pedro;Password=s3nh@;SSL Mode=Require", cs);
    }

    [Theory]
    [InlineData("chocolate", "/img/chocolate.png")]
    [InlineData("https://exemplo.com/foto.jpg", "https://exemplo.com/foto.jpg")]
    public void Resolve_url_da_imagem(string imagem, string esperado)
    {
        Assert.Equal(esperado, Formatacao.UrlImagem(imagem));
    }
}
