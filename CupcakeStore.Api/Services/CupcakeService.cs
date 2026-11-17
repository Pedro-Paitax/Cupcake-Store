using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CupcakeStore.Api.Services;

public class CupcakeService(AppDbContext db)
{
    public async Task<List<Cupcake>> ListarAsync(bool somenteAtivos = true)
    {
        var consulta = db.Cupcakes.AsNoTracking();
        if (somenteAtivos) consulta = consulta.Where(c => c.Ativo);
        return await consulta.OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<Cupcake?> ObterAsync(int id) => await db.Cupcakes.FindAsync(id);

    public async Task<Resultado<Cupcake>> SalvarAsync(CupcakeFormViewModel form)
    {
        var nome = form.Nome?.Trim() ?? string.Empty;
        if (nome.Length < 3)
            return Resultado<Cupcake>.Falha(TipoErro.Validacao, "O nome deve ter pelo menos 3 caracteres.");
        if (form.Preco <= 0)
            return Resultado<Cupcake>.Falha(TipoErro.Validacao, "O preço deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(form.Descricao))
            return Resultado<Cupcake>.Falha(TipoErro.Validacao, "A descrição é obrigatória.");
        if (string.IsNullOrWhiteSpace(form.Imagem))
            return Resultado<Cupcake>.Falha(TipoErro.Validacao, "Informe a imagem.");

        var nomeMinusculo = nome.ToLower();
        if (await db.Cupcakes.AnyAsync(c => c.Nome.ToLower() == nomeMinusculo && c.Id != (form.Id ?? 0)))
            return Resultado<Cupcake>.Falha(TipoErro.Conflito, "Já existe um cupcake com esse nome.");

        Cupcake? cupcake;
        if (form.Id.HasValue)
        {
            cupcake = await db.Cupcakes.FindAsync(form.Id.Value);
            if (cupcake is null) return Resultado<Cupcake>.Falha(TipoErro.NaoEncontrado, "Cupcake não encontrado.");
        }
        else
        {
            cupcake = new Cupcake();
            db.Cupcakes.Add(cupcake);
        }

        cupcake.Nome = nome;
        cupcake.Descricao = form.Descricao.Trim();
        cupcake.Preco = decimal.Round(form.Preco, 2);
        cupcake.Imagem = form.Imagem.Trim();
        cupcake.Ativo = form.Ativo;

        await db.SaveChangesAsync();
        return Resultado<Cupcake>.Ok(cupcake);
    }

    /// <summary>
    /// Cupcakes não são excluídos (estão ligados a pedidos antigos); são desativados
    /// e somem do cardápio.
    /// </summary>
    public async Task<Resultado<Cupcake>> AlternarAtivoAsync(int id)
    {
        var cupcake = await db.Cupcakes.FindAsync(id);
        if (cupcake is null) return Resultado<Cupcake>.Falha(TipoErro.NaoEncontrado, "Cupcake não encontrado.");
        cupcake.Ativo = !cupcake.Ativo;
        await db.SaveChangesAsync();
        return Resultado<Cupcake>.Ok(cupcake);
    }
}
