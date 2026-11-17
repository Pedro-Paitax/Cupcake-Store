using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers.Api;

/// <summary>Base dos controllers REST: converte falhas de negócio em ProblemDetails (RFC 7807).</summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id do usuário autenticado, lido da claim "sub" do token JWT.</summary>
    protected int UsuarioId =>
        int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : 0;

    protected ObjectResult Falha<T>(Resultado<T> resultado)
    {
        var (status, titulo) = resultado.Erro switch
        {
            TipoErro.Validacao => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito"),
            TipoErro.NaoAutorizado => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro")
        };
        return Problem(detail: resultado.Mensagem, statusCode: status, title: titulo);
    }
}
