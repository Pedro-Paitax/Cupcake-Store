using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers.Api;

[Route("api/usuarios")]
public class UsuariosApiController(UsuarioService usuarios) : ApiControllerBase
{
    /// <summary>Cadastro de cliente (rota que o app do PI I já chamava).</summary>
    [HttpPost]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroUsuarioDto dto)
    {
        var resultado = await usuarios.CadastrarAsync(dto);
        if (!resultado.Sucesso) return Falha(resultado);
        return Created("/api/usuarios/me", UsuarioDto.De(resultado.Valor!));
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var usuario = await usuarios.ObterAsync(UsuarioId);
        return usuario is null ? NotFound() : Ok(UsuarioDto.De(usuario));
    }

    [HttpPut("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarUsuarioDto dto)
    {
        var resultado = await usuarios.AtualizarAsync(UsuarioId, dto);
        if (!resultado.Sucesso) return Falha(resultado);
        return Ok(UsuarioDto.De(resultado.Valor!));
    }
}
