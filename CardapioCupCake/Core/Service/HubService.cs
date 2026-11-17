using CardapioCupCake.Core.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CardapioCupCake.Core.Service
{
    /// <summary>
    /// Cliente da API REST do Cupcake Store.
    /// Todos os métodos devolvem uma mensagem de erro amigável em vez de lançar exceção,
    /// para que as telas só precisem exibir o texto.
    /// </summary>
    public class HubService
    {
        // Um único HttpClient para o app inteiro (boa prática: evita esgotar sockets).
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            // O plano gratuito do Render "dorme" e pode levar ~50 s para acordar.
            Timeout = TimeSpan.FromSeconds(70)
        };

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        // ---------- Autenticação ----------

        public async Task<(bool Ok, string? Erro)> LoginAsync(string email, string senha)
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Post, "api/login", new { email, senha });
            if (resposta == null) return (false, erro);

            var dados = await resposta.Content.ReadFromJsonAsync<LoginResposta>(_json);
            if (dados?.Usuario == null || string.IsNullOrEmpty(dados.Token))
                return (false, "Resposta inválida do servidor.");

            Sessao.Iniciar(dados.Token, Converter(dados.Usuario));
            return (true, null);
        }

        public async Task<(bool Ok, string? Erro)> CadastrarAsync(CadastroModel usuario)
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Post, "api/usuarios", new
            {
                nome = usuario.Nome,
                email = usuario.Email,
                senha = usuario.Senha,
                telefone = usuario.Telefone,
                endereco = usuario.Endereco
            });
            return (resposta != null, erro);
        }

        public async Task<(bool Ok, string? Erro)> AtualizarPerfilAsync(CadastroModel usuario)
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Put, "api/usuarios/me", new
            {
                nome = usuario.Nome,
                email = usuario.Email,
                telefone = usuario.Telefone,
                endereco = usuario.Endereco
            });
            if (resposta == null) return (false, erro);

            var atualizado = await resposta.Content.ReadFromJsonAsync<UsuarioResposta>(_json);
            if (atualizado != null) Sessao.Usuario = Converter(atualizado);
            return (true, null);
        }

        // ---------- Cardápio ----------

        public async Task<(List<CupcakeModel> Lista, string? Erro)> GetCupcakesAsync()
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Get, "api/cupcakes");
            if (resposta == null) return (new List<CupcakeModel>(), erro);

            var lista = await resposta.Content.ReadFromJsonAsync<List<CupcakeResposta>>(_json) ?? new();
            return (lista.Select(c => new CupcakeModel
            {
                Id = c.Id,
                Name = c.Nome,
                Descricao = c.Descricao,
                Price = (double)c.Preco,
                Image = c.Imagem
            }).ToList(), null);
        }

        // ---------- Pedidos ----------

        public async Task<(PedidoModel? Pedido, string? Erro)> CriarPedidoAsync(IEnumerable<ItensModel> itens, string? endereco)
        {
            var corpo = new
            {
                itens = itens.Select(i => new { cupcakeId = i.Item.Id, quantidade = i.Quantity }).ToList(),
                enderecoEntrega = endereco
            };
            var (resposta, erro) = await EnviarAsync(HttpMethod.Post, "api/pedidos", corpo);
            if (resposta == null) return (null, erro);
            return (await resposta.Content.ReadFromJsonAsync<PedidoModel>(_json), null);
        }

        public async Task<(List<PedidoModel> Lista, string? Erro)> GetMeusPedidosAsync()
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Get, "api/pedidos/meus");
            if (resposta == null) return (new List<PedidoModel>(), erro);
            return (await resposta.Content.ReadFromJsonAsync<List<PedidoModel>>(_json) ?? new(), null);
        }

        public async Task<(bool Ok, string? Erro)> CancelarPedidoAsync(int id)
        {
            var (resposta, erro) = await EnviarAsync(HttpMethod.Post, $"api/pedidos/{id}/cancelar");
            return (resposta != null, erro);
        }

        // ---------- Infraestrutura ----------

        /// <summary>Envia a requisição. Sucesso → resposta; falha → mensagem pronta para mostrar ao usuário.</summary>
        private static async Task<(HttpResponseMessage? Resposta, string? Erro)> EnviarAsync(HttpMethod metodo, string rota, object? corpo = null)
        {
            try
            {
                using var requisicao = new HttpRequestMessage(metodo, rota);
                if (corpo != null)
                    requisicao.Content = JsonContent.Create(corpo, options: _json);
                if (!string.IsNullOrEmpty(Sessao.Token))
                    requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Sessao.Token);

                var resposta = await _httpClient.SendAsync(requisicao);
                if (resposta.IsSuccessStatusCode)
                    return (resposta, null);

                if (resposta.StatusCode == HttpStatusCode.Unauthorized && Sessao.Logado)
                {
                    Sessao.Encerrar();
                    return (null, "Sua sessão expirou. Faça login novamente.");
                }

                return (null, await LerMensagemDeErroAsync(resposta));
            }
            catch (TaskCanceledException)
            {
                return (null, "O servidor demorou para responder. Tente novamente em alguns segundos.");
            }
            catch (HttpRequestException)
            {
                return (null, "Não foi possível conectar ao servidor. Verifique sua internet.");
            }
            catch (Exception ex)
            {
                return (null, $"Erro inesperado: {ex.Message}");
            }
        }

        /// <summary>A API responde erros no padrão ProblemDetails; a mensagem fica em "detail".</summary>
        private static async Task<string> LerMensagemDeErroAsync(HttpResponseMessage resposta)
        {
            try
            {
                var problema = await resposta.Content.ReadFromJsonAsync<ProblemaResposta>(_json);
                if (!string.IsNullOrWhiteSpace(problema?.Detail)) return problema.Detail;
                if (!string.IsNullOrWhiteSpace(problema?.Title)) return problema.Title;
            }
            catch (JsonException) { }
            catch (NotSupportedException) { }

            return $"Erro {(int)resposta.StatusCode} ao falar com o servidor.";
        }

        private static CadastroModel Converter(UsuarioResposta u) => new()
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Telefone = u.Telefone,
            Endereco = u.Endereco ?? string.Empty
        };

        // Formatos das respostas da API
        private sealed class LoginResposta
        {
            public string Token { get; set; } = string.Empty;
            public UsuarioResposta? Usuario { get; set; }
        }

        private sealed class UsuarioResposta
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefone { get; set; } = string.Empty;
            public string? Endereco { get; set; }
        }

        private sealed class CupcakeResposta
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Descricao { get; set; } = string.Empty;
            public decimal Preco { get; set; }
            public string Imagem { get; set; } = string.Empty;
        }

        private sealed class ProblemaResposta
        {
            public string? Title { get; set; }
            public string? Detail { get; set; }
        }
    }
}
