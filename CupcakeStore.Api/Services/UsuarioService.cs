using CupcakeStore.Api.Data;
using CupcakeStore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CupcakeStore.Api.Services;

public class UsuarioService(AppDbContext db, IPasswordHasher<Usuario> hasher)
{
    public const int SenhaMinima = 8;

    public async Task<Resultado<Usuario>> CadastrarAsync(CadastroUsuarioDto dto)
    {
        var erro = ValidarDados(dto.Nome, dto.Email, dto.Telefone, dto.Endereco);
        if (erro is not null) return Resultado<Usuario>.Falha(TipoErro.Validacao, erro);

        if (string.IsNullOrEmpty(dto.Senha) || dto.Senha.Length < SenhaMinima)
            return Resultado<Usuario>.Falha(TipoErro.Validacao, $"A senha deve ter no mínimo {SenhaMinima} caracteres.");
        if (dto.Senha.Length > 100)
            return Resultado<Usuario>.Falha(TipoErro.Validacao, "A senha deve ter no máximo 100 caracteres.");

        var email = Validacoes.NormalizarEmail(dto.Email!);
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
            return Resultado<Usuario>.Falha(TipoErro.Conflito, "Este e-mail já está em uso.");

        var usuario = new Usuario
        {
            Nome = dto.Nome!.Trim(),
            Email = email,
            Telefone = Validacoes.SomenteDigitos(dto.Telefone),
            Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim(),
            Perfil = Perfis.Cliente
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, dto.Senha);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return Resultado<Usuario>.Ok(usuario);
    }

    public async Task<Resultado<Usuario>> AutenticarAsync(string? email, string? senha)
    {
        const string invalido = "E-mail ou senha inválidos.";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
            return Resultado<Usuario>.Falha(TipoErro.Validacao, "Informe o e-mail e a senha.");

        var normalizado = Validacoes.NormalizarEmail(email);
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == normalizado);
        if (usuario is null)
            return Resultado<Usuario>.Falha(TipoErro.NaoAutorizado, invalido);

        var verificacao = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (verificacao == PasswordVerificationResult.Failed)
            return Resultado<Usuario>.Falha(TipoErro.NaoAutorizado, invalido);

        if (verificacao == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = hasher.HashPassword(usuario, senha);
            await db.SaveChangesAsync();
        }

        return Resultado<Usuario>.Ok(usuario);
    }

    public async Task<Usuario?> ObterAsync(int id) => await db.Usuarios.FindAsync(id);

    public async Task<Resultado<Usuario>> AtualizarAsync(int id, AtualizarUsuarioDto dto)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return Resultado<Usuario>.Falha(TipoErro.NaoEncontrado, "Usuário não encontrado.");

        var erro = ValidarDados(dto.Nome, dto.Email, dto.Telefone, dto.Endereco);
        if (erro is not null) return Resultado<Usuario>.Falha(TipoErro.Validacao, erro);

        var email = Validacoes.NormalizarEmail(dto.Email!);
        if (await db.Usuarios.AnyAsync(u => u.Email == email && u.Id != id))
            return Resultado<Usuario>.Falha(TipoErro.Conflito, "Este e-mail já está em uso.");

        usuario.Nome = dto.Nome!.Trim();
        usuario.Email = email;
        usuario.Telefone = Validacoes.SomenteDigitos(dto.Telefone);
        usuario.Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim();
        await db.SaveChangesAsync();
        return Resultado<Usuario>.Ok(usuario);
    }

    private static string? ValidarDados(string? nome, string? email, string? telefone, string? endereco)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            return "Informe o nome completo (mínimo 3 caracteres).";
        if (nome.Trim().Length > 100)
            return "O nome deve ter no máximo 100 caracteres.";
        if (!Validacoes.EmailValido(email))
            return "O formato do e-mail é inválido.";
        if (!Validacoes.TelefoneValido(telefone))
            return "Telefone inválido. Use DDD + número (10 ou 11 dígitos).";
        if (endereco is not null && endereco.Trim().Length > 200)
            return "O endereço deve ter no máximo 200 caracteres.";
        return null;
    }
}
