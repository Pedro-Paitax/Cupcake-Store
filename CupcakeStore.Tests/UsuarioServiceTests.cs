using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;

namespace CupcakeStore.Tests;

public class UsuarioServiceTests
{
    [Fact]
    public async Task Cadastro_valido_grava_usuario_com_email_normalizado_e_telefone_so_digitos()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido("  Maria@Email.COM "));

        Assert.True(r.Sucesso);
        Assert.Equal("maria@email.com", r.Valor!.Email);
        Assert.Equal("41998765432", r.Valor.Telefone);
        Assert.Equal(Perfis.Cliente, r.Valor.Perfil);
    }

    [Fact]
    public async Task Senha_e_gravada_como_hash_e_nunca_em_texto_puro()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido());

        Assert.NotEqual("senhaForte123", r.Valor!.SenhaHash);
        Assert.DoesNotContain("senhaForte123", r.Valor.SenhaHash);
        Assert.True(r.Valor.SenhaHash.Length > 40);
    }

    [Fact]
    public async Task Email_repetido_retorna_conflito_mesmo_com_maiusculas()
    {
        using var c = new Cenario();
        await c.Usuarios.CadastrarAsync(Cenario.CadastroValido("joao@email.com"));

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido("JOAO@email.com"));

        Assert.False(r.Sucesso);
        Assert.Equal(TipoErro.Conflito, r.Erro);
        Assert.Equal("Este e-mail já está em uso.", r.Mensagem);
    }

    [Theory]
    [InlineData("", "O formato do e-mail é inválido.")]
    [InlineData("sem-arroba", "O formato do e-mail é inválido.")]
    [InlineData("a@semponto", "O formato do e-mail é inválido.")]
    public async Task Email_invalido_e_rejeitado(string email, string mensagem)
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido(email));

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Equal(mensagem, r.Mensagem);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("(41) 9999-999")]
    [InlineData("419998877665544")]
    public async Task Telefone_sem_10_ou_11_digitos_e_rejeitado(string telefone)
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido() with { Telefone = telefone });

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Contains("Telefone", r.Mensagem);
    }

    [Fact]
    public async Task Senha_com_menos_de_8_caracteres_e_rejeitada()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido() with { Senha = "1234567" });

        Assert.Equal(TipoErro.Validacao, r.Erro);
        Assert.Contains("8 caracteres", r.Mensagem);
    }

    [Fact]
    public async Task Nome_vazio_e_rejeitado()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.CadastrarAsync(Cenario.CadastroValido() with { Nome = "  " });

        Assert.Equal(TipoErro.Validacao, r.Erro);
    }

    [Fact]
    public async Task Login_com_senha_correta_autentica()
    {
        using var c = new Cenario();
        await c.CriarClienteAsync("ana@email.com");

        var r = await c.Usuarios.AutenticarAsync("ANA@email.com", "senhaForte123");

        Assert.True(r.Sucesso);
        Assert.Equal("ana@email.com", r.Valor!.Email);
    }

    [Theory]
    [InlineData("ana@email.com", "senhaErrada")]
    [InlineData("naoexiste@email.com", "senhaForte123")]
    public async Task Login_invalido_nao_revela_se_o_email_existe(string email, string senha)
    {
        using var c = new Cenario();
        await c.CriarClienteAsync("ana@email.com");

        var r = await c.Usuarios.AutenticarAsync(email, senha);

        Assert.Equal(TipoErro.NaoAutorizado, r.Erro);
        Assert.Equal("E-mail ou senha inválidos.", r.Mensagem);
    }

    [Fact]
    public async Task Login_sem_dados_retorna_validacao()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.AutenticarAsync("", "");

        Assert.Equal(TipoErro.Validacao, r.Erro);
    }

    [Fact]
    public async Task Atualizar_perfil_altera_dados()
    {
        using var c = new Cenario();
        var u = await c.CriarClienteAsync();

        var r = await c.Usuarios.AtualizarAsync(u.Id,
            new AtualizarUsuarioDto("Maria S. Lima", "maria.lima@email.com", "4133334444", "Av. Batel, 50"));

        Assert.True(r.Sucesso);
        Assert.Equal("Maria S. Lima", r.Valor!.Nome);
        Assert.Equal("Av. Batel, 50", r.Valor.Endereco);
    }

    [Fact]
    public async Task Atualizar_para_email_de_outro_usuario_retorna_conflito()
    {
        using var c = new Cenario();
        await c.CriarClienteAsync("primeiro@email.com");
        var segundo = await c.CriarClienteAsync("segundo@email.com");

        var r = await c.Usuarios.AtualizarAsync(segundo.Id,
            new AtualizarUsuarioDto("Segundo", "primeiro@email.com", "41999998888", null));

        Assert.Equal(TipoErro.Conflito, r.Erro);
    }

    [Fact]
    public async Task Atualizar_usuario_inexistente_retorna_nao_encontrado()
    {
        using var c = new Cenario();

        var r = await c.Usuarios.AtualizarAsync(999,
            new AtualizarUsuarioDto("Fulano", "fulano@email.com", "41999998888", null));

        Assert.Equal(TipoErro.NaoEncontrado, r.Erro);
    }
}
