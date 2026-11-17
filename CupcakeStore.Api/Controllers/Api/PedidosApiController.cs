using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers.Api;

[Route("api/pedidos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PedidosApiController(PedidoService pedidos) : ApiControllerBase
{
    /// <summary>Finaliza o carrinho: grava o pedido com os preços atuais do cardápio.</summary>
    [HttpPost]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto)
    {
        var resultado = await pedidos.CriarAsync(UsuarioId, dto);
        if (!resultado.Sucesso) return Falha(resultado);
        var pedido = resultado.Valor!;
        return Created($"/api/pedidos/{pedido.Id}", PedidoDto.De(pedido));
    }

    /// <summary>Histórico de pedidos do usuário logado, do mais recente para o mais antigo.</summary>
    [HttpGet("meus")]
    [ProducesResponseType<List<PedidoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Meus()
    {
        var lista = await pedidos.ListarDoUsuarioAsync(UsuarioId);
        return Ok(lista.Select(PedidoDto.De));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id)
    {
        var pedido = await pedidos.ObterAsync(id);
        // 404 também para pedido de outro cliente, para não revelar que ele existe.
        if (pedido is null || pedido.UsuarioId != UsuarioId)
            return Problem(detail: "Pedido não encontrado.", statusCode: StatusCodes.Status404NotFound, title: "Não encontrado");
        return Ok(PedidoDto.De(pedido));
    }

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType<PedidoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await pedidos.CancelarPeloClienteAsync(id, UsuarioId);
        if (!resultado.Sucesso) return Falha(resultado);
        return Ok(PedidoDto.De(resultado.Valor!));
    }
}
