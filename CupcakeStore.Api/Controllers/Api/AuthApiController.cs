using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers.Api;

[Route("api/login")]
public class AuthApiController(UsuarioService usuarios, TokenService tokens) : ApiControllerBase
{
    /// <summary>Autentica e devolve um token JWT para usar no cabeçalho Authorization: Bearer.</summary>
    [HttpPost]
    [ProducesResponseType<LoginRespostaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await usuarios.AutenticarAsync(dto.Email, dto.Senha);
        if (!resultado.Sucesso) return Falha(resultado);

        var usuario = resultado.Valor!;
        return Ok(new LoginRespostaDto(tokens.Gerar(usuario), UsuarioDto.De(usuario)));
    }
}
