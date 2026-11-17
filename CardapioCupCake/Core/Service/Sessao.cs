using CardapioCupCake.Core.Models;

namespace CardapioCupCake.Core.Service
{
    /// <summary>Usuário logado e token JWT enquanto o app está aberto.</summary>
    public static class Sessao
    {
        public static string? Token { get; private set; }
        public static CadastroModel? Usuario { get; set; }

        public static bool Logado => !string.IsNullOrEmpty(Token) && Usuario != null;

        public static void Iniciar(string token, CadastroModel usuario)
        {
            Token = token;
            Usuario = usuario;
        }

        public static void Encerrar()
        {
            Token = null;
            Usuario = null;
            CarrinhoService.Limpar();
        }
    }
}
