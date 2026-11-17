using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace CupcakeStore.Api.Services;

public enum TipoErro
{
    Nenhum,
    Validacao,
    NaoEncontrado,
    Conflito,
    NaoAutorizado
}

/// <summary>Resultado de uma operação de negócio: sucesso com valor, ou falha com tipo e mensagem.</summary>
public class Resultado<T>
{
    public bool Sucesso { get; private init; }
    public T? Valor { get; private init; }
    public TipoErro Erro { get; private init; }
    public string Mensagem { get; private init; } = string.Empty;

    public static Resultado<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };

    public static Resultado<T> Falha(TipoErro erro, string mensagem) =>
        new() { Sucesso = false, Erro = erro, Mensagem = mensagem };
}

public static partial class Validacoes
{
    public static bool EmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 150) return false;
        try
        {
            var endereco = new MailAddress(email.Trim());
            return endereco.Address == email.Trim() && endereco.Host.Contains('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string SomenteDigitos(string? valor) =>
        string.IsNullOrEmpty(valor) ? string.Empty : NaoDigitos().Replace(valor, string.Empty);

    /// <summary>Telefone brasileiro com DDD: 10 (fixo) ou 11 (celular) dígitos.</summary>
    public static bool TelefoneValido(string? telefone)
    {
        var digitos = SomenteDigitos(telefone);
        return digitos.Length is 10 or 11;
    }

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigitos();
}

public static class Formatacao
{
    /// <summary>Formata em reais (R$ 1.234,56) sem depender de dados de cultura do sistema operacional.</summary>
    public static string Brl(this decimal valor)
    {
        var texto = valor.ToString("N2", CultureInfo.InvariantCulture); // 1,234.56
        texto = texto.Replace(",", "#").Replace(".", ",").Replace("#", ".");
        return "R$ " + texto;
    }

    /// <summary>Data/hora no fuso de Brasília (UTC-3).</summary>
    public static string DataHoraBr(this DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(-3).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    public static string UrlImagem(string imagem) =>
        imagem.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        imagem.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? imagem
            : $"/img/{imagem}.png";
}
