using CupcakeStore.Api.Models;
using CupcakeStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CupcakeStore.Api.Controllers.Api;

[Route("api/cupcakes")]
public class CupcakesApiController(CupcakeService cupcakes) : ApiControllerBase
{
    /// <summary>Cardápio: somente cupcakes ativos.</summary>
    [HttpGet]
    [ProducesResponseType<List<CupcakeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var lista = await cupcakes.ListarAsync(somenteAtivos: true);
        return Ok(lista.Select(CupcakeDto.De));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<CupcakeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id)
    {
        var cupcake = await cupcakes.ObterAsync(id);
        if (cupcake is null || !cupcake.Ativo)
            return Problem(detail: "Cupcake não encontrado.", statusCode: StatusCodes.Status404NotFound, title: "Não encontrado");
        return Ok(CupcakeDto.De(cupcake));
    }
}
