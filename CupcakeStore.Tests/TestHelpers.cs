using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CupcakeStore.Tests;

/// <summary>Monta um banco em memória isolado por teste, com os serviços reais.</summary>
public sealed class Cenario : IDisposable
{
    public AppDbContext Db { get; }
    public UsuarioService Usuarios { get; }
    public CupcakeService Cupcakes { get; }
    public PedidoService Pedidos { get; }

    public Cenario(decimal taxaEntrega = 5m)
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"teste-{Guid.NewGuid()}")
            .Options;
        Db = new AppDbContext(opcoes);
        Usuarios = new UsuarioService(Db, new PasswordHasher<Usuario>());
        Cupcakes = new CupcakeService(Db);
        Pedidos = new PedidoService(Db, Options.Create(new LojaOptions { TaxaEntrega = taxaEntrega, QuantidadeMaximaPorItem = 50 }));
    }

    public static CadastroUsuarioDto CadastroValido(string email = "maria@email.com") =>
        new("Maria Souza", email, "senhaForte123", "(41) 99876-5432", "Rua das Flores, 100 - Centro");

    public async Task<Usuario> CriarClienteAsync(string email = "maria@email.com", string? endereco = "Rua das Flores, 100")
    {
        var r = await Usuarios.CadastrarAsync(CadastroValido(email) with { Endereco = endereco });
        Assert.True(r.Sucesso, r.Mensagem);
        return r.Valor!;
    }

    public async Task<Cupcake> CriarCupcakeAsync(string nome = "Cupcake de Teste", decimal preco = 8m, bool ativo = true)
    {
        var c = new Cupcake { Nome = nome, Descricao = "Descrição de teste", Preco = preco, Imagem = "chocolate", Ativo = ativo };
        Db.Cupcakes.Add(c);
        await Db.SaveChangesAsync();
        return c;
    }

    public void Dispose() => Db.Dispose();
}
