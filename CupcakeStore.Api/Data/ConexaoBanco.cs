namespace CupcakeStore.Api.Data;

public static class ConexaoBanco
{
    /// <summary>
    /// Lê a connection string de ConnectionStrings:Default ou da variável DATABASE_URL.
    /// Aceita o formato de URL usado por Neon/Render/Supabase
    /// (postgres://usuario:senha@host:5432/banco?sslmode=require) e converte para o formato do Npgsql.
    /// </summary>
    public static string? Resolver(IConfiguration config)
    {
        var valor = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(valor))
            valor = config["DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || valor.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            ? ConverterUrl(valor)
            : valor;
    }

    public static string ConverterUrl(string url)
    {
        var uri = new Uri(url);
        var credenciais = uri.UserInfo.Split(':', 2);
        var usuario = Uri.UnescapeDataString(credenciais[0]);
        var senha = credenciais.Length > 1 ? Uri.UnescapeDataString(credenciais[1]) : string.Empty;
        var banco = uri.AbsolutePath.Trim('/');
        var porta = uri.Port > 0 ? uri.Port : 5432;

        var sslMode = "Require";
        foreach (var parte in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = parte.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                sslMode = kv[1].Equals("disable", StringComparison.OrdinalIgnoreCase) ? "Disable" : "Require";
        }

        return $"Host={uri.Host};Port={porta};Database={banco};Username={usuario};Password={senha};SSL Mode={sslMode}";
    }
}
