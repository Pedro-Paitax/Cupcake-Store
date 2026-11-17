using CupcakeStore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CupcakeStore.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var hasher = services.GetRequiredService<IPasswordHasher<Usuario>>();

        // Cria as tabelas se o banco estiver vazio (equivalente a rodar schema.sql).
        await db.Database.EnsureCreatedAsync();

        if (!await db.Cupcakes.AnyAsync())
        {
            db.Cupcakes.AddRange(CardapioInicial());
        }

        var adminEmail = (config["Admin:Email"] ?? "admin@cupcakestore.com").Trim().ToLowerInvariant();
        if (!await db.Usuarios.AnyAsync(u => u.Email == adminEmail))
        {
            var admin = new Usuario
            {
                Nome = "Administrador",
                Email = adminEmail,
                Telefone = "41999999999",
                Endereco = "Loja Cupcake Store",
                Perfil = Perfis.Admin
            };
            admin.SenhaHash = hasher.HashPassword(admin, config["Admin:Senha"] ?? "Admin@12345");
            db.Usuarios.Add(admin);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Mesmo cardápio que estava fixo no app do PI I.</summary>
    public static IEnumerable<Cupcake> CardapioInicial() =>
    [
        new() { Nome = "Cupcake de Chocolate", Preco = 8.00m, Imagem = "chocolate",
                Descricao = "Um bolo fofo de chocolate amargo, coberto com ganache de chocolate cremoso e raspas de cacau." },
        new() { Nome = "Cupcake de Morango", Preco = 9.00m, Imagem = "morango",
                Descricao = "Massa de baunilha com pedaços de morango, recheado com geleia de morango caseira e cobertura de chantilly de morango." },
        new() { Nome = "Cupcake de Baunilha", Preco = 7.00m, Imagem = "baunilha",
                Descricao = "A receita original, com massa aerada de baunilha e uma suave cobertura de buttercream de baunilha." },
        new() { Nome = "Cupcake Red Velvet", Preco = 10.00m, Imagem = "velvet",
                Descricao = "O famoso bolo aveludado vermelho, com um toque suave de cacau, e a tradicional cobertura de cream cheese frosting." },
        new() { Nome = "Cupcake de Limão Siciliano", Preco = 9.00m, Imagem = "limao",
                Descricao = "Bolo com raspas e sumo de limão siciliano, com recheio ácido e doce, e cobertura de merengue suíço tostado." },
        new() { Nome = "Cupcake de Doce de Leite", Preco = 11.00m, Imagem = "doce",
                Descricao = "Massa úmida de baunilha com coco ralado, recheada com doce de leite argentino e finalizada com flocos de coco queimado." },
        new() { Nome = "Cupcake de Café e Caramelo Salgado", Preco = 10.00m, Imagem = "caramelo",
                Descricao = "Bolo de café intenso, coberto com um buttercream de caramelo salgado e um grão de café coberto com chocolate." },
        new() { Nome = "Cupcake de Cenoura", Preco = 9.00m, Imagem = "cenoura",
                Descricao = "Massa de cenoura com canela e nozes picadas, com cobertura densa e cremosa de cream cheese e um toque de especiarias." }
    ];
}
