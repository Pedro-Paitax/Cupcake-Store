using CupcakeStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CupcakeStore.Api.Data;

/// <summary>
/// Mapeamento objeto-relacional. Os nomes de tabelas e colunas seguem exatamente
/// o script situacao-1/banco/schema.sql (snake_case), então tanto o script quanto
/// o EnsureCreated geram o mesmo banco.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cupcake> Cupcakes => Set<Cupcake>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuario", t =>
                t.HasCheckConstraint("ck_usuario_perfil", "perfil IN ('cliente', 'admin')"));
            e.HasKey(x => x.Id).HasName("pk_usuario");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_usuario_email");
            e.Property(x => x.SenhaHash).HasColumnName("senha_hash").HasMaxLength(255).IsRequired();
            e.Property(x => x.Telefone).HasColumnName("telefone").HasMaxLength(11).IsRequired();
            e.Property(x => x.Endereco).HasColumnName("endereco").HasMaxLength(200);
            e.Property(x => x.Perfil).HasColumnName("perfil").HasMaxLength(10).IsRequired();
            e.Property(x => x.CriadoEm).HasColumnName("criado_em");
        });

        mb.Entity<Cupcake>(e =>
        {
            e.ToTable("cupcake", t =>
                t.HasCheckConstraint("ck_cupcake_preco", "preco > 0"));
            e.HasKey(x => x.Id).HasName("pk_cupcake");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.Nome).IsUnique().HasDatabaseName("uq_cupcake_nome");
            e.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(500).IsRequired();
            e.Property(x => x.Preco).HasColumnName("preco").HasPrecision(10, 2);
            e.Property(x => x.Imagem).HasColumnName("imagem").HasMaxLength(300).IsRequired();
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.Property(x => x.CriadoEm).HasColumnName("criado_em");
        });

        mb.Entity<Pedido>(e =>
        {
            e.ToTable("pedido", t =>
            {
                t.HasCheckConstraint("ck_pedido_status",
                    "status IN ('Recebido', 'EmPreparo', 'SaiuParaEntrega', 'Entregue', 'Cancelado')");
                t.HasCheckConstraint("ck_pedido_taxa", "taxa_entrega >= 0");
            });
            e.HasKey(x => x.Id).HasName("pk_pedido");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TaxaEntrega).HasColumnName("taxa_entrega").HasPrecision(10, 2);
            e.Property(x => x.EnderecoEntrega).HasColumnName("endereco_entrega").HasMaxLength(200).IsRequired();
            e.Property(x => x.CriadoEm).HasColumnName("criado_em");
            e.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em");
            e.Ignore(x => x.Subtotal);
            e.Ignore(x => x.Total);
            e.Ignore(x => x.QuantidadeItens);
            e.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_pedido_usuario");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_pedido_status");

            e.HasOne(x => x.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(x => x.UsuarioId)
                .HasConstraintName("fk_pedido_usuario")
                .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<ItemPedido>(e =>
        {
            e.ToTable("item_pedido", t =>
            {
                t.HasCheckConstraint("ck_item_quantidade", "quantidade BETWEEN 1 AND 50");
                t.HasCheckConstraint("ck_item_preco", "preco_unitario > 0");
            });
            e.HasKey(x => x.Id).HasName("pk_item_pedido");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PedidoId).HasColumnName("pedido_id");
            e.Property(x => x.CupcakeId).HasColumnName("cupcake_id");
            e.Property(x => x.Quantidade).HasColumnName("quantidade");
            e.Property(x => x.PrecoUnitario).HasColumnName("preco_unitario").HasPrecision(10, 2);
            e.Ignore(x => x.Subtotal);
            e.HasIndex(x => new { x.PedidoId, x.CupcakeId }).IsUnique().HasDatabaseName("uq_item_pedido_cupcake");
            e.HasIndex(x => x.CupcakeId).HasDatabaseName("ix_item_pedido_cupcake");

            e.HasOne(x => x.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(x => x.PedidoId)
                .HasConstraintName("fk_item_pedido_pedido")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Cupcake)
                .WithMany()
                .HasForeignKey(x => x.CupcakeId)
                .HasConstraintName("fk_item_pedido_cupcake")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
